using SearchMS.Settings;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SearchMS.Data;
using SearchMS.Interfaces;
using SearchMS.Repositories;
using SearchMS.Services;
using SearchMS.Clients;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Load configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

// Create logger using LoggerFactory
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});
var logger = loggerFactory.CreateLogger("Startup");

// Log all configuration key-value pairs
foreach (var kvp in builder.Configuration.AsEnumerable())
{
    logger.LogInformation("Config Key: {Key} = {Value}", kvp.Key, kvp.Value);
}


try 
{
    logger.LogInformation("Starting application in {Environment} environment", builder.Environment.EnvironmentName);

    //  Environment-based Key Vault configuration
    if (builder.Environment.IsProduction())
    {
        // Production: Always use Key Vault
        var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];

        if (string.IsNullOrEmpty(keyVaultEndpoint))
        {
            throw new InvalidOperationException("Key Vault endpoint is required in production");
        }

        logger.LogInformation("Production: Configuring Azure Key Vault at {Endpoint}", keyVaultEndpoint);
        builder.Configuration
            .AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
        logger.LogInformation("Azure Key Vault configured successfully");
    }
    else
    {
        // Development: Use environment variable
        var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];
        logger.LogInformation("Development: Using local configuration (environment variables)");
    }

    // Bind settings classes to configuration sections
    builder.Services.Configure<AISearchSettings>(
        builder.Configuration.GetSection(AISearchSettings.SectionName));
    builder.Services.Configure<DatabaseSettings>(
        builder.Configuration.GetSection(DatabaseSettings.SectionName));
    logger.LogInformation("Settings configured successfully");

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

    // HTTP Client for AISearchClient
    builder.Services.AddHttpClient<IAISearchClient, AISearchClient>((serviceProvider, client) =>
    {
        var aiSettings = serviceProvider.GetRequiredService<IOptions<AISearchSettings>>().Value;

        if (string.IsNullOrEmpty(aiSettings.Endpoint))
        {
            throw new InvalidOperationException("AI Search endpoint is not configured");
        }

        if (string.IsNullOrEmpty(aiSettings.ApiKey))
        {
            throw new InvalidOperationException("AI Search API key is not configured");
        }

        client.BaseAddress = new Uri(aiSettings.Endpoint);
        client.DefaultRequestHeaders.Add("api-key", aiSettings.ApiKey);
        logger.LogInformation("HTTP Client for AISearchClient configured with endpoint {Endpoint}", aiSettings.Endpoint);
    });

    // Add application services to the container
    builder.Services.AddScoped<IHistorialBusquedaRepository, HistorialBusquedaRepository>();
    builder.Services.AddScoped<IAISearchService, AISearchService>();
    builder.Services.AddScoped<IHistorialBusquedaService, HistorialBusquedaService>();

    // Add framework services
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    // Build the application
    var app = builder.Build();
    logger.LogInformation("Application built successfully, starting configuration");

    // Configure the HTTP request pipeline
    app.MapScalarApiReference();
    logger.LogInformation("Scalar UI at /scalar/v1");

    app.UseAuthorization();
    app.MapControllers();

    logger.LogInformation("Application starting on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application failed to start");
    throw;
}

// Comentario prueba para CICD