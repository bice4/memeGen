using MemeGen.ClientApiService.Services;
using MemeGen.Common.Services;
using MemeGen.ConfigurationService;
using MemeGen.MongoDbService;
using MemeGen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDbServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IResponseBuilder, ResponseBuilder>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddSingleton<IImageCache, ImageCache>();

builder.AddConfigurationService();

builder.AddRedisClient(connectionName: "imageRedisCache");
builder.AddRabbitMQClient(connectionName: "rabbitmq");
builder.AddAzureBlobServiceClient("photocontainer");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapDefaultEndpoints();
app.Run();