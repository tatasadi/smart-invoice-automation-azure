using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InvoiceAutomation.Services;

namespace InvoiceAutomation.Functions;

/// <summary>
/// HTTP function for retrieving a specific invoice by ID
/// </summary>
public class GetInvoice
{
    private readonly ILogger<GetInvoice> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public GetInvoice(ILogger<GetInvoice> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    [Function("GetInvoice")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Retrieving invoice: {InvoiceId}", id);

        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invoice ID is required" });
                return badResponse;
            }

            var invoice = await _cosmosDbService.GetInvoiceByIdAsync(id);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found: {InvoiceId}", id);
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new
                {
                    error = "Invoice not found",
                    id = id
                });
                return notFoundResponse;
            }

            _logger.LogInformation("Invoice retrieved successfully: {InvoiceId}", id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(invoice);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice: {InvoiceId}", id);

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new
            {
                error = "Failed to retrieve invoice",
                message = ex.Message,
                id = id
            });

            return errorResponse;
        }
    }
}
