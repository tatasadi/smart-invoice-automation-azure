"use client";

import { useEffect, useState } from "react";
import InvoiceList from "@/components/InvoiceList";
import { getInvoices, type Invoice } from "@/lib/api";

export default function DashboardPage() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const data = await getInvoices();
        setInvoices(data);
      } catch (e) {
        console.error(e);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  // Calculate statistics
  const totalInvoices = invoices.length;
  const totalAmount = invoices.reduce((sum, inv) => {
    return sum + (inv.extractedData?.totalAmount || 0);
  }, 0);

  const categoryBreakdown = invoices.reduce((acc, inv) => {
    const category = inv.classification?.category || "Unknown";
    acc[category] = (acc[category] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const avgProcessingTime = invoices.length > 0
    ? invoices.reduce((sum, inv) => sum + (inv.processingMetadata?.processingTime || 0), 0) / invoices.length
    : 0;

  return (
    <main className="mx-auto max-w-6xl px-6 py-10">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600">Overview of processed invoices and statistics.</p>
      </div>

      {/* Statistics Cards */}
      {!loading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="text-sm text-gray-500 mb-1">Total Invoices</div>
            <div className="text-2xl font-bold text-gray-900">{totalInvoices}</div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="text-sm text-gray-500 mb-1">Total Amount</div>
            <div className="text-2xl font-bold text-gray-900">
              ${totalAmount.toFixed(2)}
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="text-sm text-gray-500 mb-1">Avg Processing Time</div>
            <div className="text-2xl font-bold text-gray-900">
              {avgProcessingTime.toFixed(2)}s
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <div className="text-sm text-gray-500 mb-1">Categories</div>
            <div className="text-2xl font-bold text-gray-900">
              {Object.keys(categoryBreakdown).length}
            </div>
          </div>
        </div>
      )}

      {/* Category Breakdown */}
      {!loading && totalInvoices > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm mb-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Category Breakdown</h2>
          <div className="flex flex-wrap gap-3">
            {Object.entries(categoryBreakdown).map(([category, count]) => (
              <div
                key={category}
                className="inline-flex items-center gap-2 px-3 py-2 rounded-lg bg-blue-50 text-blue-700 border border-blue-100"
              >
                <span className="font-medium">{category}</span>
                <span className="px-2 py-0.5 rounded-full bg-blue-100 text-xs font-semibold">
                  {count}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Invoice List */}
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Recent Invoices</h2>
      </div>
      <InvoiceList invoices={invoices} loading={loading} />
    </main>
  );
}

