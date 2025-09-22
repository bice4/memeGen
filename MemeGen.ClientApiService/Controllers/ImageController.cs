using MemeGen.ClientApiService.Persistent.MongoDb;
using MemeGen.ClientApiService.Services;
using MemeGen.Common.Exceptions;
using MemeGen.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace MemeGen.ClientApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController(
    IResponseBuilder responseBuilder,
    IImageService imageService, 
    ITemplateRepository  templateRepository)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var persons = await templateRepository.GetPersonsFromTemplatesAsync(cancellationToken);
            return Ok(persons);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        try
        {
            var personImage = await imageService.CreateImageForPerson(id, cancellationToken);
            return Ok(personImage);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }
    
    [HttpGet("poll/{correlationId}")]
    public async Task<IActionResult> Get(string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            var imageGenResult = await imageService.GetImageForPersonByCorrelationId(correlationId, cancellationToken);
            return Ok(imageGenResult);
        }
        catch (DomainException e)
        {
            return responseBuilder.HandleDomainException(e);
        }
        catch (Exception e)
        {
            return responseBuilder.HandleException(e);
        }
    }
}