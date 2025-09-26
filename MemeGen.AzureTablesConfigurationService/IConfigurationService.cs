using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace MemeGen.ConfigurationService;

/// <summary>
///  Service for managing configuration settings stored in Azure Table Storage.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets a configuration entry by its row key.
    /// </summary>
    /// <param name="rowKey">The row key of the configuration entry.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="T">The type of the configuration entry, must implement ITableEntity.</typeparam>
    /// <returns>The configuration entry if found, otherwise null.</returns>
    Task<T?> GetConfigurationAsync<T>(string rowKey,
        CancellationToken cancellationToken = default) where T : class, ITableEntity;

    /// <summary>
    /// Updates an existing configuration entry.
    /// </summary>
    /// <param name="rowKey">The row key of the configuration entry to update.</param>
    /// <param name="config">The updated configuration entry.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <typeparam name="T">The type of the configuration entry, must implement ITableEntity.</typeparam>
    /// <returns></returns>
    Task UpdateConfigurationAsync<T>(string rowKey, T config, CancellationToken cancellationToken = default)
        where T : class, ITableEntity;

    /// <summary>
    /// Creates a new configuration entry.
    /// </summary>
    /// <param name="rowKey">The row key of the new configuration entry.</param>
    /// <param name="config">The configuration entry to create.</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <typeparam name="T">The type of the configuration entry, must implement ITableEntity.</typeparam>
    /// <returns>The created configuration entry.</returns>
    Task<T> CreateConfigurationAsync<T>(string rowKey, T config, CancellationToken cancellationToken = default)
        where T : class, ITableEntity;
}

/// <inheritdoc/>
public class ConfigurationService : IConfigurationService
{
    private const string TableName = "MemeGenConfiguration";

    private readonly ILogger<ConfigurationService> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public ConfigurationService(ILogger<ConfigurationService> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;

        // Ensure the table exists on initialization
        var tableClient = tableServiceClient.GetTableClient(TableName);
        tableClient.CreateIfNotExists();
    }

    /// <inheritdoc/>
    public async Task<T?> GetConfigurationAsync<T>(string rowKey,
        CancellationToken cancellationToken = default) where T : class, ITableEntity
    {
        _logger.LogInformation("Getting configuration for row {rowKey}", rowKey);

        var tableClient = _tableServiceClient.GetTableClient(TableName);
        var configuration = await GetInternal<T>(tableClient, rowKey, cancellationToken);

        return configuration;
    }

    /// <inheritdoc/>
    public async Task UpdateConfigurationAsync<T>(string rowKey, T config,
        CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        _logger.LogInformation("Updating configuration for row {rowKey}", rowKey);
        
        var tableClient = _tableServiceClient.GetTableClient(TableName);
        await tableClient.UpsertEntityAsync(config, TableUpdateMode.Replace, cancellationToken);

        _logger.LogInformation("Configuration for row {rowKey} updated", rowKey);
    }

    /// <inheritdoc/>
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