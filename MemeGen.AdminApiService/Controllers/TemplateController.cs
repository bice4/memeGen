using MemeGen.ApiService.Services;
using MemeGen.ApiService.Translators;
using MemeGen.Common.Exceptions;
using MemeGen.Common.Services;
using MemeGen.Contracts.Http.v1.Requests;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class TemplateController(
    ILogger<TemplateController> logger,
    ITemplateService templateService,
    IResponseBuilder responseBuilder,
    ITemplateUpdateService templateUpdateService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var templates = await templateService.GetAllAsync(cancellationToken);
            return Ok(templates);
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

    [HttpGet("content/{personId:int}")]
    public async Task<IActionResult> GetAllContent(int personId, CancellationToken cancellationToken)
    {
        try
        {
            var allImageContentByPersonIdAsync =
                await templateService.GetAllImageContentAsync(personId, cancellationToken);
            return Ok(allImageContentByPersonIdAsync);
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

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
    {
        try
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                logger.LogWarning("Invalid template id: {Id}", id);
                return BadRequest("Invalid Id");
            }

            var template = await templateService.GetByIdAsync(objectId, cancellationToken);

            return template == null
                ? NotFound("Template not found")
                : Ok(template);
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
            var templates = await templateService.GetAllByPersonIdAsync(personId, cancellationToken);
            return Ok(templates.Select(EntityTranslator.ToShortDto));
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

    [HttpGet("updateInfo/{id}")]
    public async Task<IActionResult> GetUpdateInformation(string id, CancellationToken cancellationToken)
    {
        try
        {
            var updateInformation = await templateUpdateService.GetUpdateInformationAsync(id, cancellationToken);
            return Ok(updateInformation);
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

    [HttpGet("createInfo/{photoId:int}/{personId:int}")]
    public async Task<IActionResult> GetCreateInformation(int photoId, int personId,
        CancellationToken cancellationToken)
    {
        try
        {
            var createInformation =
                await templateUpdateService.GetCreateInformationAsync(photoId, personId, cancellationToken);
            return Ok(createInformation);
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
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await templateService.CreateAsync(request, cancellationToken);
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
    
    [HttpPatch]
    public async Task<IActionResult> Create([FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await templateService.UpdateAsync(request, cancellationToken);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                logger.LogWarning("Invalid template id: {Id}", id);
                return BadRequest("Invalid Id");
            }

            await templateService.DeleteByIdAsync(objectId, cancellationToken);
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