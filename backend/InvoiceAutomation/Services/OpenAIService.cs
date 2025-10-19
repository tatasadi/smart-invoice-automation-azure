using Azure;
using Azure.AI.OpenAI;
using InvoiceAutomation.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace InvoiceAutomation.Services;

/// <summary>
/// Service for classifying invoices using Azure OpenAI
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Classify an invoice into a category
    /// </summary>
    /// <param name="extractedData">Extracted invoice data</param>
    /// <returns>Classification result</returns>
    Task<Classification> ClassifyInvoiceAsync(ExtractedData extractedData);
}

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(string endpoint, string key, string deploymentName, ILogger<OpenAIService> logger)
    {
        _logger = logger;
        _client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _deploymentName = deploymentName;
    }

    public async Task<Classification> ClassifyInvoiceAsync(ExtractedData extractedData)
    {
        try
        {
            _logger.LogInformation("Classifying invoice for vendor: {Vendor}", extractedData.Vendor);

            var categories = string.Join("\n- ", InvoiceCategories.GetAllCategories());

            var prompt = $@"You are an expert accountant. Classify this invoice into one of these categories:
- {categories}

Invoice details:
Vendor: {extractedData.Vendor}
Amount: {extractedData.TotalAmount:C} {extractedData.Currency}
Invoice Number: {extractedData.InvoiceNumber}

Analyze the vendor name and invoice details to determine the most appropriate category.

Return ONLY valid JSON in this exact format (no markdown, no code blocks):
{{""category"": ""category name"", ""confidence"": 0.95, ""reasoning"": ""brief explanation""}}";

            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName = _deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage("You are an expert accountant who classifies invoices. Always respond with valid JSON only."),
                    new ChatRequestUserMessage(prompt)
                },
                Temperature = 0.3f,
                MaxTokens = 200,
                NucleusSamplingFactor = 0.95f
            };

            var response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
            var content = response.Value.Choices[0].Message.Content;

            _logger.LogInformation("OpenAI response: {Response}", content);

            // Clean up the response (remove markdown code blocks if present)
            content = content.Trim();
            if (content.StartsWith("```json"))
            {
                content = content.Replace("```json", "").Replace("```", "").Trim();
            }
            else if (content.StartsWith("```"))
            {
                content = content.Replace("```", "").Trim();
            }

            // Parse JSON response
            var classification = JsonSerializer.Deserialize<Classification>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (classification == null)
            {
                _logger.LogWarning("Failed to parse classification response, using default");
                return new Classification
                {
                    Category = InvoiceCategories.Other,
                    Confidence = 0.5,
                    Reasoning = "Unable to classify"
                };
            }

            _logger.LogInformation("Invoice classified as: {Category} (confidence: {Confidence})",
                classification.Category, classification.Confidence);

            return classification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying invoice");

            // Return a default classification on error
            return new Classification
            {
                Category = InvoiceCategories.Other,
                Confidence = 0.5,
                Reasoning = $"Classification error: {ex.Message}"
            };
        }
    }
}
