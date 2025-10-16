namespace InvoiceAutomation.Models;

public class InvoiceData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public ExtractedData? ExtractedData { get; set; }
    public Classification? Classification { get; set; }
    public ProcessingMetadata? ProcessingMetadata { get; set; }
}

public class ExtractedData
{
    public string Vendor { get; set; } = string.Empty;
    public string? VendorAddress { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? InvoiceDate { get; set; }
    public string? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public List<LineItem>? LineItems { get; set; }
}

public class LineItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public class Classification
{
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Reasoning { get; set; }
}

public class ProcessingMetadata
{
    public double ProcessingTime { get; set; }
    public double FormRecognizerConfidence { get; set; }
    public string Status { get; set; } = "completed";
    public List<string>? Errors { get; set; }
}
