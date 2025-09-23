namespace MemeGen.Contracts.Http.v1.Models;

public record PhotoShortDto(string Title, int Id);

public record QuoteShortDto(string Quote, int Id);

public record TemplateShortDto(string Id, string Name, string Quotes, string PhotoTitle, int Usages);

public record ImageGenerationConfigurationShortDto(float TextPadding, int BackgroundOpacity, bool TextAtTop);