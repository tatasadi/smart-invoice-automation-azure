using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
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

    /// <summary>
    /// Generate a SAS URL for accessing a blob
    /// </summary>
    /// <param name="blobUrl">The blob URL</param>
    /// <param name="expirationMinutes">Minutes until the SAS token expires (default 60)</param>
    /// <returns>URL with SAS token</returns>
    Task<string> GenerateSasUrlAsync(string blobUrl, int expirationMinutes = 60);

    /// <summary>
    /// Get blob content as a stream
    /// </summary>
    /// <param name="blobUrl">The blob URL</param>
    /// <returns>Blob content stream and content type</returns>
    Task<(Stream Content, string ContentType)> GetBlobAsync(string blobUrl);
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

    public async Task<string> GenerateSasUrlAsync(string blobUrl, int expirationMinutes = 60)
    {
        try
        {
            // Extract blob name from URL
            // URL format: https://<account>.blob.core.windows.net/<container>/<blobname>
            // AbsolutePath will be: /<container>/<blobname>
            // We need to skip the container name (first segment) and get the rest
            var uri = new Uri(blobUrl);
            var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            // URL-decode the segments to handle spaces and special characters
            var decodedSegments = segments.Select(s => Uri.UnescapeDataString(s)).ToArray();
            var blobName = string.Join("/", decodedSegments.Skip(1));

            if (string.IsNullOrEmpty(blobName))
            {
                _logger.LogError("Could not extract blob name from URL: {BlobUrl}", blobUrl);
                throw new ArgumentException($"Invalid blob URL: {blobUrl}");
            }

            _logger.LogInformation("Extracted blob name: {BlobName} from URL: {BlobUrl}", blobName, blobUrl);

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found: {BlobName}", blobName);
                throw new FileNotFoundException($"Blob not found: {blobName}");
            }

            // Generate SAS token
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogError("BlobClient cannot generate SAS URI. Ensure you're using account key authentication.");
                throw new InvalidOperationException("Cannot generate SAS token. Check authentication method.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b", // blob
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow for clock skew
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("Generated SAS URL for blob: {BlobName}, expires in {Minutes} minutes", blobName, expirationMinutes);

            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URL for blob: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task<(Stream Content, string ContentType)> GetBlobAsync(string blobUrl)
    {
        try
        {
            // Extract blob name from URL
            // URL format: https://<account>.blob.core.windows.net/<container>/<blobname>
            // AbsolutePath will be: /<container>/<blobname>
            // We need to skip the container name (first segment) and get the rest
            var uri = new Uri(blobUrl);
            var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            // URL-decode the segments to handle spaces and special characters
            var decodedSegments = segments.Select(s => Uri.UnescapeDataString(s)).ToArray();
            var blobName = string.Join("/", decodedSegments.Skip(1));

            if (string.IsNullOrEmpty(blobName))
            {
                _logger.LogError("Could not extract blob name from URL: {BlobUrl}", blobUrl);
                throw new ArgumentException($"Invalid blob URL: {blobUrl}");
            }

            _logger.LogInformation("Extracted blob name: {BlobName} from URL: {BlobUrl}", blobName, blobUrl);

            var blobClient = _containerClient.GetBlobClient(blobName);

            // Download blob
            var response = await blobClient.DownloadAsync();
            var contentType = response.Value.ContentType;

            _logger.LogInformation("Blob downloaded successfully: {BlobName}, ContentType: {ContentType}", blobName, contentType);

            return (response.Value.Content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading blob: {BlobUrl}", blobUrl);
            throw;
        }
    }
}
