using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Translators;
using MemeGen.AzureBlobServices;
using MemeGen.Common.Exceptions;
using MemeGen.Contracts.Http.v1.Models;
using MemeGen.MongoDbService.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using InvalidDataException = MemeGen.Common.Exceptions.InvalidDataException;

namespace MemeGen.ApiService.Services;

/// <summary>
/// Service for retrieving information needed to create or update templates.
/// This service exists only to reduce the BE calls needed by the Admin UI when creating or updating templates.
/// </summary>
public interface ITemplateUpdateService
{
    /// <summary>
    /// Gets the information needed to update an existing template.
    /// </summary>
    /// <param name="templateId"><see cref="string"/> id of the template to update</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the template does not exist</exception>
    /// <exception cref="InvalidDataException"> thrown if the template ID format is invalid</exception>
    /// <exception cref="BlobNotFoundException"> thrown if the associated photo blob does not exist</exception>
    /// <returns><see cref="TemplateUpdateInformation"/> containing template details and available quotes</returns>
    Task<TemplateUpdateInformation> GetUpdateInformationAsync(string templateId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the information needed to create a new template.
    /// </summary>
    /// <param name="photoId"><see cref="int"/> id of the associated photo</param>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the associated person or photo does not exist</exception>
    /// <exception cref="BlobNotFoundException"> thrown if the associated photo blob does not exist</exception>
    /// <returns><see cref="TemplateCreateInformation"/> containing available quotes and photo details</returns>
    Task<TemplateCreateInformation> GetCreateInformationAsync(int photoId, int personId,
        CancellationToken cancellationToken);
}

/// <inheritdoc/>
public class TemplateUpdateService(
    ITemplateRepository templateRepository,
    AppDbContext appDbContext,
    IAzureBlobService azureBlobService) : ITemplateUpdateService
{
    /// <inheritdoc />
    public async Task<TemplateUpdateInformation> GetUpdateInformationAsync(string templateId,
        CancellationToken cancellationToken)
    {
        // Validate the template ID format
        if (!ObjectId.TryParse(templateId, out var objectId))
            throw new InvalidDataException("Invalid template ID format");

        // Retrieve the template from the repository
        var template = await templateRepository.GetByIdAsync(objectId, cancellationToken);
        if (template == null)
            throw new NotFoundException("Template", templateId);

        // Retrieve all quotes associated with the person, excluding those already in the template
        var templateQuotes = template.Quotes ?? [];
        var allQuotes = await appDbContext.Quotes.Where(x => x.PersonId == template.PersonId)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var quoteItem in templateQuotes.Where(quoteItem => allQuotes.Any(x => x.Quote == quoteItem)))
        {
            allQuotes.RemoveAll(x => x.Quote == quoteItem);
        }

        // Not great, but we need the index for the DTO and React needs a stable key, so a simple foreach will do.
        // If performance becomes an issue, we can optimize this later.
        var templateQuotesDtos = new List<QuoteShortDto>();
        var i = 1;

        foreach (var quote in templateQuotes)
        {
            templateQuotesDtos.Add(new QuoteShortDto(quote, i));
            i++;
        }

        var base64Image = await azureBlobService.GetContentInBase64Async(template.PhotoBlobFileName,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(base64Image))
            throw new BlobNotFoundException(template.PhotoBlobFileName);

        return new TemplateUpdateInformation(templateId, template.Name,
            templateQuotesDtos,
            allQuotes.Select(x => x.ToShortDto()),
            template.PhotoTitle,
            base64Image);
    }

    /// <inheritdoc />
    public async Task<TemplateCreateInformation> GetCreateInformationAsync(int photoId, int personId,
        CancellationToken cancellationToken)
    {
        // Check if the associated person exists
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == personId,
                cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", personId.ToString());

        // Check if the associated photo exists
        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == photoId,
            cancellationToken);
        if (photo == null)
            throw new NotFoundException("Photo", photoId.ToString());

        // Retrieve all quotes associated with the person
        var quotes = await appDbContext.Quotes
            .Where(x => x.PersonId == personId)
            .ToListAsync(cancellationToken);

        // Get the base64 representation of the photo from Azure Blob Storage
        // If the blob does not exist or has no content, appropriate exceptions will be thrown
        var base64Image = await azureBlobService.GetContentInBase64Async(photo.BlobFileName, cancellationToken);

        return string.IsNullOrWhiteSpace(base64Image)
            ? throw new BlobNotFoundException(photo.BlobFileName)
            : new TemplateCreateInformation(quotes.Select(x => x.ToShortDto()), photo.Title, base64Image);
    }
}