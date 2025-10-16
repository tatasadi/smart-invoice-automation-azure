// Parameters file for Smart Invoice Automation deployment
using './main.bicep'

// Base name for resources (keep it short, no special characters)
param baseName = 'invoice-automation'

// Location - choose closest to you
// Options: eastus, westus, westeurope, northeurope, etc.
param location = 'eastus'

// Environment
param environment = 'dev'

// Your email (for resource tagging)
param ownerEmail = 'your.email@example.com'
