// Smart Invoice Automation - Azure Infrastructure
// This Bicep template deploys all required Azure resources

targetScope = 'resourceGroup'

@description('Location for all resources')
param location string = 'swedencentral'

@description('Environment (dev, prod)')
@allowed([
  'dev'
  'prod'
])
param environment string = 'dev'

@description('Allowed CORS origins (use * for dev only)')
param allowedOrigins array = ['*']

@description('Partition key path for Cosmos DB (e.g., /vendorId, /customerId)')
param cosmosPartitionKeyPath string = '/vendorId'

// Variables
var storageAccountName = 'stinvoiceautomationta'
var cosmosAccountName = 'cosmos-invoice-automation-ta'
var formRecognizerName = 'form-invoice-automation-ta'
var openAIName = 'openai-invoice-automation-ta'
var functionAppName = 'func-invoice-automation-ta'
var appServicePlanName = 'asp-invoice-automation-ta'
var applicationInsightsName = 'appi-invoice-automation-ta'
var logAnalyticsName = 'log-invoice-automation-ta'

// Common tags
var commonTags = {
  Environment: environment
  Project: 'Smart Invoice Automation'
  ManagedBy: 'Bicep'
}

// Storage Account for invoices
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: commonTags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
  }
}

// Blob Service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: allowedOrigins
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
          allowedHeaders: ['*']
          exposedHeaders: ['*']
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

// Invoices Container
resource invoicesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'invoices'
  properties: {
    publicAccess: 'None'
  }
}

// Cosmos DB Account (Serverless)
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: cosmosAccountName
  location: location
  tags: commonTags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    enableFreeTier: false
    publicNetworkAccess: environment == 'prod' ? 'Disabled' : 'Enabled'
    networkAclBypassResourceIds: []
    ipRules: []
  }
}

// Cosmos DB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: 'InvoiceDB'
  properties: {
    resource: {
      id: 'InvoiceDB'
    }
  }
}

// Cosmos DB Container
resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: cosmosDatabase
  name: 'Invoices'
  properties: {
    resource: {
      id: 'Invoices'
      partitionKey: {
        paths: [cosmosPartitionKeyPath]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
      }
    }
  }
}

// Form Recognizer (Cognitive Services)
resource formRecognizer 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: formRecognizerName
  location: location
  tags: commonTags
  sku: {
    name: environment == 'prod' ? 'S0' : 'F0' // S0 for prod, F0 for dev
  }
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: formRecognizerName
    publicNetworkAccess: environment == 'prod' ? 'Disabled' : 'Enabled'
    networkAcls: environment == 'prod' ? {
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []
    } : null
  }
}

// Azure OpenAI
resource openAI 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIName
  location: location
  tags: commonTags
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAIName
    publicNetworkAccess: environment == 'prod' ? 'Disabled' : 'Enabled'
    networkAcls: environment == 'prod' ? {
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []
    } : null
  }
}

// Azure OpenAI Deployment (GPT-4o)
resource openAIDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAI
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-08-06'
    }
  }
}

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  tags: commonTags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: commonTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// App Service Plan (Consumption)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: commonTags
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  tags: commonTags
  kind: 'functionapp,linux'
  dependsOn: [
    invoicesContainer // Ensure storage structure is ready
  ]
  properties: {
    serverFarmId: appServicePlan.id
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      cors: {
        allowedOrigins: allowedOrigins
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        // Application Settings
        {
          name: 'AzureStorageConnectionString'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'AzureStorageContainerName'
          value: 'invoices'
        }
        {
          name: 'CosmosDbEndpoint'
          value: cosmosAccount.properties.documentEndpoint
        }
        {
          name: 'CosmosDbKey'
          value: cosmosAccount.listKeys().primaryMasterKey
        }
        {
          name: 'CosmosDbDatabaseName'
          value: 'InvoiceDB'
        }
        {
          name: 'CosmosDbContainerName'
          value: 'Invoices'
        }
        {
          name: 'FormRecognizerEndpoint'
          value: formRecognizer.properties.endpoint
        }
        {
          name: 'FormRecognizerKey'
          value: formRecognizer.listKeys().key1
        }
        {
          name: 'AzureOpenAIEndpoint'
          value: openAI.properties.endpoint
        }
        {
          name: 'AzureOpenAIKey'
          value: openAI.listKeys().key1
        }
        {
          name: 'AzureOpenAIDeploymentName'
          value: 'gpt-4o'
        }
        {
          name: 'AzureOpenAIApiVersion'
          value: '2024-08-01-preview'
        }
      ]
    }
    httpsOnly: true
  }
}

// Outputs
output storageAccountName string = storageAccount.name
@secure()
output storageConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
@secure()
output cosmosKey string = cosmosAccount.listKeys().primaryMasterKey
output formRecognizerEndpoint string = formRecognizer.properties.endpoint
@secure()
output formRecognizerKey string = formRecognizer.listKeys().key1
output openAIEndpoint string = openAI.properties.endpoint
@secure()
output openAIKey string = openAI.listKeys().key1
output functionAppName string = functionApp.name
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}/api'
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output resourceNames object = {
  storageAccount: storageAccountName
  cosmosAccount: cosmosAccountName
  formRecognizer: formRecognizerName
  openAI: openAIName
  functionApp: functionAppName
  appServicePlan: appServicePlanName
  applicationInsights: applicationInsightsName
  logAnalytics: logAnalyticsName
}
