using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InvoiceAutomation.Services;

namespace InvoiceAutomation.Functions;

/// <summary>
/// HTTP function for retrieving all invoices
/// </summary>
public class GetInvoices
{
    private readonly ILogger<GetInvoices> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public GetInvoices(ILogger<GetInvoices> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    [Function("GetInvoices")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoices")] HttpRequestData req)
    {
        _logger.LogInformation("Retrieving all invoices");

        try
        {
            var invoices = await _cosmosDbService.GetAllInvoicesAsync();

            _logger.LogInformation("Retrieved {Count} invoices", invoices.Count);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                count = invoices.Count,
                invoices = invoices
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new
            {
                error = "Failed to retrieve invoices",
                message = ex.Message
            });

            return errorResponse;
        }
    }
}
