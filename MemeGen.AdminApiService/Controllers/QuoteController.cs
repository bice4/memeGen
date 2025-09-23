using MemeGen.ApiService.Services;
using MemeGen.ApiService.Translators;
using MemeGen.Common.Exceptions;
using MemeGen.Common.Services;
using MemeGen.Contracts.Http.v1.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MemeGen.ApiService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuoteController(
    ILogger<QuoteController> logger,
    IQuoteService quoteService,
    IResponseBuilder responseBuilder) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var quoteItems = await quoteService.GetAllAsync(cancellationToken);
            return Ok(quoteItems);
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var quote = await quoteService.GetByIdAsync(id, cancellationToken);
            return Ok(quote);
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
            var quotes = await quoteService.GetByPersonIdAsync(personId, cancellationToken);
            return Ok(quotes.Select(EntityTranslator.ToShortDto));
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
    public async Task<IActionResult> Post([FromBody] CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating Quote with title: {Quote}", request.Quote);
            var quoteId = await quoteService.CreateAsync(request.Quote, request.PersonId,
                cancellationToken);
            return Ok(quoteId);
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

    [HttpPost("file")]
    public async Task<IActionResult> BulkPost([FromForm] IFormFile file, [FromForm] int personId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file?.Length == 0)
                return BadRequest("Files is empty");

            if (personId <= 0)
                return BadRequest("PersonId must be positive");
            
            await using var stream = file!.OpenReadStream();
            await quoteService.ImportFromFileForPersonAsync(stream, personId, cancellationToken);

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting Quote with id: {id}", id);
            await quoteService.DeleteByIdAsync(id, cancellationToken);
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