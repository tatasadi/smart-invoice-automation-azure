using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace InvoiceAutomation.Services;

/// <summary>
/// Service for uploading files to Azure Blob Storage
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Upload a file stream to blob storage
    /// </summary>
    /// <param name="fileStream">File content stream</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="contentType">MIME content type</param>
    /// <returns>URL to the uploaded blob</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(string connectionString, string containerName, ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Ensure container exists
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Create unique filename with date-based folder structure
            var uniqueFileName = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}-{fileName}";
            var blobClient = _containerClient.GetBlobClient(uniqueFileName);

            _logger.LogInformation("Uploading file to blob storage: {FileName}", uniqueFileName);

            // Upload with content type
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            _logger.LogInformation("File uploaded successfully: {BlobUrl}", blobClient.Uri);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to blob storage: {FileName}", fileName);
            throw;
        }
    }
}
