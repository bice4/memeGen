using MongoDB.Bson;

namespace MemeGen.Domain.Entities;

public enum ImageGenerationStatus
{
    InProgress = 0,
    Completed = 1,
    Failed = 2
}

/// <summary>
/// Entity representing an image generation. Contains details about the generation request, its status, and associated metadata.
/// </summary>
/// <param name="correlationId">for tracking the request. Used as a unique identifier.</param>
/// <param name="quote">the quote to be used in the image</param>
/// <param name="templateId">the template to be used in the image</param>
/// <param name="configurationThumbprint">the thumbprint of the configuration used for generation</param>
/// <param name="personId">id of the associated person</param>
public class ImageGeneration(
    string correlationId,
    string quote,
    string templateId,
    string configurationThumbprint,
    int personId)
{
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

    public string CorrelationId { get; private set; } = correlationId;

    public string Quote { get; private set; } = quote;

    public string TemplateId { get; private set; } = templateId;

    public string? BlobFileName { get; private set; }

    public string? AdditionalMessage { get; private set; }

    public ImageGenerationStatus Status { get; private set; } = ImageGenerationStatus.InProgress;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.Now;

    public string ConfigurationThumbprint { get; private set; } = configurationThumbprint;

    public int PersonId { get; private set; } = personId;

    public void Update(ImageGenerationStatus status, string? additionalMessage, string? blobFileName = null)
    {
        Status = status;
        AdditionalMessage = additionalMessage;
        BlobFileName = blobFileName;
        UpdatedAt = DateTimeOffset.Now;
    }
}