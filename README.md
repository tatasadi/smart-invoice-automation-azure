# Smart Invoice Automation

> AI-powered serverless invoice processing platform built on Azure

[![Azure](https://img.shields.io/badge/Azure-0078D4?style=flat&logo=microsoft-azure&logoColor=white)](https://azure.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-15-black?style=flat&logo=next.js&logoColor=white)](https://nextjs.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

An end-to-end serverless solution that automatically processes invoice uploads, extracts key data using AI, and intelligently classifies them by category. Built to demonstrate modern cloud architecture and AI service integration.

### Key Features

🤖 **AI-Powered Extraction** - Automatically extracts vendor, amount, date, and line items from invoices

🏷️ **Intelligent Classification** - GPT-4o categorizes invoices into business expense categories

📊 **Real-time Dashboard** - View and manage all processed invoices in one place

⚡ **Serverless Architecture** - Scalable, cost-effective Azure Functions backend

🎨 **Modern UI** - Clean, responsive Next.js 15 interface

---

## Architecture

```
┌──────────────┐
│   Browser    │
└──────┬───────┘
       │
       ▼
┌──────────────────┐
│   Next.js 15     │
│   Frontend       │
└────────┬─────────┘
         │ HTTPS
         ▼
┌─────────────────────────┐
│   Azure Functions       │
│   (.NET 8 Isolated)     │
└───┬─────────┬──────┬────┘
    │         │      │
    ▼         ▼      ▼
┌────────┐ ┌─────────────┐ ┌────────────┐
│ Blob   │ │Form         │ │ Cosmos DB  │
│Storage │ │Recognizer   │ │(Serverless)│
└────────┘ └──────┬──────┘ └────────────┘
                  │
                  ▼
          ┌───────────────┐
          │ Azure OpenAI  │
          │   (GPT-4o)    │
          └───────────────┘
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

---

## Tech Stack

**Frontend**
- Next.js 15 with App Router
- React 19
- TypeScript
- Tailwind CSS

**Backend**
- Azure Functions (.NET 8 Isolated)
- C# 12
- Azure Blob Storage
- Azure Cosmos DB (Serverless)

**AI/ML**
- Azure Form Recognizer (Document Intelligence)
- Azure OpenAI Service (GPT-4o)

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+ (for frontend)
- Azure subscription
- Azure CLI
- Azure Functions Core Tools v4

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/tatasadi/smart-invoice-automation-azure.git
cd smart-invoice-automation-azure
```

2. **Set up Azure resources**

Create the following Azure resources:
- Resource Group
- Storage Account with Blob Container
- Cosmos DB (Serverless mode)
- Form Recognizer service
- Azure OpenAI with GPT-4o deployment
- Function App

3. **Configure backend**
```bash
cd backend
dotnet restore

# Create local.settings.json with your Azure credentials
cp local.settings.example.json local.settings.json
# Edit local.settings.json with your Azure service endpoints and keys
```

4. **Configure frontend**
```bash
cd frontend
npm install

# Create environment file
echo "NEXT_PUBLIC_API_URL=http://localhost:7071/api" > .env.local
```

5. **Run locally**

Terminal 1 (Backend):
```bash
cd backend
func start
```

Terminal 2 (Frontend):
```bash
cd frontend
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

---

## Usage

1. **Upload an invoice** - Drag and drop a PDF or image file
2. **Wait for processing** - AI extraction and classification typically takes 3-5 seconds
3. **View results** - See extracted data and category classification with confidence scores
4. **Browse dashboard** - View all processed invoices with search and filter options

---

## Project Structure

```
smart-invoice-automation-azure/
├── backend/              # Azure Functions (.NET 8)
│   ├── Functions/       # HTTP trigger functions
│   ├── Services/        # Azure service integrations
│   ├── Models/          # Data models and DTOs
│   ├── Program.cs
│   └── InvoiceAutomation.csproj
│
├── frontend/            # Next.js application
│   ├── app/            # App Router pages
│   ├── components/     # React components
│   ├── lib/            # Utilities and API client
│   └── package.json
│
├── docs/               # Documentation
│   └── ARCHITECTURE.md # System architecture details
│
└── samples/            # Sample invoice files
    └── invoices/
```

---

## API Endpoints

### `POST /api/upload`
Upload and process an invoice

**Request**: Multipart form data with invoice file

**Response**:
```json
{
  "id": "uuid",
  "fileName": "invoice.pdf",
  "extractedData": {
    "vendor": "Acme Corp",
    "invoiceNumber": "INV-12345",
    "date": "2024-01-15",
    "totalAmount": 1299.99,
    "currency": "USD"
  },
  "classification": {
    "category": "IT Services",
    "confidence": 0.95
  },
  "processingTime": 3.2
}
```

### `GET /api/invoices`
Retrieve all processed invoices

### `GET /api/invoice/{id}`
Retrieve a specific invoice by ID

---

## Deployment

### Deploy Backend

```bash
cd backend
func azure functionapp publish your-function-app-name
```

### Deploy Frontend

```bash
cd frontend
npm run build

# Deploy to Azure Static Web Apps
az staticwebapp create \
  --name your-app-name \
  --resource-group your-rg \
  --source .
```

---

## Performance

- **Processing Time**: 3-5 seconds per invoice
- **Extraction Accuracy**: 95%+ on standard invoices
- **Scalability**: Handles concurrent uploads automatically
- **Cost**: < $5/month for typical demo usage

---

## Security Notes

This is a demonstration project. For production use, consider implementing:

- Azure AD authentication
- API rate limiting
- Input validation and sanitization
- Data encryption at rest
- GDPR compliance measures
- Audit logging

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Author

**Ehsan Tatasadi**

- LinkedIn: [ehsan-tatasadi](https://linkedin.com/in/ehsan-tatasadi)
- GitHub: [@tatasadi](https://github.com/tatasadi)
- Portfolio: [ehsan.tatasadi.com](https://ehsan.tatasadi.com)

---

## Acknowledgments

- Built with Azure AI Services
- Powered by Next.js and React
- UI components styled with Tailwind CSS

---

**If this project helped you, please give it a ⭐️**
