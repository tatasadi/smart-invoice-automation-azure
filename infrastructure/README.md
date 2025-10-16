# Infrastructure as Code - Bicep

This folder contains the Bicep templates to deploy all Azure resources for the Smart Invoice Automation project.

## What Gets Deployed

- **Storage Account** - For invoice file storage (blob container)
- **Cosmos DB** - Serverless database for invoice metadata
- **Form Recognizer** - Free tier for invoice data extraction
- **Azure OpenAI** - With GPT-4o deployment for classification
- **Function App** - .NET 8 isolated worker for backend API
- **Application Insights** - Monitoring and logging
- **Log Analytics Workspace** - Log aggregation

## Prerequisites

```bash
# Azure CLI
brew install azure-cli

# Login to Azure
az login
```

## Quick Deploy

### Option 1: Using the Deployment Script (Recommended)

```bash
cd infrastructure
./deploy.sh
```

The script will:
1. Prompt for configuration (resource group, location, email)
2. Create the resource group
3. Deploy all resources using Bicep
4. Generate `backend/local.settings.json` with all credentials
5. Display deployment summary

**Estimated time**: 5-10 minutes

### Option 2: Manual Deployment

```bash
# Set variables
RESOURCE_GROUP="rg-invoice-automation"
LOCATION="eastus"
YOUR_EMAIL="your.email@example.com"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Deploy Bicep template
az deployment group create \
  --name invoice-automation-deployment \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters ownerEmail="$YOUR_EMAIL" location="$LOCATION"

# Get outputs
az deployment group show \
  --name invoice-automation-deployment \
  --resource-group $RESOURCE_GROUP \
  --query properties.outputs
```

## Files

- **main.bicep** - Main infrastructure template
- **main.bicepparam** - Parameters file (optional)
- **deploy.sh** - Automated deployment script
- **README.md** - This file

## Configuration

Edit `main.bicepparam` to customize:
- `baseName` - Base name for resources (default: `invoice-automation`)
- `location` - Azure region (default: `eastus`)
- `environment` - Environment tag (default: `dev`)
- `ownerEmail` - Your email for resource tagging

## Outputs

After deployment, the following values are available:

- `storageConnectionString` - Storage account connection string
- `cosmosEndpoint` - Cosmos DB endpoint URL
- `cosmosKey` - Cosmos DB primary key
- `formRecognizerEndpoint` - Form Recognizer endpoint
- `formRecognizerKey` - Form Recognizer API key
- `openAIEndpoint` - Azure OpenAI endpoint
- `openAIKey` - Azure OpenAI API key
- `functionAppName` - Function App name
- `functionAppUrl` - Function App API URL
- `applicationInsightsConnectionString` - App Insights connection string

## Verify Deployment

```bash
# List all resources in resource group
az resource list --resource-group rg-invoice-automation --output table

# Check Function App status
az functionapp show \
  --name <function-app-name> \
  --resource-group rg-invoice-automation \
  --query state
```

## Cost Estimation

**Daily Cost (Serverless, minimal usage):**
- Storage Account: ~$0.01
- Cosmos DB Serverless: ~$0.05
- Form Recognizer (Free tier): $0
- Azure OpenAI (GPT-4o): ~$0.05
- Function App (Consumption): ~$0
- Application Insights: ~$0.01

**Total: ~$0.12/day or ~$3.60/month**

## Cleanup

To delete all resources:

```bash
# Delete the entire resource group
az group delete --name rg-invoice-automation --yes --no-wait
```

This will delete:
- All Azure resources
- All data (invoices, database records)
- All logs

**Note**: Deletion is permanent and cannot be undone.

## Troubleshooting

### Issue: "Resource name already exists"
**Solution**: Change the `baseName` parameter to something unique

### Issue: "Azure OpenAI access denied"
**Solution**: Apply for access at https://aka.ms/oai/access (may take 1-2 business days)

### Issue: "Form Recognizer free tier limit reached"
**Solution**: Use `S0` sku instead of `F0` in main.bicep (costs ~$1/month)

### Issue: "Deployment failed - InvalidTemplate"
**Solution**: Validate template with:
```bash
az deployment group validate \
  --resource-group rg-invoice-automation \
  --template-file main.bicep \
  --parameters ownerEmail="test@example.com"
```

## Manual Configuration

If you need to manually configure `local.settings.json`:

```bash
# Get storage connection string
az storage account show-connection-string \
  --name <storage-account-name> \
  --resource-group rg-invoice-automation

# Get Cosmos DB endpoint and key
az cosmosdb show \
  --name <cosmos-account-name> \
  --resource-group rg-invoice-automation \
  --query documentEndpoint

az cosmosdb keys list \
  --name <cosmos-account-name> \
  --resource-group rg-invoice-automation \
  --query primaryMasterKey

# Get Form Recognizer endpoint and key
az cognitiveservices account show \
  --name <form-recognizer-name> \
  --resource-group rg-invoice-automation \
  --query properties.endpoint

az cognitiveservices account keys list \
  --name <form-recognizer-name> \
  --resource-group rg-invoice-automation \
  --query key1

# Get Azure OpenAI endpoint and key
az cognitiveservices account show \
  --name <openai-name> \
  --resource-group rg-invoice-automation \
  --query properties.endpoint

az cognitiveservices account keys list \
  --name <openai-name> \
  --resource-group rg-invoice-automation \
  --query key1
```

## Deployment to Production

For production deployment:

1. Update `main.bicepparam`:
   - Set `environment = 'prod'`
   - Choose production-appropriate location

2. Consider:
   - Enabling Azure AD authentication
   - Setting up VNET integration
   - Enabling backup for Cosmos DB
   - Upgrading to Standard App Service Plan for better performance
   - Setting up staging slots

3. Deploy with:
   ```bash
   ./deploy.sh
   ```

## Next Steps

After successful deployment:

1. **Test locally**:
   ```bash
   cd ../backend
   dotnet restore
   func start
   ```

2. **Deploy backend**:
   ```bash
   func azure functionapp publish <function-app-name>
   ```

3. **Set up frontend**:
   ```bash
   cd ../frontend
   echo "NEXT_PUBLIC_API_URL=<function-app-url>" > .env.local
   npm install
   npm run dev
   ```

## Support

For issues with:
- **Bicep syntax**: https://learn.microsoft.com/azure/azure-resource-manager/bicep/
- **Azure resources**: https://docs.microsoft.com/azure/
- **Deployment errors**: Check Azure Portal > Deployments for detailed error messages
