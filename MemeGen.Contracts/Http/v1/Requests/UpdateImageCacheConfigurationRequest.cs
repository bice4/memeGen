namespace MemeGen.Contracts.Http.v1.Requests;

public class UpdateImageCacheConfigurationRequest(int cacheDurationInMinutes, int imageRetentionInMinutes)
{
    public int CacheDurationInMinutes { get; set; } = cacheDurationInMinutes;

    public int ImageRetentionInMinutes { get; set; } = imageRetentionInMinutes;
}
