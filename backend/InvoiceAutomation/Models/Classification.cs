using Newtonsoft.Json;

namespace InvoiceAutomation.Models;

/// <summary>
/// Classification result from Azure OpenAI
/// </summary>
public class Classification
{
    /// <summary>
    /// Classified category
    /// </summary>
    [JsonProperty("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score (0-1)
    /// </summary>
    [JsonProperty("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Brief explanation of the classification
    /// </summary>
    [JsonProperty("reasoning")]
    public string? Reasoning { get; set; }
}

/// <summary>
/// Available invoice categories
/// </summary>
public static class InvoiceCategories
{
    public const string MarketingAdvertising = "Marketing & Advertising";
    public const string ITServicesSoftware = "IT Services & Software";
    public const string OfficeSupplies = "Office Supplies";
    public const string Utilities = "Utilities";
    public const string ProfessionalServices = "Professional Services";
    public const string TravelEntertainment = "Travel & Entertainment";
    public const string EquipmentHardware = "Equipment & Hardware";
    public const string MaintenanceRepairs = "Maintenance & Repairs";
    public const string Other = "Other";

    /// <summary>
    /// Get all available categories as a list
    /// </summary>
    public static List<string> GetAllCategories() => new()
    {
        MarketingAdvertising,
        ITServicesSoftware,
        OfficeSupplies,
        Utilities,
        ProfessionalServices,
        TravelEntertainment,
        EquipmentHardware,
        MaintenanceRepairs,
        Other
    };
}
