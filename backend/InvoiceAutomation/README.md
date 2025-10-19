# Invoice Automation Backend

.NET 8 Azure Functions backend for AI-powered invoice processing.

## Tech Stack

- **.NET 8** (Isolated Worker Process)
- **Azure Functions v4**
- **Azure Blob Storage** - File storage
- **Azure Form Recognizer** - Invoice data extraction
- **Azure OpenAI GPT-4o** - Invoice classification
- **Azure Cosmos DB** - Data persistence

## Project Structure

```
InvoiceAutomation/
├── Functions/              # HTTP trigger functions
│   ├── UploadInvoice.cs   # POST /api/upload
│   ├── GetInvoices.cs     # GET /api/invoices
│   └── GetInvoice.cs      # GET /api/invoice/{id}
├── Services/              # Azure service integrations
│   ├── BlobStorageService.cs
│   ├── FormRecognizerService.cs
│   ├── OpenAIService.cs
│   └── CosmosDbService.cs
├── Models/                # Data models
│   ├── InvoiceData.cs
│   ├── ExtractedData.cs
│   ├── Classification.cs
│   └── ProcessingMetadata.cs
├── Program.cs             # DI configuration
├── host.json             # Functions host config
└── local.settings.json   # Local environment variables
```

## Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- Azure account with provisioned resources:
  - Storage Account
  - Cosmos DB
  - Form Recognizer
  - Azure OpenAI

### Installation

1. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

2. **Configure local settings:**

   Copy `local.settings.example.json` to `local.settings.json` and update with your Azure credentials:

   ```json
   {
     "Values": {
       "AzureStorageConnectionString": "your-connection-string",
       "CosmosDbEndpoint": "https://your-account.documents.azure.com:443/",
       "CosmosDbKey": "your-key",
       "FormRecognizerEndpoint": "https://your-region.api.cognitive.microsoft.com/",
       "FormRecognizerKey": "your-key",
       "AzureOpenAIEndpoint": "https://your-resource.openai.azure.com/",
       "AzureOpenAIKey": "your-key",
       "AzureOpenAIDeploymentName": "gpt-4o"
     }
   }
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

## Running Locally

```bash
# Start the Function App
func start

# Or with dotnet
dotnet run

# With hot reload
dotnet watch run
```

The API will be available at `http://localhost:7071/api`

### Available Endpoints

- **POST** `/api/upload` - Upload and process an invoice
- **GET** `/api/invoices` - Get all invoices
- **GET** `/api/invoice/{id}` - Get specific invoice

## Testing

### Upload an Invoice

```bash
curl -X POST http://localhost:7071/api/upload \
  -F "file=@path/to/invoice.pdf"
```

### Get All Invoices

```bash
curl http://localhost:7071/api/invoices
```

### Get Specific Invoice

```bash
curl http://localhost:7071/api/invoice/{invoice-id}
```

## Deployment

Deploy to Azure:

```bash
func azure functionapp publish <your-function-app-name>
```

Make sure to configure the same environment variables in Azure Portal under:
**Function App > Configuration > Application Settings**

## Invoice Categories

The system classifies invoices into these categories:

- Marketing & Advertising
- IT Services & Software
- Office Supplies
- Utilities
- Professional Services
- Travel & Entertainment
- Equipment & Hardware
- Maintenance & Repairs
- Other

## Architecture

### Processing Flow

1. **Upload** → File uploaded to Azure Blob Storage
2. **Extract** → Azure Form Recognizer extracts invoice fields
3. **Classify** → Azure OpenAI classifies into category
4. **Store** → Metadata saved to Cosmos DB
5. **Return** → Complete invoice data returned to client

### Services

- **BlobStorageService**: Handles file uploads to Azure Blob Storage
- **FormRecognizerService**: Extracts structured data from invoices
- **OpenAIService**: Classifies invoices using GPT-4o
- **CosmosDbService**: Persists and retrieves invoice data

## Development

### Adding a New Service

1. Create interface in `Services/IYourService.cs`
2. Implement service in `Services/YourService.cs`
3. Register in `Program.cs` ConfigureServices

### Adding a New Function

1. Create function class in `Functions/YourFunction.cs`
2. Add `[Function("FunctionName")]` attribute
3. Inject required services via constructor

## Troubleshooting

### "Configuration value not found"
- Ensure all required settings are in `local.settings.json`
- Check setting names match exactly (case-sensitive)

### CORS errors
- CORS is configured in `host.json` and `local.settings.json`
- For production, update CORS settings in Azure Portal

### Cold start performance
- Use Premium plan for production if needed
- Current setup uses Consumption plan (cost-effective for demo)

## License

MIT
