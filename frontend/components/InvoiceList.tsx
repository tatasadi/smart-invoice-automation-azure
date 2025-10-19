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
    <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
      <table className="min-w-full text-sm">
        <thead>
          <tr className="text-left text-gray-600">
            <th className="py-3 pl-4 pr-4">Vendor</th>
            <th className="py-3 pr-4">Invoice #</th>
            <th className="py-3 pr-4">Amount</th>
            <th className="py-3 pr-4">Category</th>
            <th className="py-3 pr-4">Uploaded</th>
          </tr>
        </thead>
        <tbody>
          {data.map((inv, idx) => {
            const ex = inv.extractedData;
            const cls = inv.classification;
            const uploaded = inv.uploadDate ? new Date(inv.uploadDate) : null;
            const key = [inv.id, inv.vendorId, inv.fileName].filter(Boolean).join("|") || `row-${idx}`;
            const detailUrl = `/invoice/${encodeURIComponent(inv.id)}?vendorId=${encodeURIComponent(inv.vendorId)}`;

            return (
              <tr
                key={key}
                className="border-t hover:bg-gray-50 transition-colors cursor-pointer"
                onClick={() => window.location.href = detailUrl}
              >
                <td className="py-3 pl-4 pr-4 font-medium text-gray-900">{ex?.vendor || inv.vendorId}</td>
                <td className="py-3 pr-4 text-gray-700">{ex?.invoiceNumber || "-"}</td>
                <td className="py-3 pr-4 text-gray-900">{ex?.totalAmount != null ? ex.totalAmount.toFixed(2) : "-"}</td>
                <td className="py-3 pr-4">
                  <span className="inline-block text-xs px-2 py-1 rounded-md bg-blue-50 text-blue-700">
                    {cls?.category || "-"}
                  </span>
                </td>
                <td className="py-3 pr-4 text-gray-700">{uploaded ? format(uploaded, "PPpp") : "-"}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
