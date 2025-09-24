using Azure;
using Azure.Data.Tables;

namespace MemeGen.Domain.Entities.Configuration;

public class ImageCachingConfiguration : ITableEntity
{
    public const string DefaultRowKey = "ImageCachingConfiguration";

    private const int DefaultCacheDurationInMinutes = 60;
    private const int DefaultImageRetentionInMinutes = 120;

    private const int MaxCacheDurationInMinutes = 1440;
    private const int MaxImageRetentionInMinutes = 10080;

    private const int MinCacheDurationInMinutes = 1;
    private const int MinImageRetentionInMinutes = 2;

    public int CacheDurationInMinutes { get; set; }

    public int ImageRetentionInMinutes { get; set; }

    public required string PartitionKey { get; set; }

    public string RowKey { get; set; } = DefaultRowKey;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; } = ETag.All;

    public void Update(int cacheDurationInMinutes, int imageRetentionInMinutes)
    {
        if (cacheDurationInMinutes is >= MinCacheDurationInMinutes and <= MaxCacheDurationInMinutes)
        {
            CacheDurationInMinutes = cacheDurationInMinutes;
        }

        if (imageRetentionInMinutes is >= MinImageRetentionInMinutes and <= MaxImageRetentionInMinutes)
        {
            ImageRetentionInMinutes = imageRetentionInMinutes;
        }
    }

    public static ImageCachingConfiguration CreateDefault(string partitionKey) => new()
    {
        PartitionKey = partitionKey,
        CacheDurationInMinutes = DefaultCacheDurationInMinutes,
        ImageRetentionInMinutes = DefaultImageRetentionInMinutes,
    };
}