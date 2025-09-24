using MemeGen.Contracts.Http.v1.Models;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.Domain.Entities.Configuration;

namespace MemeGen.ApiService.Translators;

public static class EntityTranslator
{
    public static QuoteShortDto ToShortDto(this QuoteItem quote)
        => new(quote.Quote, quote.Id);

    public static PhotoShortDto ToShortDto(this Photo photo)
        => new(photo.Title, photo.Id);

    public static TemplateShortDto ToShortDto(this Template template)
        => new(template.Id.ToString(), template.Name, string.Join(", ", template.Quotes), template.PhotoTitle,
            template.Usages);

    public static Template ToDomain(this CreateTemplateRequest request, string photoBlobFileName)
        => new(request.Name, request.PhotoId, photoBlobFileName, request.Quotes,
            request.PersonId, request.PersonName, request.PhotoTitle);
    
    public static ImageGenerationConfigurationShortDto ToShortDto(this ImageGenerationConfiguration configuration)
        => new(configuration.TextPadding, configuration.BackgroundOpacity, configuration.TextAtTop, configuration.UseUpperText);

    public static ImageCachingConfigurationShortDto ToShortDto(this ImageCachingConfiguration configuration)
        => new(configuration.CacheDurationInMinutes,
            configuration.ImageRetentionInMinutes);
}