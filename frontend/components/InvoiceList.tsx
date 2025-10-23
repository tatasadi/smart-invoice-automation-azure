"use client";

import { useEffect, useState } from "react";
import { getInvoices, type Invoice } from "@/lib/api";
import { format } from "date-fns";
import Link from "next/link";

type Props = {
  invoices?: Invoice[];
  loading?: boolean;
};

export default function InvoiceList({ invoices: propInvoices, loading: propLoading }: Props) {
  const [data, setData] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // If props are provided, use them; otherwise fetch data
  const shouldFetch = propInvoices === undefined;

  useEffect(() => {
    if (!shouldFetch) {
      setData(propInvoices || []);
      setLoading(propLoading || false);
      return;
    }

    (async () => {
      try {
        const invoices = await getInvoices();
        setData(invoices);
      } catch (e: any) {
        setError(e?.message || "Failed to load invoices");
      } finally {
        setLoading(false);
      }
    })();
  }, [shouldFetch, propInvoices, propLoading]);

  if (loading) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm p-8">
        <div className="flex items-center justify-center">
          <div className="animate-pulse text-gray-600">Loading invoices...</div>
        </div>
      </div>
    );
  }

  if (error) return <div className="text-sm text-red-600">{error}</div>;

  if (!data.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm p-8">
        <div className="text-center text-gray-500">
          <p className="text-lg mb-2">No invoices yet</p>
          <p className="text-sm">Upload your first invoice to get started</p>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      {/* Header - hidden on mobile */}
      <div className="hidden md:grid md:grid-cols-5 text-left text-gray-600 text-sm border-b border-gray-200">
        <div className="py-3 pl-4 pr-4">Vendor</div>
        <div className="py-3 pr-4">Invoice #</div>
        <div className="py-3 pr-4">Amount</div>
        <div className="py-3 pr-4">Category</div>
        <div className="py-3 pr-4">Uploaded</div>
      </div>

      {/* Invoice items */}
      <div className="divide-y divide-gray-200">
        {data.map((inv, idx) => {
          const ex = inv.extractedData;
          const cls = inv.classification;
          const uploaded = inv.uploadDate ? new Date(inv.uploadDate) : null;
          const key = [inv.id, inv.vendorId, inv.fileName].filter(Boolean).join("|") || `row-${idx}`;
          const detailUrl = `/invoice/${encodeURIComponent(inv.id)}?vendorId=${encodeURIComponent(inv.vendorId)}`;

          return (
            <div
              key={key}
              className="hover:bg-gray-50 transition-colors cursor-pointer"
              onClick={() => window.location.href = detailUrl}
            >
              {/* Desktop grid layout */}
              <div className="hidden md:grid md:grid-cols-5 text-sm py-3">
                <div className="pl-4 pr-4 font-medium text-gray-900">{ex?.vendor || inv.vendorId}</div>
                <div className="pr-4 text-gray-700">{ex?.invoiceNumber || "-"}</div>
                <div className="pr-4 text-gray-900">{ex?.totalAmount != null ? ex.totalAmount.toFixed(2) : "-"}</div>
                <div className="pr-4">
                  <span className="inline-block text-xs px-2 py-1 rounded-md bg-blue-50 text-blue-700">
                    {cls?.category || "-"}
                  </span>
                </div>
                <div className="pr-4 text-gray-700">{uploaded ? format(uploaded, "PPpp") : "-"}</div>
              </div>

              {/* Mobile stacked layout */}
              <div className="md:hidden p-4 space-y-2 text-sm">
                <div className="font-medium text-gray-900">{ex?.vendor || inv.vendorId}</div>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <span className="text-gray-600">Invoice #: </span>
                    <span className="text-gray-700">{ex?.invoiceNumber || "-"}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Amount: </span>
                    <span className="text-gray-900">{ex?.totalAmount != null ? ex.totalAmount.toFixed(2) : "-"}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Category: </span>
                    <span className="inline-block text-xs px-2 py-1 rounded-md bg-blue-50 text-blue-700">
                      {cls?.category || "-"}
                    </span>
                  </div>
                  <div>
                    <span className="text-gray-600">Uploaded: </span>
                    <span className="text-gray-700">{uploaded ? format(uploaded, "PPpp") : "-"}</span>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
