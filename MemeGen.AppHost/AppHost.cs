var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume(isReadOnly: false)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var imageRedisCache = builder.AddRedis("imageRedisCache")
    .WithDataVolume()
    .WithRedisInsight()
    .WithLifetime(ContainerLifetime.Persistent);

var sqlserver = builder.AddSqlServer("sqlserver")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// var initScriptPath = Path.Join(Path.GetDirectoryName(typeof(Program).Assembly.Location), "init.sql");
// var sqlMemeGenDb = sqlserver.AddDatabase("MemeGen")
//     .WithCreationScript(File.ReadAllText(initScriptPath));

var sqlMemeGenDb = sqlserver.AddDatabase("MemeGen");

var mongo = builder.AddMongoDB("mongodb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("memeGenTemplates");

var azureBlobStorage = builder.AddAzureStorage("memeGenAzureStorage")
    .RunAsEmulator(emulator => emulator.WithDataVolume())
    .AddBlobs("mainphotocontainer");

var adminApiService = builder.AddProject<Projects.MemeGen_AdminApiService>("adminApiService")
    .WithReference(azureBlobStorage)
    .WithReference(sqlMemeGenDb)
    .WithReference(mongodb)
    .WaitFor(azureBlobStorage)
    .WaitFor(sqlMemeGenDb)
    .WaitFor(mongodb)
    .WithHttpHealthCheck("/health");

builder.AddViteApp(name: "admin-frontend", workingDirectory: "../meme-gen-admin-frontend")
    .WithReference(adminApiService)
    .WaitFor(adminApiService)
    .WithNpmPackageInstallation();

var clientApiService = builder.AddProject<Projects.MemeGen_ClientApiService>("clientApiService")
    .WithReference(mongodb)
    .WithReference(imageRedisCache)
    .WithReference(rabbitmq)
    .WithReference(azureBlobStorage)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(imageRedisCache)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

builder.AddViteApp(name: "client-frontend", workingDirectory: "../meme-gen-client-frontend")
    .WithReference(clientApiService)
    .WaitFor(clientApiService)
    .WithNpmPackageInstallation();

builder.AddProject<Projects.MemeGen_ImageProcessor>("imageProcessorService")
    .WithReference(azureBlobStorage)
    .WithReference(mongodb)
    .WithReference(rabbitmq)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.MemeGen_Lcm>("lcmWorker")
    .WithReference(azureBlobStorage)
    .WithReference(mongodb)
    .WithReference(imageRedisCache)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(imageRedisCache);

builder.Build().Run();