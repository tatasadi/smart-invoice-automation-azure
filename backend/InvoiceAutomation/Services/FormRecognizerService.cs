using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using InvoiceAutomation.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Linq;

namespace InvoiceAutomation.Services;

/// <summary>
/// Service for extracting data from invoices using Azure Form Recognizer
/// </summary>
public interface IFormRecognizerService
{
    /// <summary>
    /// Analyze an invoice and extract structured data
    /// </summary>
    /// <param name="documentStream">Stream containing the invoice document</param>
    /// <returns>Extracted invoice data</returns>
    Task<ExtractedData> AnalyzeInvoiceAsync(Stream documentStream);
}

public class FormRecognizerService : IFormRecognizerService
{
    private readonly DocumentAnalysisClient _client;
    private readonly ILogger<FormRecognizerService> _logger;

    public FormRecognizerService(string endpoint, string key, ILogger<FormRecognizerService> logger)
    {
        _logger = logger;
        _client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    public async Task<ExtractedData> AnalyzeInvoiceAsync(Stream documentStream)
    {
        try
        {
            _logger.LogInformation("Analyzing invoice from stream");

            // Use prebuilt invoice model with stream
            var operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-invoice",
                documentStream);

            var result = operation.Value;
            var invoice = result.Documents.FirstOrDefault();

            if (invoice == null)
            {
                _logger.LogWarning("No invoice document found in the analyzed file");
                throw new InvalidOperationException("No invoice found in document");
            }

            // Extract fields with robust fallbacks for vendor and currency
            var vendor = GetFieldValue(invoice, "VendorName");
            if (string.IsNullOrWhiteSpace(vendor))
            {
                vendor = TryInferVendorFromLayout(result);
            }

            var invoiceNumber = GetFieldValue(invoice, "InvoiceId");
            var invoiceDate = GetFieldValue(invoice, "InvoiceDate");
            var totalAmount = GetFieldValueAsDecimal(invoice, "InvoiceTotal");

            // Prefer explicit currency code; otherwise infer from symbols/content; finally default to USD
            var currency = GetFieldValue(invoice, "CurrencyCode");
            if (string.IsNullOrWhiteSpace(currency))
            {
                string? totalText = null;
                if (invoice.Fields.TryGetValue("InvoiceTotal", out var totalField))
                {
                    totalText = totalField.Content;
                }
                currency = InferCurrencyFromText(totalText) ?? InferCurrencyFromText(result.Content) ?? "USD";
            }

            var extractedData = new ExtractedData
            {
                Vendor = vendor,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = invoiceDate,
                TotalAmount = totalAmount,
                Currency = currency,
                LineItems = ExtractLineItems(invoice)
            };

            _logger.LogInformation("Invoice analyzed successfully. Vendor: {Vendor}, Amount: {Amount}",
                extractedData.Vendor, extractedData.TotalAmount);

            return extractedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing invoice from stream");
            throw;
        }
    }

    private string GetFieldValue(AnalyzedDocument document, string fieldName)
    {
        if (document.Fields.TryGetValue(fieldName, out var field) && field.Content != null)
        {
            return field.Content;
        }
        return string.Empty;
    }

    private decimal GetFieldValueAsDecimal(AnalyzedDocument document, string fieldName)
    {
        if (document.Fields.TryGetValue(fieldName, out var field) && field.Content != null)
        {
            // Try to parse the content as decimal, removing currency symbols and commas
            var cleanedContent = field.Content.Replace("$", "").Replace(",", "").Trim();
            if (decimal.TryParse(cleanedContent, out var amount))
            {
                return amount;
            }
        }
        return 0;
    }

    // Try to infer a vendor name from page layout when the model doesn't map VendorName.
    // Heuristic: pick the first prominent line on page 1 that isn't a label like "INVOICE",
    // and doesn't look like a date/number block.
    private string TryInferVendorFromLayout(AnalyzeResult result)
    {
        var page = result.Pages.FirstOrDefault();
        if (page == null || page.Lines == null) return string.Empty;

        // Common labels to skip
        string[] skipTokens = new[] { "INVOICE", "BILL TO", "SHIP TO", "DATE", "BALANCE DUE", "SUBTOTAL", "TOTAL", "DISCOUNT", "SHIPPING" };
        foreach (var line in page.Lines)
        {
            var text = (line.Content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;

            var upper = text.ToUpperInvariant();
            if (skipTokens.Any(t => upper.Contains(t))) continue;
            // Skip obvious invoice numbers like "# 24939" or lines starting with '#'
            if (Regex.IsMatch(text, @"^#?\s*\d{3,}")) continue;
            // Skip monetary lines
            if (Regex.IsMatch(text, @"[\$€£¥]\s*\d")) continue;

            // Favor short brand-like lines (1-3 words)
            var words = Regex.Split(text, @"\s+").Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            if (words.Length > 0 && words.Length <= 3)
            {
                return text;
            }
        }
        return string.Empty;
    }

    // Infer ISO currency code from text containing amounts or symbols
    private string? InferCurrencyFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var t = text.Trim();
        var upper = t.ToUpperInvariant();

        // Direct codes first
        if (upper.Contains("USD")) return "USD";
        if (upper.Contains("EUR")) return "EUR";
        if (upper.Contains("GBP")) return "GBP";
        if (upper.Contains("JPY")) return "JPY";
        if (upper.Contains("AUD")) return "AUD";
        if (upper.Contains("CAD")) return "CAD";
        if (upper.Contains("INR") || upper.Contains("RS ") || upper.Contains(" RS")) return "INR";
        if (upper.Contains("NZD")) return "NZD";

        // Symbol heuristics and localized prefixes
        if (Regex.IsMatch(t, @"(US\$|\$)"))
        {
            // Disambiguate some prefixes
            if (upper.Contains("CA$") || upper.Contains("C$")) return "CAD";
            if (upper.Contains("AU$") || upper.Contains("A$")) return "AUD";
            if (upper.Contains("NZ$") ) return "NZD";
            return "USD";
        }
        if (t.Contains("€")) return "EUR";
        if (t.Contains("£")) return "GBP";
        if (t.Contains("¥")) return "JPY";

        return null;
    }

    private List<LineItem>? ExtractLineItems(AnalyzedDocument document)
    {
        var lineItems = new List<LineItem>();

        if (document.Fields.TryGetValue("Items", out var itemsField) &&
            itemsField.FieldType == DocumentFieldType.List)
        {
            var items = itemsField.Value as IReadOnlyList<DocumentField>;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.FieldType == DocumentFieldType.Dictionary)
                    {
                        var itemDict = item.Value as IReadOnlyDictionary<string, DocumentField>;
                        if (itemDict != null)
                        {
                            var lineItem = new LineItem
                            {
                                Description = GetDictionaryFieldValue(itemDict, "Description"),
                                Quantity = GetDictionaryFieldValueAsDecimal(itemDict, "Quantity"),
                                UnitPrice = GetDictionaryFieldValueAsDecimal(itemDict, "UnitPrice"),
                                Amount = GetDictionaryFieldValueAsDecimal(itemDict, "Amount") ?? 0
                            };
                            lineItems.Add(lineItem);
                        }
                    }
                }
            }
        }

        return lineItems.Count > 0 ? lineItems : null;
    }

    private string GetDictionaryFieldValue(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        return dict.TryGetValue(key, out var field) ? field.Content ?? string.Empty : string.Empty;
    }

    private decimal? GetDictionaryFieldValueAsDecimal(IReadOnlyDictionary<string, DocumentField> dict, string key)
    {
        if (dict.TryGetValue(key, out var field) && field.Content != null)
        {
            if (decimal.TryParse(field.Content.Replace("$", "").Replace(",", ""), out var value))
            {
                return value;
            }
        }
        return null;
    }
}
