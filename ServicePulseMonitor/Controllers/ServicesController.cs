using Microsoft.AspNetCore.Mvc;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Features.Services;
using ServicePulseMonitor.Common;

namespace ServicePulseMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(IRegistrationService registrationService, ILogger<ServicesController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new service with the monitoring system
    /// </summary>
    /// <param name="dto">Service registration details</param>
    /// <returns>The registered service</returns>
    /// <response code="201">Service successfully registered</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Service with this name already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceDto>> RegisterService([FromBody] CreateServiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _registrationService.RegisterServiceAsync(dto);

            return CreatedAtAction(
                nameof(GetServiceById),
                new { id = result.ServiceId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Service registration failed: {ServiceName}", dto.ServiceName);
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all services (paginated)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ServiceDto>>> GetAllServices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new { message = "Page number must be >= 1" });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Page size must be between 1 and 100" });
        }

        var result = await _registrationService.GetAllServicesAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> GetServiceById(long id)
    {
        var service = await _registrationService.GetServiceByIdAsync(id);

        if (service is null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    /// <summary>
    /// Update an existing service
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceDto>> UpdateService(long id, [FromBody] UpdateServiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _registrationService.UpdateServiceAsync(id, dto);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Service update failed: {ServiceId}", id);
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a service
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteService(long id)
    {
        var deleted = await _registrationService.DeleteServiceAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Search services by name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> SearchServices([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Search query 'q' is required" });
        }

        var services = await _registrationService.SearchServicesByNameAsync(q);
        return Ok(services);
    }

    /// <summary>
    /// Get service health summary
    /// </summary>
    [HttpGet("{id}/health")]
    [ProducesResponseType(typeof(ServiceHealthSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceHealthSummaryDto>> GetServiceHealthSummary(long id)
    {
        var summary = await _registrationService.GetServiceHealthSummaryAsync(id);

        if (summary == null)
        {
            return NotFound();
        }

        return Ok(summary);
    }
}
