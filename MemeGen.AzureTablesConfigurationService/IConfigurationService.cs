using Azure.Data.Tables;
using MemeGen.Common.Constants;
using Microsoft.Extensions.Logging;

namespace MemeGen.ConfigurationService;

public interface IConfigurationService
{
    Task<T?> GetConfigurationAsync<T>(string rowKey,
        CancellationToken cancellationToken = default) where T : class, ITableEntity;

    Task UpdateConfigurationAsync<T>(string rowKey, T config, CancellationToken cancellationToken = default)
        where T : class, ITableEntity;

    Task<T> CreateConfigurationAsync<T>(string rowKey, T config, CancellationToken cancellationToken = default)
        where T : class, ITableEntity;
}

public class ConfigurationService : IConfigurationService
{
    private const string TableName = "MemeGenConfiguration";

    private readonly ILogger<ConfigurationService> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public ConfigurationService(ILogger<ConfigurationService> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;

        var tableClient = tableServiceClient.GetTableClient(TableName);
        tableClient.CreateIfNotExists();
    }

    public async Task<T?> GetConfigurationAsync<T>(string rowKey,
        CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        _logger.LogInformation("Getting configuration for row {rowKey}", rowKey);

        var tableClient = _tableServiceClient.GetTableClient(TableName);
        var configuration = await GetInternal<T>(tableClient, rowKey, cancellationToken);

        return configuration;
    }

    public async Task UpdateConfigurationAsync<T>(string rowKey, T config,
        CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        _logger.LogInformation("Updating configuration for row {rowKey}", rowKey);
        var tableClient = _tableServiceClient.GetTableClient(TableName);

        await tableClient.UpsertEntityAsync(config, TableUpdateMode.Replace, cancellationToken);

        _logger.LogInformation("Configuration for row {rowKey} updated", rowKey);
    }

    public async Task<T> CreateConfigurationAsync<T>(string rowKey, T config,
        CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        _logger.LogInformation("Creating configuration for row {rowKey}", rowKey);
        var tableClient = _tableServiceClient.GetTableClient(TableName);

        await tableClient.AddEntityAsync(config, cancellationToken);

        _logger.LogInformation("Configuration for row {rowKey} created", rowKey);
        
        return config;
    }

    private static async Task<T?> GetInternal<T>(TableClient client, string rowKey, CancellationToken cancellationToken)
        where T : class, ITableEntity
    {
        var result = await client.GetEntityIfExistsAsync<T>(
            AzureTablesConstants.DefaultPartitionKey, rowKey,
            cancellationToken: cancellationToken);

        return result.HasValue ? result.Value : null;
    }
}