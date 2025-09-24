using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeGen.ConfigurationService;

public static class Extensions
{
    public static void AddConfigurationService(this IHostApplicationBuilder builder, string connectionName = "configurations")
    {
        builder.AddAzureTableServiceClient(connectionName,
            configureSettings: settings => settings.DisableHealthChecks = true);
        
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    }
}