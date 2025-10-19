# Smart Invoice Automation - Frontend

A modern Next.js 15 web application for AI-powered invoice processing using Azure services.

## 🚀 Features

- **Drag & Drop Upload**: Intuitive file upload interface supporting PDF and image formats
- **Real-time Processing**: Upload invoices and get AI-extracted data in seconds
- **Invoice Dashboard**: View all processed invoices with statistics and insights
- **Document Viewer**: View uploaded invoice documents (PDF/images) inline
- **Category Classification**: Automatic invoice categorization with confidence scores
- **Responsive Design**: Mobile-friendly interface with Azure-themed styling

## 📋 Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **Next.js** | 15.5.6 | React framework with App Router |
| **React** | 19.1.0 | UI library |
| **TypeScript** | 5.x | Type safety |
| **Tailwind CSS** | 4.x | Styling and responsive design |
| **Axios** | 1.12.2 | HTTP client for API calls |
| **React Dropzone** | 14.3.8 | File drag & drop functionality |
| **Lucide React** | 0.546.0 | Icon library |
| **date-fns** | 4.1.0 | Date formatting utilities |

## 📁 Project Structure

```
frontend/
├── app/                          # Next.js App Router
│   ├── layout.tsx                # Root layout with navigation
│   ├── page.tsx                  # Home page (upload interface)
│   ├── dashboard/
│   │   └── page.tsx              # Dashboard with statistics
│   └── invoice/[id]/
│       └── page.tsx              # Invoice detail view
├── components/                   # React components
│   ├── FileUpload.tsx            # Drag & drop file upload
│   ├── InvoiceResults.tsx        # Display extracted invoice data
│   ├── InvoiceList.tsx           # Table of invoices
│   ├── InvoiceDocumentViewer.tsx # PDF/image viewer
│   └── ui/
│       └── button.tsx            # Reusable button component
├── lib/
│   └── api.ts                    # API client and type definitions
├── .env.local.example            # Environment variable template
└── package.json                  # Dependencies and scripts
```

## 🎨 Key Components

### FileUpload
- Drag & drop interface using `react-dropzone`
- File validation (PDF, PNG, JPG, JPEG, TIFF, BMP)
- Loading states with animated spinner
- Error handling and display
- Accepts single file only

### InvoiceResults
- Card-based layout for extracted data
- Displays vendor, invoice number, date, amount
- Shows line items if available
- Category classification with confidence score
- Visual confidence indicators
- Processing time metrics

### InvoiceList
- Responsive table/grid of invoices
- Click to view detailed invoice
- Shows vendor, amount, date, category
- Empty state when no invoices
- Loading skeleton states

### InvoiceDocumentViewer
- Displays PDF files and images inline
- Fetches secure SAS URLs from backend
- Fallback for unsupported formats
- Error handling for missing documents

### Dashboard
- Statistics cards: total invoices, total amount, avg processing time, categories
- Category breakdown visualization
- Recent invoices list
- Real-time data from Cosmos DB

## 🛠️ Setup & Installation

### Prerequisites
- Node.js 20.x or higher
- npm, yarn, pnpm, or bun package manager
- Backend Azure Functions running (see `../backend/README.md`)

### Installation

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Configure environment variables**

   Copy the example file:
   ```bash
   cp .env.local.example .env.local
   ```

   Edit `.env.local`:
   ```env
   # For local development (backend running on localhost:7071)
   NEXT_PUBLIC_API_URL=http://localhost:7071

   # For production (deployed Azure Function)
   NEXT_PUBLIC_API_URL=https://your-function-app.azurewebsites.net
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```

   Open [http://localhost:3000](http://localhost:3000) in your browser.

## 🚀 Available Scripts

```bash
# Run development server with Turbopack (faster builds)
npm run dev

# Build for production
npm run build

# Start production server
npm start

# Run ESLint
npm run lint
```

## 🔌 API Integration

The frontend communicates with the backend Azure Functions through a REST API.

### API Client (`lib/api.ts`)

**Key Functions:**
- `uploadInvoice(file: File): Promise<Invoice>` - Upload and process invoice
- `getInvoices(): Promise<Invoice[]>` - Fetch all invoices
- `getInvoice(id: string, vendorId: string): Promise<Invoice>` - Get single invoice
- `getInvoiceBlobSasUrl(blobUrl: string): Promise<string>` - Get secure blob URL
- `getInvoiceBlobUrl(...)` - Proxy URL for blob access

### Type Definitions

All types are aligned with backend C# models:

```typescript
export type Invoice = {
  id: string;
  vendorId: string;
  fileName: string;
  blobUrl: string;
  uploadDate: string;
  extractedData?: ExtractedData | null;
  classification?: Classification | null;
  processingMetadata?: ProcessingMetadata | null;
};

export type ExtractedData = {
  vendor: string;
  invoiceNumber: string;
  invoiceDate: string;
  totalAmount: number;
  currency: string;
  lineItems?: LineItem[];
};

export type Classification = {
  category: string;
  confidence: number; // 0-1
  reasoning?: string | null;
};
```

## 🎯 Key Features Explained

### 1. File Upload Flow
1. User drags/drops or selects file
2. Frontend validates file type
3. File sent as binary stream with `X-File-Name` header
4. Backend processes with Azure AI services
5. Results displayed immediately on success

### 2. Data Normalization
The API client includes robust normalization to handle both camelCase and PascalCase responses from the backend, ensuring consistent data structure throughout the app.

### 3. Error Handling
- Network errors caught and displayed
- Invalid file types prevented at upload
- Missing data handled gracefully with fallbacks
- Loading states prevent UI flickering

### 4. Responsive Design
- Mobile-first approach
- Grid layouts adapt to screen size
- Touch-friendly upload zone
- Optimized for tablets and desktops

## 🎨 Styling

The app uses **Azure Blue** (`#0078D4`) as the primary theme color, matching the Microsoft Azure brand.

**Key Design Elements:**
- Clean, modern card-based layouts
- Subtle shadows and borders
- Smooth transitions and hover effects
- Loading animations for better UX
- Consistent spacing and typography

## 📱 Pages Overview

### Home Page (`/`)
- Hero section with project description
- File upload component
- Displays results after successful upload
- Quick stats about the feature

### Dashboard Page (`/dashboard`)
- Statistics cards (totals, averages)
- Category breakdown visualization
- List of all processed invoices
- Navigation to individual invoices

### Invoice Detail Page (`/invoice/[id]`)
- Full invoice details
- Document viewer (PDF/image)
- All extracted fields
- Classification results
- Processing metadata

## 🔒 Security Considerations

- **No authentication** in current version (demo/POC only)
- **SAS URLs**: Backend generates time-limited secure URLs for blob access
- **CORS**: Configure allowed origins in Azure Function settings
- **Environment variables**: Never commit `.env.local` to version control

### Production Recommendations:
- Implement authentication (Azure AD, Auth0, etc.)
- Add rate limiting
- Enable HTTPS only
- Implement CSRF protection
- Add input sanitization
- Set up monitoring and logging

## 🌐 Deployment

### Deploy to Vercel (Recommended)

1. **Install Vercel CLI**
   ```bash
   npm i -g vercel
   ```

2. **Deploy**
   ```bash
   vercel deploy --prod
   ```

3. **Set environment variables in Vercel dashboard**
   - `NEXT_PUBLIC_API_URL` → Your Azure Function URL

### Deploy to Azure Static Web Apps

1. **Build the app**
   ```bash
   npm run build
   ```

2. **Deploy using Azure CLI**
   ```bash
   az staticwebapp create \
     --name smart-invoice-frontend \
     --resource-group <your-resource-group> \
     --source ./out \
     --location eastus \
     --branch main
   ```

3. **Configure environment variable**
   - Add `NEXT_PUBLIC_API_URL` in Azure Portal

## 🧪 Testing Locally

1. **Start backend** (Azure Functions)
   ```bash
   cd ../backend/InvoiceAutomation
   func start
   ```

2. **Start frontend** (in new terminal)
   ```bash
   cd frontend
   npm run dev
   ```

3. **Test the flow**
   - Open http://localhost:3000
   - Upload a sample invoice (PDF or image)
   - Verify data extraction
   - Check dashboard for statistics
   - View invoice details

## 🐛 Troubleshooting

### CORS Errors
**Problem**: Browser blocks requests to backend

**Solution**:
- For local development, backend CORS is configured to allow `http://localhost:3000`
- For production, add your frontend URL to `host.json` CORS settings in backend

### API Connection Failed
**Problem**: Frontend can't reach backend

**Solution**:
- Verify `.env.local` has correct `NEXT_PUBLIC_API_URL`
- Ensure backend is running (`func start`)
- Check browser console for exact error
- Verify backend functions are accessible (try in Postman)

### File Upload Fails
**Problem**: Upload returns 4xx or 5xx error

**Solution**:
- Check file size (Azure has limits)
- Verify file format is supported
- Check backend logs for specific error
- Ensure Azure services (Blob, Form Recognizer, OpenAI) are configured

### Document Viewer Not Working
**Problem**: Can't view uploaded invoice document

**Solution**:
- Verify backend has `GetInvoiceBlob` function
- Check SAS URL generation in backend
- Ensure blob storage is accessible
- Check browser console for errors

## 📚 Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [React Documentation](https://react.dev)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Azure Static Web Apps](https://learn.microsoft.com/en-us/azure/static-web-apps/)
- [Vercel Deployment](https://vercel.com/docs)

## 🤝 Integration with Backend

This frontend is designed to work with the `.NET 8 Azure Functions` backend located in `../backend/InvoiceAutomation/`.

**Backend Endpoints Used:**
- `POST /api/upload` - Upload and process invoice
- `GET /api/invoices` - Get all invoices
- `GET /api/invoice/{id}?vendorId={vendorId}` - Get single invoice
- `GET /api/invoice/blob/{id}?vendorId={vendorId}&blobUrl={blobUrl}` - Get invoice document
- `GET /api/blob/sas?blobUrl={blobUrl}` - Get SAS URL for blob

## 📄 License

This is a portfolio/demo project. Feel free to use and modify as needed.

## 👤 Author

Built as a demonstration of Azure AI services integration with modern web technologies.

---

**Built with ❤️ using Next.js 15, React 19, and Azure AI Services**
