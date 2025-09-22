using MemeGen.Lcm;
using MemeGen.Lcm.Persistent.MongoDb;
using MemeGen.Lcm.Services;
using MemeGen.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<LcmWorker>();
builder.Services.AddSingleton<IImageGenerationRepository, ImageGenerationRepository>();
builder.Services.AddSingleton<ILcmService, LcmService>();

builder.AddServiceDefaults();

builder.AddMongoDBClient(connectionName: "memeGenTemplates");
builder.AddRedisClient(connectionName: "imageRedisCache");
builder.AddAzureBlobServiceClient("mainPhotoContainer");

var host = builder.Build();
host.Run();