using MongoDB.Bson;

namespace MemeGen.Domain.Entities;

public enum ImageGenerationStatus
{
    InProgress = 0,
    Completed = 1,
    Failed = 2
}

public class ImageGeneration(string correlationId, string quote, string templateId, string configurationThumbprint, int personId)
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