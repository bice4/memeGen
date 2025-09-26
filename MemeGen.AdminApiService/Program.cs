using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Services;
using MemeGen.AzureBlobServices;
using MemeGen.Common.Services;
using MemeGen.ConfigurationService;
using MemeGen.MongoDbService;
using MemeGen.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.AddAzureBlobServices();
builder.AddConfigurationService();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MemeGen")));

builder.AddMongoDbServices();

builder.Services.AddScoped<IResponseBuilder, ResponseBuilder>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<ITemplateUpdateService, TemplateUpdateService>();

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