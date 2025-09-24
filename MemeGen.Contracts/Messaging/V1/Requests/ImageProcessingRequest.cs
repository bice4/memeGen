namespace MemeGen.Contracts.Messaging.V1.Requests;

public class ImageProcessingRequest(
    string correlationId,
    string randomQuote,
    string randomTemplateId,
    ImageProcessingConfig imageProcessingConfig)
{
    public string CorrelationId { get; set; } = correlationId;
    public string RandomQuote { get; set; } = randomQuote;
    public string RandomTemplateId { get; set; } = randomTemplateId;

    public ImageProcessingConfig ImageProcessingConfig { get; set; } = imageProcessingConfig;
}

public record ImageProcessingConfig(int TextPadding, int BackgroundOpacity, bool TextAtTop, bool UseUpperText);