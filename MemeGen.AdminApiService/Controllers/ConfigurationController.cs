using MemeGen.ApiService.Translators;
using MemeGen.ConfigurationService;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigurationController(IConfigurationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await service.GetConfigurationAsync<ImageGenerationConfiguration>(
                                    ImageGenerationConfiguration.DefaultRowKey, cancellationToken)
                                ??
                                await service.CreateConfigurationAsync(ImageGenerationConfiguration.DefaultRowKey,
                                    ImageGenerationConfiguration.CreateDefault(), cancellationToken);

            return Ok(configuration.ToShortDto());
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Get([FromBody] UpdateImageGenerationConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await service.GetConfigurationAsync<ImageGenerationConfiguration>(
                ImageGenerationConfiguration.DefaultRowKey, cancellationToken);

            if (configuration == null)
            {
                return NotFound("Configuration not found");
            }

            configuration.Update(request.TextPadding, request.BackgroundOpacity, request.TextAtTop);
            await service.UpdateConfigurationAsync(ImageGenerationConfiguration.DefaultRowKey, configuration,
                cancellationToken);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}