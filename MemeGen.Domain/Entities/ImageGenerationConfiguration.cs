using Azure;
using Azure.Data.Tables;
using MemeGen.Common.Constants;

namespace MemeGen.Domain.Entities;

public class ImageGenerationConfiguration : ITableEntity
{
    private const int MaxTextPadding = 50;
    private const int MinTextPadding = 10;

    private const int MaxBackgroundOpacity = 210;
    private const int MinBackgroundOpacity = 80;

    public const int DefaultTextPadding = 28;
    public const int DefaultBackgroundOpacity = 120;
    public const bool DefaultTextAtTop = true;

    public const string DefaultRowKey = "ImageGenerationConfiguration";

    public int TextPadding { get; set; }

    public bool TextAtTop { get;  set; }

    public int BackgroundOpacity { get; set; }

    public string PartitionKey { get; set; } = AzureTablesConstants.DefaultPartitionKey;

    public string RowKey { get; set; } = DefaultRowKey;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; } = ETag.All;

    public void Update(int textPadding, int backgroundOpacity, bool textAtTop)
    {
        if (backgroundOpacity is >= MinBackgroundOpacity and <= MaxBackgroundOpacity)
        {
            BackgroundOpacity = backgroundOpacity;
        }

        if (textPadding is >= MinTextPadding and <= MaxTextPadding)
        {
            TextPadding = textPadding;
        }

        TextAtTop = textAtTop;
    }

    public static ImageGenerationConfiguration CreateDefault() => new()
    {
        TextPadding = DefaultTextPadding,
        BackgroundOpacity = DefaultBackgroundOpacity,
        TextAtTop = DefaultTextAtTop,
    };
    
    public string GetThumbprint()
    {
        return $"{TextPadding}-{BackgroundOpacity}-{TextAtTop}";
    }
}