namespace MemeGen.ClientApiService.Models;

public record PersonImage(string CorrelationId, bool cached, string? cachedImage = null);