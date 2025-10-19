"use client";

import { useEffect, useState } from "react";
import { useParams, useSearchParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Loader2 } from "lucide-react";
import { getInvoice, type Invoice } from "@/lib/api";
import InvoiceResults from "@/components/InvoiceResults";
import { Button } from "@/components/ui/button";

export default function InvoiceDetailPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const vendorId = searchParams.get("vendorId") || "";

  const [invoice, setInvoice] = useState<Invoice | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id || !vendorId) {
      setError("Missing invoice ID or vendor ID");
      setLoading(false);
      return;
    }

    (async () => {
      try {
        const data = await getInvoice(id, vendorId);
        setInvoice(data);
      } catch (e: any) {
        setError(e?.message || "Failed to load invoice");
      } finally {
        setLoading(false);
      }
    })();
  }, [id, vendorId]);

  if (loading) {
    return (
      <main className="mx-auto max-w-5xl px-6 py-10">
        <div className="flex items-center justify-center py-20">
          <Loader2 className="animate-spin text-[#0078D4]" size={48} />
        </div>
      </main>
    );
  }

  if (error || !invoice) {
    return (
      <main className="mx-auto max-w-5xl px-6 py-10">
        <div className="text-center py-20">
          <p className="text-red-600 text-lg mb-4">{error || "Invoice not found"}</p>
          <Link href="/dashboard">
            <Button>
              <ArrowLeft size={16} />
              Back to Dashboard
            </Button>
          </Link>
        </div>
      </main>
    );
  }

  return (
    <main className="mx-auto max-w-5xl px-6 py-10">
      <div className="mb-6 flex items-center gap-4">
        <Link href="/dashboard">
          <Button variant="outline" size="sm">
            <ArrowLeft size={16} />
            Back
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Invoice Details</h1>
          <p className="text-sm text-gray-600 mt-1">{invoice.fileName}</p>
        </div>
      </div>

      {invoice.blobUrl && (
        <div className="mb-6 rounded-xl border border-gray-200 p-4 bg-white">
          <a
            href={invoice.blobUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="text-[#0078D4] hover:underline text-sm font-medium"
          >
            View Original Document →
          </a>
        </div>
      )}

      <InvoiceResults invoice={invoice} />
    </main>
  );
}
