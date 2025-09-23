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

var azureBlobStorage = builder.AddAzureStorage("azureblobstorage")
    .RunAsEmulator(emulator => emulator.WithDataVolume());

 var azureBlobPhotoContainer = azureBlobStorage.AddBlobs("photocontainer");
 var azureTables = azureBlobStorage.AddTables("configurations");

var adminApiService = builder.AddProject<Projects.MemeGen_AdminApiService>("adminApiService")
    .WithReference(azureBlobPhotoContainer)
    .WithReference(azureTables)
    .WithReference(sqlMemeGenDb)
    .WithReference(mongodb)
    .WaitFor(azureBlobPhotoContainer)
    .WaitFor(sqlMemeGenDb)
    .WaitFor(mongodb)
    .WaitFor(azureTables)
    .WithHttpHealthCheck("/health");

builder.AddViteApp(name: "admin-frontend", workingDirectory: "../meme-gen-admin-frontend")
    .WithReference(adminApiService)
    .WaitFor(adminApiService)
    .WithNpmPackageInstallation();

var clientApiService = builder.AddProject<Projects.MemeGen_ClientApiService>("clientApiService")
    .WithReference(mongodb)
    .WithReference(imageRedisCache)
    .WithReference(rabbitmq)
    .WithReference(azureBlobPhotoContainer)
    .WithReference(azureTables)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(imageRedisCache)
    .WaitFor(rabbitmq)
    .WaitFor(azureTables)
    .WithHttpHealthCheck("/health");

builder.AddViteApp(name: "client-frontend", workingDirectory: "../meme-gen-client-frontend")
    .WithReference(clientApiService)
    .WaitFor(clientApiService)
    .WithNpmPackageInstallation();

builder.AddProject<Projects.MemeGen_ImageProcessor>("imageProcessorService")
    .WithReference(azureBlobPhotoContainer)
    .WithReference(mongodb)
    .WithReference(rabbitmq)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.MemeGen_Lcm>("lcmWorker")
    .WithReference(azureBlobPhotoContainer)
    .WithReference(mongodb)
    .WithReference(imageRedisCache)
    .WaitFor(azureBlobStorage)
    .WaitFor(mongodb)
    .WaitFor(imageRedisCache);

builder.Build().Run();