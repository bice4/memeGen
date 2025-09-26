using MemeGen.AzureBlobServices;
using MemeGen.ConfigurationService;
using MemeGen.Lcm;
using MemeGen.Lcm.Services;
using MemeGen.MongoDbService;
using MemeGen.RedisService;
using MemeGen.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddMongoDbServices();
builder.AddRedisServices();
builder.AddConfigurationService();
builder.AddAzureBlobServices();
builder.AddServiceDefaults();

builder.Services.AddHostedService<LcmWorker>();
builder.Services.AddSingleton<ILcmService, LcmService>();

var host = builder.Build();
host.Run();