using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Domain;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Controllers;

[ApiController]
[Route("api/conversions")]
public sealed class ConversionsController : ControllerBase
{
    private readonly IConversionService _service;

    public ConversionsController(IConversionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Convert([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _service.ConvertAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Invalid request",
                Detail = ex.Message,
            });
        }
        catch (CurrencyRateProviderUnavailableException ex)
        {
            return StatusCode(503, new ProblemDetails
            {
                Status = 503,
                Title = "Currency rate provider unavailable",
                Detail = "Currency rate provider unavailable",
            });
        }
        catch (CurrencyRateNormalizationException)
        {
            return StatusCode(503, new ProblemDetails
            {
                Status = 503,
                Title = "Currency rate provider unavailable",
                Detail = "Currency rate provider unavailable",
            });
        }
    }

    [HttpGet("{auditId}")]
    public async Task<IActionResult> GetById([FromRoute] string auditId, CancellationToken cancellationToken)
    {
        var response = await _service.GetAsync(auditId, cancellationToken);
        if (response is null)
            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Audit record not found",
                Detail = $"No conversion audit record exists for auditId '{auditId}'.",
            });

        return Ok(response);
    }
}
