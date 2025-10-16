#!/bin/bash

# Smart Invoice Automation - Deployment Script
# This script deploys all Azure resources using Bicep

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Smart Invoice Automation - Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI is not installed${NC}"
    echo "Install it from: https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in
echo -e "${YELLOW}Checking Azure login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Not logged in. Logging in...${NC}"
    az login
fi

# Variables
RESOURCE_GROUP="rg-invoice-automation"
LOCATION="eastus"
DEPLOYMENT_NAME="invoice-automation-$(date +%Y%m%d-%H%M%S)"

# Prompt for parameters
echo ""
echo -e "${YELLOW}Please provide the following information:${NC}"
echo ""

read -p "Resource Group Name [$RESOURCE_GROUP]: " input_rg
RESOURCE_GROUP="${input_rg:-$RESOURCE_GROUP}"

read -p "Location [$LOCATION]: " input_location
LOCATION="${input_location:-$LOCATION}"

read -p "Your email (for tagging): " OWNER_EMAIL
if [ -z "$OWNER_EMAIL" ]; then
    echo -e "${RED}Error: Email is required${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}Deployment Configuration:${NC}"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  Owner Email: $OWNER_EMAIL"
echo ""

read -p "Continue with deployment? (y/n): " confirm
if [ "$confirm" != "y" ]; then
    echo "Deployment cancelled"
    exit 0
fi

# Create resource group
echo ""
echo -e "${YELLOW}Creating resource group...${NC}"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none

echo -e "${GREEN}✓ Resource group created${NC}"

# Deploy Bicep template
echo ""
echo -e "${YELLOW}Deploying Azure resources (this may take 5-10 minutes)...${NC}"
az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters ownerEmail="$OWNER_EMAIL" location="$LOCATION" \
    --output none

echo -e "${GREEN}✓ Resources deployed${NC}"

# Get outputs
echo ""
echo -e "${YELLOW}Retrieving deployment outputs...${NC}"

STORAGE_CONNECTION=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.storageConnectionString.value \
    --output tsv)

COSMOS_ENDPOINT=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.cosmosEndpoint.value \
    --output tsv)

COSMOS_KEY=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.cosmosKey.value \
    --output tsv)

FORM_RECOGNIZER_ENDPOINT=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.formRecognizerEndpoint.value \
    --output tsv)

FORM_RECOGNIZER_KEY=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.formRecognizerKey.value \
    --output tsv)

OPENAI_ENDPOINT=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.openAIEndpoint.value \
    --output tsv)

OPENAI_KEY=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.openAIKey.value \
    --output tsv)

FUNCTION_APP_NAME=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.functionAppName.value \
    --output tsv)

FUNCTION_APP_URL=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.functionAppUrl.value \
    --output tsv)

# Save credentials to file
CREDENTIALS_FILE="../backend/local.settings.json"

echo ""
echo -e "${YELLOW}Creating local.settings.json...${NC}"

cat > "$CREDENTIALS_FILE" << EOF
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "AzureStorageConnectionString": "$STORAGE_CONNECTION",
    "AzureStorageContainerName": "invoices",

    "CosmosDbEndpoint": "$COSMOS_ENDPOINT",
    "CosmosDbKey": "$COSMOS_KEY",
    "CosmosDbDatabaseName": "InvoiceDB",
    "CosmosDbContainerName": "Invoices",

    "FormRecognizerEndpoint": "$FORM_RECOGNIZER_ENDPOINT",
    "FormRecognizerKey": "$FORM_RECOGNIZER_KEY",

    "AzureOpenAIEndpoint": "$OPENAI_ENDPOINT",
    "AzureOpenAIKey": "$OPENAI_KEY",
    "AzureOpenAIDeploymentName": "gpt-4o",
    "AzureOpenAIApiVersion": "2024-08-01-preview"
  }
}
EOF

echo -e "${GREEN}✓ Configuration saved to $CREDENTIALS_FILE${NC}"

# Display summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Deployment Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "${YELLOW}Deployed Resources:${NC}"
echo "  • Storage Account"
echo "  • Cosmos DB (Serverless)"
echo "  • Form Recognizer (Free tier)"
echo "  • Azure OpenAI (with GPT-4o deployment)"
echo "  • Function App (.NET 8)"
echo "  • Application Insights"
echo ""
echo -e "${YELLOW}Function App:${NC}"
echo "  Name: $FUNCTION_APP_NAME"
echo "  URL: $FUNCTION_APP_URL"
echo ""
echo -e "${YELLOW}Configuration:${NC}"
echo "  File: $CREDENTIALS_FILE"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "  1. cd ../backend"
echo "  2. dotnet restore"
echo "  3. func start"
echo "  4. Test locally, then deploy: func azure functionapp publish $FUNCTION_APP_NAME"
echo ""
echo -e "${YELLOW}To delete all resources:${NC}"
echo "  az group delete --name $RESOURCE_GROUP --yes --no-wait"
echo ""
