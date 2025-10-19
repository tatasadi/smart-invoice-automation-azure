"use client";

import { useEffect, useState } from "react";
import { getInvoices, type Invoice } from "@/lib/api";
import { format } from "date-fns";

export default function InvoiceList() {
  const [data, setData] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
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
  }, []);

  if (loading) return <div className="text-sm text-gray-600">Loading invoices...</div>;
  if (error) return <div className="text-sm text-red-600">{error}</div>;
  if (!data.length) return <div className="text-sm text-gray-600">No invoices yet.</div>;

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
            return (
              <tr key={key} className="border-t">
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
