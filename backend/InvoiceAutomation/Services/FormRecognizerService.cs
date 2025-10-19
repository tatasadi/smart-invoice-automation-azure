using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using InvoiceAutomation.Models;
using Microsoft.Extensions.Logging;

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

            // Extract fields
            var extractedData = new ExtractedData
            {
                Vendor = GetFieldValue(invoice, "VendorName"),
                InvoiceNumber = GetFieldValue(invoice, "InvoiceId"),
                InvoiceDate = GetFieldValue(invoice, "InvoiceDate"),
                TotalAmount = GetFieldValueAsDecimal(invoice, "InvoiceTotal"),
                Currency = GetFieldValue(invoice, "CurrencyCode") ?? "USD",
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
