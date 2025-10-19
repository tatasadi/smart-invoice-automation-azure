using Microsoft.Azure.Cosmos;
using InvoiceAutomation.Models;
using Microsoft.Extensions.Logging;

namespace InvoiceAutomation.Services;

/// <summary>
/// Service for persisting invoice data to Azure Cosmos DB
/// </summary>
public interface ICosmosDbService
{
    /// <summary>
    /// Save an invoice to the database
    /// </summary>
    /// <param name="invoice">Invoice data to save</param>
    /// <returns>Saved invoice data</returns>
    Task<InvoiceData> SaveInvoiceAsync(InvoiceData invoice);

    /// <summary>
    /// Get all invoices, ordered by upload date descending
    /// </summary>
    /// <returns>List of all invoices</returns>
    Task<List<InvoiceData>> GetAllInvoicesAsync();

    /// <summary>
    /// Get a specific invoice by ID and vendor ID
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="vendorId">Vendor ID (partition key)</param>
    /// <returns>Invoice data or null if not found</returns>
    Task<InvoiceData?> GetInvoiceByIdAsync(string id, string vendorId);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(
        string endpoint,
        string key,
        string databaseName,
        string containerName,
        ILogger<CosmosDbService> logger)
    {
        _logger = logger;
        var client = new CosmosClient(endpoint, key);
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task<InvoiceData> SaveInvoiceAsync(InvoiceData invoice)
    {
        try
        {
            _logger.LogInformation("Saving invoice to Cosmos DB: {InvoiceId}, VendorId: {VendorId}", invoice.Id, invoice.VendorId);

            var response = await _container.CreateItemAsync(
                invoice,
                new PartitionKey(invoice.VendorId));

            _logger.LogInformation("Invoice saved successfully. Request charge: {RequestCharge} RU",
                response.RequestCharge);

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving invoice to Cosmos DB: {InvoiceId}", invoice.Id);
            throw;
        }
    }

    public async Task<List<InvoiceData>> GetAllInvoicesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all invoices from Cosmos DB");

            var query = new QueryDefinition("SELECT * FROM c ORDER BY c.uploadDate DESC");
            var queryIterator = _container.GetItemQueryIterator<InvoiceData>(query);

            var results = new List<InvoiceData>();

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response);

                _logger.LogInformation("Retrieved {Count} invoices. Request charge: {RequestCharge} RU",
                    response.Count, response.RequestCharge);
            }

            _logger.LogInformation("Total invoices retrieved: {TotalCount}", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices from Cosmos DB");
            throw;
        }
    }

    public async Task<InvoiceData?> GetInvoiceByIdAsync(string id, string vendorId)
    {
        try
        {
            _logger.LogInformation("Retrieving invoice from Cosmos DB: {InvoiceId}, VendorId: {VendorId}", id, vendorId);

            var response = await _container.ReadItemAsync<InvoiceData>(
                id,
                new PartitionKey(vendorId));

            _logger.LogInformation("Invoice retrieved successfully. Request charge: {RequestCharge} RU",
                response.RequestCharge);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Invoice not found: {InvoiceId}, VendorId: {VendorId}", id, vendorId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice from Cosmos DB: {InvoiceId}", id);
            throw;
        }
    }
}
