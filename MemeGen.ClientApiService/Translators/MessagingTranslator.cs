using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.Domain.Entities;

namespace MemeGen.ClientApiService.Translators;

public static class MessagingTranslator
{
    public static ImageProcessingConfig ToMessagingConfig(this ImageGenerationConfiguration config)
        => new(config.TextPadding, config.BackgroundOpacity, config.TextAtTop);
}