using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeGen.AzureBlobServices;

public static class Extensions
{
    public static void AddAzureBlobServices(this IHostApplicationBuilder builder,
        string connectionName = "photocontainer")
    {
        builder.AddAzureBlobServiceClient(connectionName);

        builder.Services.AddSingleton<IAzureBlobService, AzureBlobService>();
    }
}