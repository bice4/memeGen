using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeGen.ConfigurationService;

public static class Extensions
{
    public static void AddConfigurationService(this IHostApplicationBuilder builder)
    {
        builder.AddAzureTableServiceClient("configurations",
            configureSettings: settings => settings.DisableHealthChecks = true);
        
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    }
}