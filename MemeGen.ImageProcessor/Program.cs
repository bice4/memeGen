using MemeGen.AzureBlobServices;
using MemeGen.ImageProcessor;
using MemeGen.ImageProcessor.Services;
using MemeGen.MongoDbService;
using MemeGen.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddMongoDbServices();
builder.AddServiceDefaults();
builder.AddAzureBlobServices();

builder.AddRabbitMQClient(connectionName: "rabbitmq");

builder.Services.AddHostedService<ImageProcessorWorker>();
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();

ImageTextDrawer.Init();

var host = builder.Build();
host.Run();