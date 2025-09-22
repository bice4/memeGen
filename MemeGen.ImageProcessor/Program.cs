using MemeGen.ImageProcessor;
using MemeGen.ImageProcessor.Persistent.MongoDb;
using MemeGen.ImageProcessor.Services;
using MemeGen.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ImageProcessingWorker>();
builder.Services.AddSingleton<ITemplateRepository, TemplateRepository>();
builder.Services.AddSingleton<IImageGenerationRepository, ImageGenerationRepository>();
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();

builder.AddServiceDefaults();

builder.AddRabbitMQClient(connectionName: "rabbitmq");
builder.AddAzureBlobServiceClient("mainPhotoContainer");
builder.AddMongoDBClient(connectionName: "memeGenTemplates");

ImageTextDrawer.Init();

var host = builder.Build();
host.Run();