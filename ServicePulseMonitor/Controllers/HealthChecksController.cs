using Microsoft.AspNetCore.Mvc;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Features.HealthChecks;

namespace ServicePulseMonitor.Controllers;

[ApiController]
[Route("api/healthchecks")]
public class HealthChecksController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthChecksController> _logger;

    public HealthChecksController(
        IHealthCheckService healthCheckService,
        ILogger<HealthChecksController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _healthCheckService.SubmitHealthCheckAsync(serviceId, dto);

            return CreatedAtAction(
                nameof(GetHealthCheckById),
                new { id = result.HealthCheckId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Health check submission failed for service {ServiceId}", serviceId);
            return NotFound(new { message = ex.Message });
        }
    }

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
        var healthCheck = await _healthCheckService.GetHealthCheckByIdAsync(id);

        if (healthCheck is null)
        {
            return NotFound();
        }

        return Ok(healthCheck);
    }

    /// <summary>
    /// Get recent health checks for a specific service
    /// </summary>
    /// <param name="serviceId">Service ID</param>
    /// <param name="limit">Maximum number of results (default: 10, max: 100)</param>
    /// <returns>List of health checks</returns>
    /// <response code="200">Health checks retrieved</response>
    [HttpGet]
    [Route("/api/services/{serviceId}/healthchecks")]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetHealthChecksByService(
        long serviceId,
        [FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(new { message = "Limit must be between 1 and 100" });
        }

        var healthChecks = await _healthCheckService.GetHealthChecksByServiceIdAsync(serviceId, limit);
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
        var healthCheck = await _healthCheckService.GetLatestHealthCheckAsync(serviceId);

        if (healthCheck is null)
        {
            return NotFound(new { message = $"No health checks found for service {serviceId}" });
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
        var validStatuses = new[] { "Healthy", "Degraded", "Unhealthy" };
        if (!validStatuses.Contains(status))
        {
            return BadRequest(new
            {
                message = "Invalid status. Must be 'Healthy', 'Degraded', or 'Unhealthy'"
            });
        }

        if (limit < 1 || limit > 200)
        {
            return BadRequest(new { message = "Limit must be between 1 and 200" });
        }

        var healthChecks = await _healthCheckService.GetHealthChecksByStatusAsync(status, limit);
        return Ok(healthChecks);
    }

    /// <summary>
    /// Get all health checks (paginated)
    /// </summary>
    /// <param name="limit">Maximum number of results (default: 20, max: 100)</param>
    /// <param name="offset">Number of records to skip (default: 0)</param>
    /// <returns>List of health checks</returns>
    /// <response code="200">Health checks retrieved</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HealthCheckDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HealthCheckDto>>> GetAllHealthChecks(
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest(new { message = "Limit must be between 1 and 100" });
        }

        if (offset < 0)
        {
            return BadRequest(new { message = "Offset must be non-negative" });
        }

        var healthChecks = await _healthCheckService.GetAllHealthChecksAsync(limit, offset);
        return Ok(healthChecks);
    }
}
