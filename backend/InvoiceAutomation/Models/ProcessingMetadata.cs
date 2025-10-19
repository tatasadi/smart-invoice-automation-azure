using Newtonsoft.Json;

namespace InvoiceAutomation.Models;

/// <summary>
/// Metadata about invoice processing
/// </summary>
public class ProcessingMetadata
{
    /// <summary>
    /// Total processing time in seconds
    /// </summary>
    [JsonProperty("processingTime")]
    public double ProcessingTime { get; set; }

    /// <summary>
    /// Form Recognizer confidence score (0-1)
    /// </summary>
    [JsonProperty("formRecognizerConfidence")]
    public double FormRecognizerConfidence { get; set; }

    /// <summary>
    /// Processing status (e.g., "completed", "failed", "processing")
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    [JsonProperty("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when processing started
    /// </summary>
    [JsonProperty("startTime")]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when processing completed
    /// </summary>
    [JsonProperty("endTime")]
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Processing status constants
/// </summary>
public static class ProcessingStatus
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Completed = "completed";
    public const string Failed = "failed";
}
