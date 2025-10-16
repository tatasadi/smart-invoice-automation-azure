using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // TODO: Register services here
        // services.AddSingleton<IBlobStorageService, BlobStorageService>();
        // services.AddSingleton<IFormRecognizerService, FormRecognizerService>();
        // services.AddSingleton<IOpenAIService, OpenAIService>();
        // services.AddSingleton<ICosmosDbService, CosmosDbService>();
    })
    .Build();

host.Run();
