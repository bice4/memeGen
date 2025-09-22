namespace MemeGen.Contracts.Messaging.V1.Requests;

public class ImageProcessingRequest(string correlationId, string randomQuote, string randomTemplateId)
{
    public string CorrelationId { get; set; } = correlationId;
    public string RandomQuote { get; set; } = randomQuote;
    public string RandomTemplateId { get; set; } = randomTemplateId;
}