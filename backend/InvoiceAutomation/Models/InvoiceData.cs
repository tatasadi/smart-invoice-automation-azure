using Newtonsoft.Json;

namespace InvoiceAutomation.Models;

/// <summary>
/// Main invoice data model stored in Cosmos DB
/// </summary>
public class InvoiceData
{
    /// <summary>
    /// Unique identifier for the invoice (also used as partition key)
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original filename of the uploaded invoice
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// URL to the blob storage location
    /// </summary>
    [JsonProperty("blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the invoice was uploaded
    /// </summary>
    [JsonProperty("uploadDate")]
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data extracted from the invoice by Form Recognizer
    /// </summary>
    [JsonProperty("extractedData")]
    public ExtractedData? ExtractedData { get; set; }

    /// <summary>
    /// Classification result from Azure OpenAI
    /// </summary>
    [JsonProperty("classification")]
    public Classification? Classification { get; set; }

    /// <summary>
    /// Metadata about the processing
    /// </summary>
    [JsonProperty("processingMetadata")]
    public ProcessingMetadata? ProcessingMetadata { get; set; }
}
