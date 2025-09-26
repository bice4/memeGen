namespace MemeGen.Contracts.Http.v1.Models;

public record PhotoShortDto(string Title, int Id);

public record QuoteShortDto(string Quote, int Id);

public record TemplateShortDto(string Id, string Name, string Quotes, string PhotoTitle, int Usages);

public record ImageGenerationConfigurationShortDto(
    float TextPadding,
    int BackgroundOpacity,
    bool TextAtTop,
    bool UseUpperText);

public record ImageCachingConfigurationShortDto(int CacheDurationInMinutes, int ImageRetentionInMinutes);

/// <summary>
/// Information needed to update an existing template.
/// </summary>
/// <param name="TemplateId"><see cref="string"/> id of the template to update</param>
/// <param name="Name"><see cref="string"/> name of the template</param>
/// <param name="TemplateQuotes"> quotes currently associated with the template</param>
/// <param name="QuotesToAdd"> quotes available to add to the template</param>
/// <param name="PhotoTitle"> title of the associated photo</param>
/// <param name="PhotoBase64"> Base64 encoded content of the associated photo</param>
public record TemplateUpdateInformation(
    string TemplateId,
    string Name,
    IEnumerable<QuoteShortDto> TemplateQuotes,
    IEnumerable<QuoteShortDto> QuotesToAdd,
    string PhotoTitle,
    string PhotoBase64);

/// <summary>
/// Information needed to create a new template.
/// </summary>
/// <param name="Quotes"> quotes available to add to the new template</param>
/// <param name="PhotoTitle"> title of the associated photo</param>
/// <param name="PhotoBase64"> Base64 encoded content of the associated photo</param>
public record TemplateCreateInformation(IEnumerable<QuoteShortDto> Quotes, string PhotoTitle, string PhotoBase64);