import axios from "axios";

// Types aligned with backend models
export type LineItem = {
  description: string;
  quantity?: number;
  unitPrice?: number;
  amount: number;
};

export type ExtractedData = {
  vendor: string;
  invoiceNumber: string;
  invoiceDate: string; // keep as string, backend returns string
  totalAmount: number;
  currency: string;
  lineItems?: LineItem[];
};

export type Classification = {
  category: string;
  confidence: number; // 0-1
  reasoning?: string | null;
};

export type ProcessingMetadata = {
  processingTime: number;
  formRecognizerConfidence: number;
  status: string;
  errorMessage?: string | null;
  startTime: string;
  endTime?: string | null;
};

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

type ListResponse = {
  count: number;
  invoices: Invoice[];
};

function getApiBase() {
  const base = (process.env.NEXT_PUBLIC_API_URL || "").replace(/\/$/, "");
  // If env already ends with /api, use as-is; otherwise append /api
  return base.endsWith("/api") ? base : `${base}/api`;
}

export async function uploadInvoice(file: File): Promise<Invoice> {
  const apiBase = getApiBase();
  const url = `${apiBase}/upload`;

  // Backend expects raw file stream (not multipart) with X-File-Name header
  const res = await axios.post(url, await file.arrayBuffer(), {
    headers: {
      "Content-Type": file.type || "application/octet-stream",
      "X-File-Name": file.name,
    },
  });
  return normalizeInvoice(res.data);
}

export async function getInvoices(): Promise<Invoice[]> {
  const apiBase = getApiBase();
  const url = `${apiBase}/invoices`;
  const res = await axios.get<ListResponse>(url);
  const list: any[] = (res.data as any)?.invoices ?? [];
  return list.map(normalizeInvoice);
}

export async function getInvoice(id: string, vendorId: string): Promise<Invoice> {
  const apiBase = getApiBase();
  const url = `${apiBase}/invoice/${encodeURIComponent(id)}?vendorId=${encodeURIComponent(vendorId)}`;
  const res = await axios.get<Invoice>(url);
  return normalizeInvoice(res.data);
}

// --- Helpers ---
function normalizeInvoice(input: any): Invoice {
  if (!input || typeof input !== "object") {
    // Fallback empty object to keep UI stable
    return {
      id: "",
      vendorId: "",
      fileName: "",
      blobUrl: "",
      uploadDate: "",
      extractedData: undefined,
      classification: undefined,
      processingMetadata: undefined,
    };
  }

  const exSrc = input.extractedData ?? input.ExtractedData;
  const clsSrc = input.classification ?? input.Classification;
  const pmSrc = input.processingMetadata ?? input.ProcessingMetadata;

  const extractedData: ExtractedData | null | undefined = exSrc
    ? {
        vendor: exSrc.vendor ?? exSrc.Vendor ?? "",
        invoiceNumber: exSrc.invoiceNumber ?? exSrc.InvoiceNumber ?? "",
        invoiceDate: exSrc.invoiceDate ?? exSrc.InvoiceDate ?? "",
        totalAmount: numberOr(exSrc.totalAmount ?? exSrc.TotalAmount),
        currency: exSrc.currency ?? exSrc.Currency ?? "",
        lineItems: normalizeLineItems(exSrc.lineItems ?? exSrc.LineItems),
      }
    : undefined;

  const classification: Classification | null | undefined = clsSrc
    ? {
        category: clsSrc.category ?? clsSrc.Category ?? "",
        confidence: numberOr(clsSrc.confidence ?? clsSrc.Confidence),
        reasoning: clsSrc.reasoning ?? clsSrc.Reasoning ?? null,
      }
    : undefined;

  const processingMetadata: ProcessingMetadata | null | undefined = pmSrc
    ? {
        processingTime: numberOr(pmSrc.processingTime ?? pmSrc.ProcessingTime),
        formRecognizerConfidence: numberOr(
          pmSrc.formRecognizerConfidence ?? pmSrc.FormRecognizerConfidence
        ),
        status: pmSrc.status ?? pmSrc.Status ?? "",
        errorMessage: pmSrc.errorMessage ?? pmSrc.ErrorMessage ?? null,
        startTime: pmSrc.startTime ?? pmSrc.StartTime ?? "",
        endTime: pmSrc.endTime ?? pmSrc.EndTime ?? null,
      }
    : undefined;

  return {
    id: input.id ?? input.Id ?? "",
    vendorId: input.vendorId ?? input.VendorId ?? "",
    fileName: input.fileName ?? input.FileName ?? "",
    blobUrl: input.blobUrl ?? input.BlobUrl ?? "",
    uploadDate: input.uploadDate ?? input.UploadDate ?? "",
    extractedData,
    classification,
    processingMetadata,
  };
}

function normalizeLineItems(src: any): LineItem[] | undefined {
  if (!Array.isArray(src)) return undefined;
  return src
    .filter((x) => x && typeof x === "object")
    .map((li: any) => ({
      description: li.description ?? li.Description ?? "",
      quantity: numberOr(li.quantity ?? li.Quantity, undefined),
      unitPrice: numberOr(li.unitPrice ?? li.UnitPrice, undefined),
      amount: numberOr(li.amount ?? li.Amount),
    }));
}

function numberOr(value: any, fallback: any = 0): any {
  if (value == null) return fallback;
  const n = typeof value === "number" ? value : Number(value);
  return Number.isFinite(n) ? n : fallback;
}
