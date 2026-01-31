using Microsoft.AspNetCore.Mvc;
using ServicePulseMonitor.Data.DTOs;
using ServicePulseMonitor.Features.Services;

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
    /// Get service by ID (placeholder for Phase 2, Step 5)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> GetServiceById(long id)
    {
        var service = await _registrationService.GetServiceByIdAsync(id);

        if (service == null)
        {
            return NotFound();
        }

        return Ok(service);
    }
}
