using MemeGen.ApiService.Translators;
using MemeGen.ConfigurationService;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigurationController(ILogger<ConfigurationController> logger, IConfigurationService service)
    : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        try
        {
            // Very bad practice, but this is a demo app after all
            if (id <= 0)
            {
                logger.LogError("Invalid configuration type requested: {Id}", id);
                return BadRequest("Invalid configuration type requested");
            }

            // I'm not proud of this code, but it's a demo app after all
            switch (id)
            {
                case 2:
                {
                    var configuration = await service.GetConfigurationAsync<ImageCachingConfiguration>(
                                            ImageCachingConfiguration.DefaultRowKey, cancellationToken)
                                        ??
                                        await service.CreateConfigurationAsync(ImageCachingConfiguration.DefaultRowKey,
                                            ImageCachingConfiguration.CreateDefault(AzureTablesConstants
                                                .DefaultPartitionKey), cancellationToken);

                    return Ok(configuration.ToShortDto());
                }

                case 1:
                {
                    var configuration = await service.GetConfigurationAsync<ImageGenerationConfiguration>(
                                            ImageGenerationConfiguration.DefaultRowKey, cancellationToken)
                                        ??
                                        await service.CreateConfigurationAsync(
                                            ImageGenerationConfiguration.DefaultRowKey,
                                            ImageGenerationConfiguration.CreateDefault(AzureTablesConstants
                                                .DefaultPartitionKey), cancellationToken);

                    return Ok(configuration.ToShortDto());
                }
                default:
                {
                    return BadRequest("Unknown configuration type");
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("imageGeneration")]
    public async Task<IActionResult> UpdateImageGenerationConfiguration(
        [FromBody] UpdateImageGenerationConfigurationRequest request,
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

            configuration.Update(request.TextPadding, request.BackgroundOpacity, request.TextAtTop,
                request.UseUpperText);
            await service.UpdateConfigurationAsync(ImageGenerationConfiguration.DefaultRowKey, configuration,
                cancellationToken);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("imageCaching")]
    public async Task<IActionResult> UpdateImageCachingConfiguration(
        [FromBody] UpdateImageCacheConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await service.GetConfigurationAsync<ImageCachingConfiguration>(
                ImageCachingConfiguration.DefaultRowKey, cancellationToken);

            if (configuration == null)
            {
                return NotFound("Configuration not found");
            }

            configuration.Update(request.CacheDurationInMinutes, request.ImageRetentionInMinutes);
            await service.UpdateConfigurationAsync(ImageCachingConfiguration.DefaultRowKey, configuration,
                cancellationToken);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}