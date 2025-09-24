using MemeGen.Lcm;
using MemeGen.Lcm.Services;
using MemeGen.MongoDbService;
using MemeGen.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddMongoDbServices();

builder.Services.AddHostedService<LcmWorker>();
builder.Services.AddSingleton<ILcmService, LcmService>();

builder.AddServiceDefaults();

builder.AddRedisClient(connectionName: "imageRedisCache");
builder.AddAzureBlobServiceClient("photocontainer");

var host = builder.Build();
host.Run();