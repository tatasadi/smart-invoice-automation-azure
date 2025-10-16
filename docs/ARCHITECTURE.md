# System Architecture

This document provides a detailed overview of the Smart Invoice Automation system architecture.

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         User Layer                           │
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │   Desktop    │    │    Mobile    │    │    Tablet    │  │
│  │   Browser    │    │   Browser    │    │   Browser    │  │
│  └──────────────┘    └──────────────┘    └──────────────┘  │
└─────────────────┬───────────────────────────────────────────┘
                  │ HTTPS
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│                                                              │
│              ┌──────────────────────────┐                   │
│              │   Next.js 15 Frontend    │                   │
│              │   - React 19 Components  │                   │
│              │   - Tailwind CSS         │                   │
│              │   - Client-side routing  │                   │
│              └───────────┬──────────────┘                   │
└────────────────────────────┼────────────────────────────────┘
                             │ REST API (HTTPS)
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                        │
│                                                              │
│              ┌──────────────────────────┐                   │
│              │   Azure Functions v4     │                   │
│              │   (.NET 8 Isolated)      │                   │
│              │                          │                   │
│              │  ┌────────────────────┐  │                   │
│              │  │ Upload Function    │  │                   │
│              │  │ Get Invoices       │  │                   │
│              │  │ Get Invoice by ID  │  │                   │
│              │  └────────────────────┘  │                   │
│              └───────────┬──────────────┘                   │
└────────────────────────────┼────────────────────────────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌────────────────┐
│   Data Layer    │  │    AI Layer     │  │  Storage Layer │
│                 │  │                 │  │                │
│ ┌─────────────┐ │  │ ┌─────────────┐ │  │ ┌────────────┐ │
│ │  Cosmos DB  │ │  │ │    Form     │ │  │ │    Blob    │ │
│ │ (Serverless)│ │  │ │ Recognizer  │ │  │ │  Storage   │ │
│ └─────────────┘ │  │ └─────────────┘ │  │ └────────────┘ │
│                 │  │                 │  │                │
│                 │  │ ┌─────────────┐ │  │                │
│                 │  │ │ Azure OpenAI│ │  │                │
│                 │  │ │  (GPT-4o)   │ │  │                │
│                 │  │ └─────────────┘ │  │                │
└─────────────────┘  └─────────────────┘  └────────────────┘
```

---

## Component Details

### 1. Frontend (Next.js 15)

**Technology Stack:**
- Next.js 15 with App Router
- React 19
- TypeScript
- Tailwind CSS

**Key Components:**
- **Upload Component**: Drag-and-drop file upload with validation
- **Results Display**: Shows extracted invoice data
- **Dashboard**: Lists all processed invoices
- **API Client**: Handles communication with backend

**Responsibilities:**
- User interface and experience
- File upload handling
- Results visualization
- Client-side state management

**Deployment:**
- Azure Static Web Apps (recommended)
- Automatic HTTPS
- Global CDN distribution

---

### 2. Backend (Azure Functions)

**Technology Stack:**
- Azure Functions v4
- .NET 8 (Isolated Worker Process)
- C# 12
- Consumption Plan (serverless)

**Functions:**

#### `POST /api/upload`
- Accepts multipart/form-data file upload
- Validates file type (PDF, PNG, JPG)
- Orchestrates the processing pipeline
- Returns processed invoice data

#### `GET /api/invoices`
- Queries Cosmos DB for all invoices
- Returns paginated results
- Supports basic filtering

#### `GET /api/invoice/{id}`
- Retrieves specific invoice by ID
- Returns full invoice details

**Responsibilities:**
- API gateway
- Request validation
- Service orchestration
- Error handling
- Response formatting

---

### 3. Storage Layer

#### Azure Blob Storage
**Purpose:** Store original invoice files

**Configuration:**
- Container: `invoices`
- Access: Private (SAS tokens for access)
- Redundancy: LRS (Locally Redundant Storage)

**File Organization:**
```
/invoices/
  ├── 2024/
  │   ├── 01/
  │   │   ├── invoice-uuid-1.pdf
  │   │   ├── invoice-uuid-2.png
  │   └── 02/
  │       └── invoice-uuid-3.pdf
```

#### Azure Cosmos DB
**Purpose:** Store invoice metadata and extracted data

**Configuration:**
- API: Core (SQL)
- Mode: Serverless (cost-effective for demo)
- Database: `InvoiceDB`
- Container: `Invoices`
- Partition Key: `/id`

**Document Schema:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fileName": "invoice-2024-01.pdf",
  "blobUrl": "https://storage.../invoices/2024/01/invoice-uuid.pdf",
  "uploadDate": "2024-01-15T10:30:00.000Z",
  "extractedData": {
    "vendor": "Acme Corporation",
    "vendorAddress": "123 Main St, City, State 12345",
    "invoiceNumber": "INV-2024-001",
    "invoiceDate": "2024-01-10",
    "dueDate": "2024-02-10",
    "totalAmount": 1299.99,
    "currency": "USD",
    "lineItems": [
      {
        "description": "Professional Services",
        "quantity": 40,
        "unitPrice": 32.50,
        "amount": 1299.99
      }
    ]
  },
  "classification": {
    "category": "Professional Services",
    "confidence": 0.95,
    "reasoning": "Invoice for consulting and professional services"
  },
  "processingMetadata": {
    "processingTime": 3.24,
    "formRecognizerConfidence": 0.98,
    "status": "completed",
    "errors": []
  }
}
```

---

### 4. AI Layer

#### Azure Form Recognizer (Document Intelligence)

**Purpose:** Extract structured data from invoice documents

**Model:** Prebuilt Invoice Model

**Capabilities:**
- Vendor name and address extraction
- Invoice number and dates
- Line items with descriptions and amounts
- Tax and total calculations
- Multi-page invoice support

**API Flow:**
```
1. Upload invoice to Blob Storage
2. Generate SAS URL for Form Recognizer access
3. Call Form Recognizer async API
4. Poll for completion (typically 2-3 seconds)
5. Parse and structure results
```

#### Azure OpenAI (GPT-4o)

**Purpose:** Classify invoices into business expense categories

**Model:** GPT-4o
**Deployment:** Standard S0 tier

**Classification Categories:**
- Marketing & Advertising
- IT Services & Software
- Office Supplies
- Utilities
- Professional Services
- Travel & Entertainment
- Equipment & Hardware
- Maintenance & Repairs
- Other

**Prompt Strategy:**
```
System: You are an expert accountant specializing in expense classification.

User: Classify this invoice:
Vendor: {vendor}
Amount: {amount}
Line Items: {items}

Return JSON: {"category": "...", "confidence": 0.0-1.0, "reasoning": "..."}
```

---

## Data Flow

### Invoice Upload and Processing Flow

```
1. User uploads file (PDF/PNG/JPG)
   ↓
2. Frontend validates file type and size
   ↓
3. POST request to /api/upload
   ↓
4. Azure Function receives file
   ↓
5. Upload to Blob Storage
   ├─ Generate unique filename (UUID)
   ├─ Organize by date (YYYY/MM/)
   └─ Return blob URL
   ↓
6. Call Form Recognizer
   ├─ Generate SAS token for blob access
   ├─ Submit document for analysis
   ├─ Poll for results (async)
   └─ Parse extracted fields
   ↓
7. Call Azure OpenAI for classification
   ├─ Format extracted data as prompt
   ├─ Request category classification
   └─ Receive category + confidence
   ↓
8. Save to Cosmos DB
   ├─ Combine all data into document
   ├─ Generate unique ID
   └─ Store in Invoices container
   ↓
9. Return response to frontend
   ├─ Include all extracted data
   ├─ Include classification
   └─ Include processing metadata
   ↓
10. Frontend displays results
```

**Typical Processing Time:** 3-5 seconds

---

## Security Architecture

### Authentication & Authorization
- **Demo/Development**: Public endpoints (no auth)
- **Production**: Would use Azure AD B2C or Managed Identity

### Data Security
- **In Transit**: HTTPS/TLS 1.2+
- **At Rest**: Azure default encryption
- **Blob Access**: Private containers with SAS tokens
- **Database**: Firewall rules limiting access

### API Security
- **CORS**: Configured for frontend domain only
- **Rate Limiting**: Azure Function consumption plan limits
- **Input Validation**: File type and size validation
- **Error Handling**: No sensitive data in error messages

---

## Scalability

### Horizontal Scaling
- **Frontend**: CDN-distributed static files
- **Backend**: Azure Functions auto-scale based on load
- **Database**: Cosmos DB serverless auto-scales
- **Storage**: Blob Storage scales automatically

### Performance Optimization
- **Frontend**: Code splitting, lazy loading
- **Backend**: Async processing, connection pooling
- **Database**: Proper indexing on partition key
- **Caching**: CDN caching for static assets

### Cost Optimization
- **Serverless architecture**: Pay only for usage
- **Cosmos DB serverless**: No minimum throughput
- **Free tiers**: Form Recognizer F0 (500 pages/month)
- **Consumption plan**: 1M free function executions

---

## Monitoring & Observability

### Application Insights 
- Function execution metrics
- Error tracking and logging
- Performance monitoring
- Custom event tracking

### Metrics to Track
- Invoice processing success rate
- Average processing time
- Form Recognizer confidence scores
- Classification accuracy
- API response times
- Error rates by type

---

## Error Handling

### Error Types & Responses

**File Upload Errors:**
- Invalid file type → 400 Bad Request
- File too large → 413 Payload Too Large
- Upload failed → 500 Internal Server Error

**Processing Errors:**
- Form Recognizer timeout → Retry with exponential backoff
- Low confidence extraction → Flag for manual review
- OpenAI API failure → Fallback to default category

**Database Errors:**
- Connection failure → Retry logic
- Duplicate ID → Regenerate UUID
- Query timeout → Return partial results

---

## Deployment Architecture

### Development Environment
```
Local Machine
├── Frontend: localhost:3000
├── Backend: localhost:7071
└── Azure Resources: Cloud
```

### Production Environment
```
Azure Cloud
├── Frontend: Azure Static Web Apps
│   └── Custom domain with HTTPS
├── Backend: Azure Function App
│   └── Consumption plan
└── Data & AI Services
    ├── Blob Storage
    ├── Cosmos DB
    ├── Form Recognizer
    └── Azure OpenAI
```

---

## Future Enhancements

### Scalability
- Implement queue-based processing (Azure Queue Storage)
- Add Redis cache for frequently accessed invoices
- Implement CDN for blob storage

### Features
- Duplicate invoice detection
- Batch upload support
- Export to CSV/Excel
- Advanced search and filtering
- Approval workflows

### Security
- Azure AD authentication
- Role-based access control (RBAC)
- Data retention policies
- Audit logging

### AI/ML
- Custom Form Recognizer model training
- Multi-language support
- Confidence threshold tuning
- Manual correction feedback loop

---

## Technology Choices & Rationale

| Choice | Rationale |
|--------|-----------|
| **Next.js 15** | Modern React framework, excellent DX, SSR support |
| **Azure Functions** | Serverless, auto-scaling, pay-per-use |
| **Cosmos DB Serverless** | No minimum cost, auto-scaling, globally distributed |
| **Form Recognizer** | Pretrained invoice model, high accuracy |
| **GPT-4o** | Best classification accuracy, cost-effective |
| **TypeScript** | Type safety, better DX, fewer runtime errors |
| **Tailwind CSS** | Rapid UI development, consistent design |

---

## Conclusion

This architecture provides:
✅ **Scalability** - Serverless components auto-scale
✅ **Cost-effectiveness** - Pay only for usage
✅ **Reliability** - Managed services with SLAs
✅ **Performance** - Async processing, optimized queries
✅ **Maintainability** - Clear separation of concerns
✅ **Extensibility** - Easy to add new features
