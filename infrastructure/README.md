# Smart Invoice Automation - Azure Infrastructure

This folder contains the Infrastructure as Code (IaC) for deploying the Smart Invoice Automation solution to Azure using Bicep.

## Architecture Overview

This deployment creates the following Azure resources:

- **Storage Account** - For storing invoice files (blob storage)
- **Cosmos DB** - Serverless NoSQL database for invoice metadata
- **Form Recognizer** - Azure AI service for document intelligence
- **Azure OpenAI** - GPT-4o deployment for invoice processing
- **Function App** - Serverless compute (.NET 8 Isolated)
- **Application Insights** - Monitoring and telemetry
- **Log Analytics Workspace** - Centralized logging

## Prerequisites

Before deploying, ensure you have:

1. **Azure CLI** installed ([Installation Guide](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli))
2. **Active Azure Subscription** with sufficient permissions
3. **Azure OpenAI Access** - Your subscription must be approved for Azure OpenAI
4. **Bicep CLI** (included with Azure CLI 2.20.0+)

Verify installations:
```bash
az --version
az bicep version
```

## Deployment Steps

### 1. Login to Azure

```bash
az login
```

Set your subscription (if you have multiple):
```bash
az account list --output table
az account set --subscription "<Your-Subscription-ID>"
```

### 2. Create Resource Group

```bash
# Define variables
RESOURCE_GROUP="rg-invoice-automation"
LOCATION="swedencentral"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

**Location Note**: Use `swedencentral` or another region that supports Azure OpenAI. Check availability:
```bash
az account list-locations --query "[?metadata.regionCategory=='Recommended'].{Name:name, DisplayName:displayName}" --output table
```

### 3. Review and Customize Parameters

Edit [main.bicepparam](main.bicepparam) to customize your deployment:

```bicep
param location = 'swedencentral'              // Azure region
param environment = 'dev'                     // 'dev' or 'prod'
param allowedOrigins = ['*']                  // CORS origins (restrict in prod!)
param cosmosPartitionKeyPath = '/vendorId'    // Partition key strategy
```

**Parameter Guidelines:**

- **environment**: Use `dev` for development (free tier services), `prod` for production
- **allowedOrigins**: Set to `['*']` for dev only. For production, use specific URLs:
  ```bicep
  param allowedOrigins = ['https://yourdomain.com', 'https://app.yourdomain.com']
  ```
- **cosmosPartitionKeyPath**: Choose based on your query patterns:
  - `/vendorId` - If you query by vendor frequently
  - `/customerId` - If you query by customer frequently
  - `/invoiceDate` - For time-based partitioning (use YYYY-MM format)
  - **DO NOT** use `/id` - Creates poor partitioning

### 4. Deploy Infrastructure

#### Option A: Using Parameters File (Recommended)

```bash
RESOURCE_GROUP="rg-invoice-automation"

az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam
```

#### Option B: Inline Parameters

```bash
RESOURCE_GROUP="rg-invoice-automation"

az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters location=swedencentral \
  --parameters environment=dev \
  --parameters allowedOrigins="['*']" \
  --parameters cosmosPartitionKeyPath=/vendorId
```

#### Option C: What-If Deployment (Preview Changes)

Preview changes before actual deployment:

```bash
az deployment group what-if \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam
```

### 5. Monitor Deployment

The deployment takes approximately 5-10 minutes. Monitor progress:

```bash
# Watch deployment status
az deployment group list \
  --resource-group $RESOURCE_GROUP \
  --output table

# Show deployment details
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --output json
```

### 6. Retrieve Outputs

After successful deployment, retrieve important configuration values:

```bash
# Get all outputs
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs

# Get specific values
FUNCTION_APP_URL=$(az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs.functionAppUrl.value \
  --output tsv)

echo "Function App URL: $FUNCTION_APP_URL"
```

**Important Outputs:**
- `functionAppUrl` - API endpoint
- `storageAccountName` - Storage account name
- `applicationInsightsConnectionString` - For monitoring
- Secure outputs (keys/secrets) - Retrieved securely when needed

## Post-Deployment Steps

### 1. Verify Resource Creation

```bash
# List all resources in the resource group
az resource list \
  --resource-group $RESOURCE_GROUP \
  --output table
```

### 2. Configure Function App

Deploy your application code to the Function App:

```bash
# Navigate to your function app code directory
cd ../src  # Adjust path as needed

# Deploy function app (if using Azure Functions Core Tools)
func azure functionapp publish func-invoice-automation-ta
```

### 3. Test the Deployment

```bash
# Get the Function App URL
FUNCTION_APP_URL=$(az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs.functionAppUrl.value \
  --output tsv)

# Test health endpoint (adjust based on your API)
curl $FUNCTION_APP_URL/health
```

### 4. Monitor Application

Access Application Insights:
```bash
az monitor app-insights component show \
  --resource-group $RESOURCE_GROUP \
  --app appi-invoice-automation-ta
```

## Resource Naming Convention

Resources follow Azure naming best practices:

| Resource Type | Name Pattern | Example |
|---------------|--------------|---------|
| Storage Account | `stinvoiceautomationta` | stinvoiceautomationta |
| Cosmos DB | `cosmos-invoice-automation-ta` | cosmos-invoice-automation-ta |
| Form Recognizer | `form-invoice-automation-ta` | form-invoice-automation-ta |
| Azure OpenAI | `openai-invoice-automation-ta` | openai-invoice-automation-ta |
| Function App | `func-invoice-automation-ta` | func-invoice-automation-ta |
| App Service Plan | `asp-invoice-automation-ta` | asp-invoice-automation-ta |
| Application Insights | `appi-invoice-automation-ta` | appi-invoice-automation-ta |
| Log Analytics | `log-invoice-automation-ta` | log-invoice-automation-ta |

## Cost Optimization

### Development Environment
- **Form Recognizer**: F0 (Free tier)
- **Azure OpenAI**: S0 (Pay-as-you-go)
- **Cosmos DB**: Serverless mode
- **Function App**: Consumption plan (Y1)
- **Storage**: Standard LRS

### Production Environment
- **Form Recognizer**: S0 (Standard)
- **Network Security**: Private endpoints enabled
- **Public Access**: Disabled for sensitive services

**Estimated Monthly Cost (Dev)**: ~$10-50 USD (depends on usage)
**Estimated Monthly Cost (Prod)**: ~$100-500 USD (depends on scale)

## Updating Infrastructure

To update existing resources:

```bash
# 1. Modify main.bicep or main.bicepparam
# 2. Preview changes
az deployment group what-if \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam

# 3. Apply changes
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam
```

## Cleanup

To delete all resources and avoid charges:

```bash
# Delete the entire resource group (⚠️ CAUTION: This is irreversible!)
az group delete \
  --name $RESOURCE_GROUP \
  --yes \
  --no-wait
```

## Troubleshooting

### Common Issues

**Issue**: Azure OpenAI deployment fails
```
Error: Azure OpenAI is not available in this region/subscription
```
**Solution**:
1. Verify your subscription has Azure OpenAI access
2. Change location to a supported region (e.g., swedencentral, eastus)
3. Apply for Azure OpenAI access: https://aka.ms/oai/access

**Issue**: Form Recognizer F0 tier already exists
```
Error: The free tier is already used in another resource
```
**Solution**: Use S0 tier or delete the existing F0 resource

**Issue**: Storage account name already taken
```
Error: Storage account name 'stinvoiceautomationta' is already taken
```
**Solution**: Storage names must be globally unique. Modify the name in [main.bicep](main.bicep:23) to include a unique suffix

### Validation

Validate your Bicep file before deployment:

```bash
az bicep build --file main.bicep
az deployment group validate \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam
```

## Security Considerations

### Development Environment
- Public access enabled for easier testing
- CORS set to `*` (allow all origins)
- Free/standard tiers for cost savings

### Production Environment
- Public network access disabled for Cosmos DB, Form Recognizer, and OpenAI
- CORS restricted to specific domains
- HTTPS-only traffic enforced
- Managed identities recommended (future enhancement)
- Consider adding:
  - Azure Key Vault for secrets management
  - Virtual Network integration
  - Private endpoints
  - Azure Front Door for global distribution

## Support & Documentation

- [Azure Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Azure CLI Reference](https://learn.microsoft.com/en-us/cli/azure/)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- [Azure Form Recognizer](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/)
- [Azure Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/)

## File Structure

```
infrastructure/
├── README.md              # This file - deployment guide
├── main.bicep            # Main infrastructure template
└── main.bicepparam       # Parameters configuration
```

## Quick Start Script

Save this as `deploy.sh` for quick deployment:

```bash
#!/bin/bash
set -e

# Configuration
RESOURCE_GROUP="rg-invoice-automation"
LOCATION="swedencentral"

# Login check
echo "Checking Azure login..."
az account show > /dev/null 2>&1 || az login

# Create resource group
echo "Creating resource group..."
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Deploy infrastructure
echo "Deploying infrastructure..."
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.bicepparam

# Show outputs
echo "Deployment complete! Here are your outputs:"
az deployment group show \
  --resource-group $RESOURCE_GROUP \
  --name main \
  --query properties.outputs
```

Make it executable and run:
```bash
chmod +x deploy.sh
./deploy.sh
```
