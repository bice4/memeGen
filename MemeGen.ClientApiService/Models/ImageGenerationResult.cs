using MemeGen.Domain.Entities;

namespace MemeGen.ClientApiService.Models;

public class ImageGenerationResult(
    ImageGenerationStatus status,
    string correlationId,
    string? additionalInformation,
    string? imageBase64 = null!)
{
    public string CorrelationId { get; set; } = correlationId;

    public ImageGenerationStatus Status { get; set; } = status;

    public string? ImageBase64 { get; set; } = imageBase64;

    public string? AdditionalInformation { get; set; } = additionalInformation;
}