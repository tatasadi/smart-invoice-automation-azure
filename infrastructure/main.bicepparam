// Parameters file for Smart Invoice Automation deployment
using './main.bicep'


// Location - must support Azure OpenAI
param location = 'swedencentral'

// Environment
param environment = 'dev'

// CORS allowed origins (use '*' for dev, specific URLs for prod)
// Example for prod: ['https://yourdomain.com', 'https://app.yourdomain.com']
param allowedOrigins = ['*']

// Cosmos DB Partition Key - choose based on your query patterns
// Options: '/vendorId', '/customerId', '/invoiceDate' (YYYY-MM format)
// IMPORTANT: Do NOT use '/id' as it creates poor partitioning
param cosmosPartitionKeyPath = '/vendorId'
