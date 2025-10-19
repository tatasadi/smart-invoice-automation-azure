using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvoiceAutomation.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Add Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;

        // Register Blob Storage Service
        services.AddSingleton<IBlobStorageService>(sp =>
        {
            var connectionString = configuration["AzureStorageConnectionString"]
                ?? throw new InvalidOperationException("AzureStorageConnectionString not configured");
            var containerName = configuration["AzureStorageContainerName"] ?? "invoices";
            var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

            return new BlobStorageService(connectionString, containerName, logger);
        });

        // Register Form Recognizer Service
        services.AddSingleton<IFormRecognizerService>(sp =>
        {
            var endpoint = configuration["FormRecognizerEndpoint"]
                ?? throw new InvalidOperationException("FormRecognizerEndpoint not configured");
            var key = configuration["FormRecognizerKey"]
                ?? throw new InvalidOperationException("FormRecognizerKey not configured");
            var logger = sp.GetRequiredService<ILogger<FormRecognizerService>>();

            return new FormRecognizerService(endpoint, key, logger);
        });

        // Register OpenAI Service
        services.AddSingleton<IOpenAIService>(sp =>
        {
            var endpoint = configuration["AzureOpenAIEndpoint"]
                ?? throw new InvalidOperationException("AzureOpenAIEndpoint not configured");
            var key = configuration["AzureOpenAIKey"]
                ?? throw new InvalidOperationException("AzureOpenAIKey not configured");
            var deploymentName = configuration["AzureOpenAIDeploymentName"] ?? "gpt-4o";
            var logger = sp.GetRequiredService<ILogger<OpenAIService>>();

            return new OpenAIService(endpoint, key, deploymentName, logger);
        });

        // Register Cosmos DB Service
        services.AddSingleton<ICosmosDbService>(sp =>
        {
            var endpoint = configuration["CosmosDbEndpoint"]
                ?? throw new InvalidOperationException("CosmosDbEndpoint not configured");
            var key = configuration["CosmosDbKey"]
                ?? throw new InvalidOperationException("CosmosDbKey not configured");
            var databaseName = configuration["CosmosDbDatabaseName"] ?? "InvoiceDB";
            var containerName = configuration["CosmosDbContainerName"] ?? "Invoices";
            var logger = sp.GetRequiredService<ILogger<CosmosDbService>>();

            return new CosmosDbService(endpoint, key, databaseName, containerName, logger);
        });
    })
    .Build();

host.Run();
