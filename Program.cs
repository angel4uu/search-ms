using SearchMS.Settings;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SearchMS.Data;
using SearchMS.Interfaces;
using SearchMS.Repositories;
using SearchMS.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Create logger using LoggerFactory
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger("Startup");


try
{
    logger.LogInformation("Starting application in {Environment} environment", builder.Environment.EnvironmentName);

    //Load configuration from environment variables
    builder.Configuration.AddEnvironmentVariables();

    // Bind settings classes to configuration sections
    builder.Services.Configure<AISearchSettings>(
        builder.Configuration.GetSection(AISearchSettings.SectionName));
    builder.Services.Configure<DatabaseSettings>(
        builder.Configuration.GetSection(DatabaseSettings.SectionName));
    logger.LogInformation("Settings configured successfully");

    // Configure AISearchSettings singleton with validation
    builder.Services.AddSingleton(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<AISearchSettings>>().Value;
        if (string.IsNullOrEmpty(settings.ServiceEndpoint))
        {
            throw new InvalidOperationException(
                "AISearchSettings:ServiceEndpoint is not configured. Check Docker Compose environment mapping.");
        }
        return settings;
    });

    // Configure Entity Framework with PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
        var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

        if (string.IsNullOrEmpty(dbSettings.ConnectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured");
        }

        options.UseNpgsql(dbSettings.ConnectionString);
        logger.LogInformation("Entity Framework configured with PostgreSQL");
    });

    // Add application services to the container
    builder.Services.AddScoped<IHistorialBusquedaRepository, HistorialBusquedaRepository>();
    builder.Services.AddScoped<IAISearchRepository, AISearchRepository>();
    builder.Services.AddScoped<IAISearchService, AISearchService>();
    builder.Services.AddScoped<IHistorialBusquedaService, HistorialBusquedaService>();

    // Add framework services
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    // Build the application
    var app = builder.Build();
    logger.LogInformation("Application built successfully, starting configuration");

    // Configure the HTTP request pipeline
    app.MapOpenApi();
    app.MapScalarApiReference();
    logger.LogInformation("Scalar UI at /scalar/v1");

    app.UseAuthorization();
    app.MapControllers();

    logger.LogInformation("Application starting on {Environment}", app.Environment.EnvironmentName);
    app.MapGet("/healthz", () => Results.Ok("Healthy"));
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application failed to start");
    throw;
}

