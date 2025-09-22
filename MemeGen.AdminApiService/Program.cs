using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Persistent.MongoDb;
using MemeGen.ApiService.Services;
using MemeGen.Common.Services;
using MemeGen.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.AddAzureBlobServiceClient("mainPhotoContainer");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MemeGen")));

builder.AddMongoDBClient(connectionName: "memeGenTemplates");

builder.Services.AddScoped<IResponseBuilder, ResponseBuilder>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddSingleton<ITemplateRepository, TemplateRepository>();
builder.Services.AddSingleton<IImageGenerationRepository, ImageGenerationRepository>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MigrateAndSeed();

app.MapControllers();
app.MapDefaultEndpoints();
app.Run();