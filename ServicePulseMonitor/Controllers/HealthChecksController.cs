using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ServicePulseMonitor.Common;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Features.HealthChecks;
using ServicePulseMonitor.Features.Services;
using ServicePulseMonitor.Hubs;

namespace ServicePulseMonitor.Controllers;

/// <summary>Submits and queries health check results for monitored services.</summary>
[ApiController]
[Route("api/healthchecks")]
public class HealthChecksController(
    IHealthCheckService healthCheckService,
    IRegistrationService registrationService,
    IHubContext<HealthHub> hubContext,
    ILogger<HealthChecksController> logger)
    : ControllerBase
{
    /// <summary>
    /// Accept a self-reported health update from a monitored service identified by name.
    /// Used by the ServicePulseMonitor client library's background reporting loop.
    /// </summary>
    /// <param name="dto">Health report payload from the service.</param>
    /// <response code="200">Health report accepted.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="404">No service registered with that name.</response>
    [HttpPost]
    [Route("/api/health/report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportHealth([FromBody] HealthReportDto dto)
    {
        var service = await registrationService.GetServiceByNameAsync(dto.ServiceName);
        if (service is null)
            return NotFound($"No service registered with name '{dto.ServiceName}'.");

        var previousStatus = service.CurrentStatus;

        var createDto = new CreateHealthCheckDto
        {
            Status = dto.Status,
            ResponseTimeMs = (int)Math.Min(dto.ResponseTimeMs, int.MaxValue),
            Details = dto.Checks is { Count: > 0 }
                ? dto.Checks.ToDictionary(
                    c => c.Name,
                    c => (object)new { status = c.Status, description = c.Description })
                : null
        };

        await healthCheckService.SubmitHealthCheckAsync(service.ServiceId, createDto);

        if (previousStatus != dto.Status)
        {
            await hubContext.Clients.All.SendAsync("ServiceStatusChanged", new
            {
                serviceId = service.ServiceId,
                serviceName = service.ServiceName,
                status = dto.Status,
                responseTimeMs = (int)Math.Min(dto.ResponseTimeMs, int.MaxValue),
                timestamp = dto.CheckedAt
            });

            if (dto.Status == "Healthy")
            {
                await hubContext.Clients.All.SendAsync("AlertsResolved", new
                {
                    serviceId = service.ServiceId,
                    serviceName = service.ServiceName
                });
            }
            else
            {
                await hubContext.Clients.All.SendAsync("AlertGenerated", new
                {
                    serviceId = service.ServiceId,
                    serviceName = service.ServiceName,
                    alertType = "StatusChange",
                    message = $"{service.ServiceName} changed from {previousStatus ?? "Unknown"} to {dto.Status}.",
                    triggeredAt = dto.CheckedAt
                });
            }
        }

        return Ok();
    }

    /// <summary>
    /// Submit a health check result for a service
    /// </summary>
    /// <param name="serviceId">The service ID</param>
    /// <param name="dto">Health check details</param>
    /// <returns>The submitted health check</returns>
    /// <response code="201">Health check successfully submitted</response>
    /// <response code="400">Invalid health check data</response>
    /// <response code="404">Service not found</response>
    [HttpPost]
    [Route("/api/services/{serviceId}/healthchecks")]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthCheckDto>> SubmitHealthCheck(
        long serviceId,
        [FromBody] CreateHealthCheckDto dto)
    {
        try
        {
            var result = await healthCheckService.SubmitHealthCheckAsync(serviceId, dto);

            return CreatedAtAction(
                nameof(GetHealthCheckById),
                new { id = result.HealthCheckId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Health check submission failed for service {ServiceId}", serviceId);
            return Problem(detail: ex.Message, statusCode: 404);
        }
    }

    /// <inheritdoc cref="SubmitHealthCheck"/>
    /// <remarks>Alias for <c>POST /api/services/{serviceId}/healthchecks</c>.</remarks>
    [HttpPost]
    [Route("/api/services/{serviceId}/health")]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<HealthCheckDto>> SubmitHealthReport(
        long serviceId,
        [FromBody] CreateHealthCheckDto dto)
        => SubmitHealthCheck(serviceId, dto);

    /// <summary>
    /// Get a specific health check by ID
    /// </summary>
    /// <param name="id">Health check ID</param>
    /// <returns>Health check details</returns>
    /// <response code="200">Health check found</response>
    /// <response code="404">Health check not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthCheckDto>> GetHealthCheckById(long id)
    {
        var healthCheck = await healthCheckService.GetHealthCheckByIdAsync(id);

        if (healthCheck is null)
        {
            return NotFound();
        }

        return Ok(healthCheck);
    }

    /// <summary>
    /// Get recent health checks for a specific service, optionally filtered by time range.
    /// </summary>
    /// <param name="serviceId">Service ID</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 100)</param>
    /// <param name="from">Only return checks at or after this UTC timestamp (ISO 8601).</param>
    /// <param name="to">Only return checks at or before this UTC timestamp (ISO 8601).</param>
    /// <returns>List of health checks ordered by <c>CheckedAt</c> descending</returns>
    /// <response code="200">Health checks retrieved</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [Route("/api/services/{serviceId}/healthchecks")]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetHealthChecksByService(
        long serviceId,
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (limit < 1 || limit > 100)
            return Problem(detail: "Limit must be between 1 and 100", statusCode: 400);

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return Problem(detail: "'from' must be earlier than or equal to 'to'", statusCode: 400);

        var healthChecks = await healthCheckService.GetHealthChecksByServiceIdAsync(serviceId, limit, from, to);
        return Ok(healthChecks);
    }

    /// <summary>
    /// Get the latest health check for a specific service
    /// </summary>
    /// <param name="serviceId">Service ID</param>
    /// <returns>Latest health check</returns>
    /// <response code="200">Latest health check found</response>
    /// <response code="404">No health checks found for this service</response>
    [HttpGet]
    [Route("/api/services/{serviceId}/healthchecks/latest")]
    [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthCheckDto>> GetLatestHealthCheck(long serviceId)
    {
        var healthCheck = await healthCheckService.GetLatestHealthCheckAsync(serviceId);

        if (healthCheck is null)
        {
            return Problem(detail: $"No health checks found for service {serviceId}", statusCode: 404);
        }

        return Ok(healthCheck);
    }

    /// <summary>
    /// Get health checks filtered by status
    /// </summary>
    /// <param name="status">Status filter (Healthy, Degraded, or Unhealthy)</param>
    /// <param name="limit">Maximum number of results (default: 50, max: 200)</param>
    /// <returns>List of health checks matching the status</returns>
    /// <response code="200">Health checks retrieved</response>
    /// <response code="400">Invalid status value</response>
    [HttpGet]
    [Route("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetHealthChecksByStatus(
        string status,
        [FromQuery] int limit = 50)
    {
        if (status is not ("Healthy" or "Degraded" or "Unhealthy"))
        {
            return Problem(detail: "Invalid status. Must be 'Healthy', 'Degraded', or 'Unhealthy'", statusCode: 400);
        }

        if (limit < 1 || limit > 200)
        {
            return Problem(detail: "Limit must be between 1 and 200", statusCode: 400);
        }

        var healthChecks = await healthCheckService.GetHealthChecksByStatusAsync(status, limit);
        return Ok(healthChecks);
    }

    /// <summary>
    /// Get all health checks (paginated)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paged list of health checks</returns>
    /// <response code="200">Health checks retrieved</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<HealthCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<HealthCheckDto>>> GetAllHealthChecks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1)
        {
            return Problem(detail: "Page number must be >= 1", statusCode: 400);
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return Problem(detail: "Page size must be between 1 and 100", statusCode: 400);
        }

        var result = await healthCheckService.GetAllHealthChecksAsync(pageNumber, pageSize);
        return Ok(result);
    }
}
