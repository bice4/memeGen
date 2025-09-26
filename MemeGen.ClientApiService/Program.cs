using MemeGen.AzureBlobServices;
using MemeGen.ClientApiService.Services;
using MemeGen.Common.Services;
using MemeGen.ConfigurationService;
using MemeGen.MongoDbService;
using MemeGen.RedisService;
using MemeGen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDbServices();
builder.AddRedisServices();
builder.AddAzureBlobServices();
builder.AddConfigurationService();
builder.AddRabbitMQClient(connectionName: "rabbitmq");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IResponseBuilder, ResponseBuilder>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<IImageCache, ImageCache>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapDefaultEndpoints();
app.Run();