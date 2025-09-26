using MemeGen.ImageProcessor;
using MemeGen.ImageProcessor.Services;
using MemeGen.MongoDbService;
using MemeGen.ServiceDefaults;


var builder = Host.CreateApplicationBuilder(args);
builder.AddMongoDbServices();
builder.Services.AddHostedService<ImageProcessorWorker>();
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();

builder.AddServiceDefaults();

builder.AddRabbitMQClient(connectionName: "rabbitmq");
builder.AddAzureBlobServiceClient("photocontainer");

ImageTextDrawer.Init();

var host = builder.Build();
host.Run();