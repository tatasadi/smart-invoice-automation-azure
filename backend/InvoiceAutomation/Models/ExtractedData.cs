using Newtonsoft.Json;

namespace InvoiceAutomation.Models;

/// <summary>
/// Data extracted from invoice using Azure Form Recognizer
/// </summary>
public class ExtractedData
{
    /// <summary>
    /// Vendor/supplier name
    /// </summary>
    [JsonProperty("vendor")]
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// Invoice number/ID
    /// </summary>
    [JsonProperty("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Invoice date (as string for flexibility)
    /// </summary>
    [JsonProperty("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    /// <summary>
    /// Total amount on the invoice
    /// </summary>
    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR)
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Line items extracted from the invoice (optional)
    /// </summary>
    [JsonProperty("lineItems")]
    public List<LineItem>? LineItems { get; set; }
}

/// <summary>
/// Individual line item from invoice
/// </summary>
public class LineItem
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("quantity")]
    public decimal? Quantity { get; set; }

    [JsonProperty("unitPrice")]
    public decimal? UnitPrice { get; set; }

    [JsonProperty("amount")]
    public decimal Amount { get; set; }
}
