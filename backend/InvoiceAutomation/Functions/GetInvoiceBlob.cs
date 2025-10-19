using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InvoiceAutomation.Services;

namespace InvoiceAutomation.Functions;

/// <summary>
/// Azure Function to retrieve invoice blob with SAS token
/// </summary>
public class GetInvoiceBlob
{
    private readonly IBlobStorageService _blobService;
    private readonly ILogger<GetInvoiceBlob> _logger;

    public GetInvoiceBlob(IBlobStorageService blobService, ILogger<GetInvoiceBlob> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/blob/sas?blobUrl={blobUrl}
    /// Returns a SAS URL for accessing the blob
    /// </summary>
    [Function("GetInvoiceBlobSasUrl")]
    public async Task<HttpResponseData> GetSasUrl(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blob/sas")] HttpRequestData req)
    {
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var blobUrl = query["blobUrl"];

            if (string.IsNullOrEmpty(blobUrl))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Missing blobUrl parameter" });
                return badResponse;
            }

            _logger.LogInformation("Generating SAS URL for blob: {BlobUrl}", blobUrl);

            // Generate SAS URL with 60-minute expiration
            var sasUrl = await _blobService.GenerateSasUrlAsync(blobUrl, expirationMinutes: 60);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { sasUrl, expiresInMinutes = 60 });
            return response;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Blob not found");
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { error = "File not found" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URL");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Failed to generate SAS URL" });
            return response;
        }
    }

    /// <summary>
    /// GET /api/invoice/blob/{id}?vendorId={vendorId}&blobUrl={blobUrl}
    /// Returns the invoice blob file with proper content type
    /// </summary>
    [Function("GetInvoiceBlob")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice/blob/{id}")] HttpRequestData req,
        string id)
    {
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var vendorId = query["vendorId"];
            var blobUrl = query["blobUrl"];

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(vendorId))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Missing id or vendorId parameter" });
                return badResponse;
            }

            if (string.IsNullOrEmpty(blobUrl))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Missing blobUrl parameter" });
                return badResponse;
            }

            _logger.LogInformation("Getting blob for invoice: {Id}, vendor: {VendorId}", id, vendorId);

            // Download blob
            var (content, contentType) = await _blobService.GetBlobAsync(blobUrl);

            // Return blob content as file
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", contentType);
            response.Headers.Add("Content-Disposition", $"inline; filename=invoice-{id}.pdf");

            await response.Body.WriteAsync(await ReadStreamAsByteArrayAsync(content));

            return response;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Blob not found for invoice: {Id}", id);
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { error = "Invoice file not found" });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blob for invoice: {Id}", id);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "Failed to retrieve invoice file" });
            return response;
        }
    }

    private static async Task<byte[]> ReadStreamAsByteArrayAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}
