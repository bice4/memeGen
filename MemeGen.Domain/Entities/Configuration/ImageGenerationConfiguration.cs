using Azure;
using Azure.Data.Tables;

namespace MemeGen.Domain.Entities.Configuration;

public class ImageGenerationConfiguration : ITableEntity
{
    private const int MaxTextPadding = 50;
    private const int MinTextPadding = 10;

    private const int MaxBackgroundOpacity = 210;
    private const int MinBackgroundOpacity = 80;

    private const int DefaultTextPadding = 28;
    private const int DefaultBackgroundOpacity = 120;
    private const bool DefaultTextAtTop = true;
    private const bool DefaultUseUpperText = false;

    public const string DefaultRowKey = "ImageGenerationConfiguration";

    public int TextPadding { get; set; }

    public bool TextAtTop { get;  set; }

    public int BackgroundOpacity { get; set; }
    
    public bool UseUpperText { get; set; } 

    public required string PartitionKey { get; set; }

    public string RowKey { get; set; } = DefaultRowKey;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; } = ETag.All;

    public void Update(int textPadding, int backgroundOpacity, bool textAtTop, bool useUpperText)
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
        UseUpperText = useUpperText;
    }

    public static ImageGenerationConfiguration CreateDefault(string partitionKey) => new()
    {
        TextPadding = DefaultTextPadding,
        BackgroundOpacity = DefaultBackgroundOpacity,
        TextAtTop = DefaultTextAtTop,
        UseUpperText = DefaultUseUpperText,
        PartitionKey = partitionKey
    };
    
    public string GetThumbprint()
    {
        return $"{TextPadding}-{BackgroundOpacity}-{TextAtTop}-{UseUpperText}";
    }
}