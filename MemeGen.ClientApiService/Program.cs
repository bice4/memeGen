using MemeGen.ClientApiService.Persistent.MongoDb;
using MemeGen.ClientApiService.Services;
using MemeGen.Common.Services;
using MemeGen.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IResponseBuilder, ResponseBuilder>();
builder.Services.AddSingleton<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IImageGenerationRepository, ImageGenerationRepository>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddSingleton<IImageCache, ImageCache>();

builder.AddMongoDBClient(connectionName: "memeGenTemplates");
builder.AddRedisClient(connectionName: "imageRedisCache");
builder.AddRabbitMQClient(connectionName: "rabbitmq");
builder.AddAzureBlobServiceClient("mainPhotoContainer");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapDefaultEndpoints();
app.Run();