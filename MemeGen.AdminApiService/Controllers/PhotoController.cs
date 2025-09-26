using MemeGen.ApiService.Services;
using MemeGen.ApiService.Translators;
using MemeGen.Common.Exceptions;
using MemeGen.Common.Services;
using MemeGen.Contracts.Http.v1.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController(
    ILogger<PhotoController> logger,
    IPhotoService photoService,
    IResponseBuilder responseBuilder) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var photoItems = await photoService.GetAllAsync(cancellationToken);
            return Ok(photoItems);
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

    [HttpGet("person/{personId:int}")]
    public async Task<IActionResult> GetByPersonId(int personId, CancellationToken cancellationToken)
    {
        try
        {
            var photoItems = await photoService.GetAllByPersonIdAsync(personId, cancellationToken);
            return Ok(photoItems.Select(EntityTranslator.ToShortDto));
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
    
    [HttpGet("content/{id:int}")]
    public async Task<IActionResult> GetPhotoContentById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var photoItemContentBase64 = await photoService.GetPhotoContentByIdInBase64Async(id, cancellationToken);
            return Ok(photoItemContentBase64);
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

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePhotoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating Photo with title: {title}", request.Title);
            var photoItemId = await photoService.CreateAsync(request.Title, request.PersonId, request.ContentBase64,
                cancellationToken);
            return Ok(photoItemId);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting Photo with id: {id}", id);
            await photoService.DeleteByIdAsync(id, cancellationToken);
            return Ok();
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