using System.Diagnostics;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InvoiceAutomation.Models;
using InvoiceAutomation.Services;

namespace InvoiceAutomation.Functions;

/// <summary>
/// HTTP function for uploading and processing invoices
/// </summary>
public class UploadInvoice
{
    private readonly ILogger<UploadInvoice> _logger;
    private readonly IBlobStorageService _blobService;
    private readonly IFormRecognizerService _formRecognizerService;
    private readonly IOpenAIService _openAIService;
    private readonly ICosmosDbService _cosmosDbService;

    public UploadInvoice(
        ILogger<UploadInvoice> logger,
        IBlobStorageService blobService,
        IFormRecognizerService formRecognizerService,
        IOpenAIService openAIService,
        ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _blobService = blobService;
        _formRecognizerService = formRecognizerService;
        _openAIService = openAIService;
        _cosmosDbService = cosmosDbService;
    }

    [Function("UploadInvoice")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")] HttpRequestData req)
    {
        _logger.LogInformation("Processing invoice upload request");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get content type and read body as stream
            var contentType = req.Headers.GetValues("Content-Type").FirstOrDefault() ?? string.Empty;

            if (!contentType.Contains("multipart/form-data"))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Request must be multipart/form-data" });
                return badResponse;
            }

            // For simplicity in the demo, read the entire stream
            // In production, use a proper multipart parser
            var stream = req.Body;
            var fileName = "invoice-" + Guid.NewGuid().ToString() + ".pdf";
            var fileContentType = "application/pdf";

            // Try to extract filename from headers
            if (req.Headers.Contains("X-File-Name"))
            {
                fileName = req.Headers.GetValues("X-File-Name").FirstOrDefault() ?? fileName;
            }

            // Validate file extension
            var allowedExtensions = new List<string> { ".pdf", ".png", ".jpg", ".jpeg", ".tiff", ".bmp" };
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Invalid file type: {FileExtension}", fileExtension);
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new
                {
                    error = $"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}"
                });
                return badResponse;
            }

            _logger.LogInformation("Processing file: {FileName}, ContentType: {ContentType}", fileName, fileContentType);

            // Read stream into memory once
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Step 1: Upload to Blob Storage
            string blobUrl;
            using (var uploadStream = new MemoryStream(fileBytes))
            {
                blobUrl = await _blobService.UploadFileAsync(uploadStream, fileName, fileContentType);
            }
            _logger.LogInformation("File uploaded to blob storage: {BlobUrl}", blobUrl);

            // Step 2: Extract data with Form Recognizer (using stream instead of URL)
            ExtractedData extractedData;
            using (var analysisStream = new MemoryStream(fileBytes))
            {
                extractedData = await _formRecognizerService.AnalyzeInvoiceAsync(analysisStream);
            }
            _logger.LogInformation("Invoice data extracted. Vendor: {Vendor}, Amount: {Amount}",
                extractedData.Vendor, extractedData.TotalAmount);

            // Step 3: Classify with OpenAI
            var classification = await _openAIService.ClassifyInvoiceAsync(extractedData);
            _logger.LogInformation("Invoice classified as: {Category} (confidence: {Confidence})",
                classification.Category, classification.Confidence);

            // Step 4: Create invoice data object
            stopwatch.Stop();

            // Normalize vendor name to create vendorId (partition key)
            var vendorId = NormalizeVendorId(extractedData.Vendor);

            var invoiceData = new InvoiceData
            {
                VendorId = vendorId,
                FileName = fileName,
                BlobUrl = blobUrl,
                UploadDate = DateTime.UtcNow,
                ExtractedData = extractedData,
                Classification = classification,
                ProcessingMetadata = new ProcessingMetadata
                {
                    ProcessingTime = stopwatch.Elapsed.TotalSeconds,
                    FormRecognizerConfidence = 0.95,
                    Status = ProcessingStatus.Completed,
                    StartTime = DateTime.UtcNow.AddSeconds(-stopwatch.Elapsed.TotalSeconds),
                    EndTime = DateTime.UtcNow
                }
            };

            // Step 5: Save to Cosmos DB
            var savedInvoice = await _cosmosDbService.SaveInvoiceAsync(invoiceData);
            _logger.LogInformation("Invoice saved to database: {InvoiceId}", savedInvoice.Id);

            // Return success response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(savedInvoice);

            _logger.LogInformation("Invoice processed successfully in {ElapsedSeconds:F2} seconds",
                stopwatch.Elapsed.TotalSeconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice upload");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new
            {
                error = "Failed to process invoice",
                message = ex.Message,
                details = ex.InnerException?.Message
            });

            return errorResponse;
        }
    }

    /// <summary>
    /// Normalize vendor name to create a consistent vendor ID for partition key
    /// </summary>
    private static string NormalizeVendorId(string vendorName)
    {
        if (string.IsNullOrWhiteSpace(vendorName))
        {
            return "unknown";
        }

        // Convert to lowercase, remove special characters, replace spaces with hyphens
        var normalized = vendorName.ToLowerInvariant()
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Trim();

        // Replace multiple spaces with single space, then replace spaces with hyphens
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");
        normalized = normalized.Replace(" ", "-");

        return normalized;
    }
}
