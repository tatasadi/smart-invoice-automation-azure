"use client";

import { BadgeCheck, AlertTriangle, Tag } from "lucide-react";
import type { Invoice } from "@/lib/api";
import { format } from "date-fns";

type Props = {
  invoice: Invoice | null;
};

function confidenceColor(score?: number) {
  if (score == null) return "bg-gray-200 text-gray-800";
  if (score >= 0.9) return "bg-green-100 text-green-800";
  if (score >= 0.75) return "bg-yellow-100 text-yellow-800";
  return "bg-red-100 text-red-800";
}

export default function InvoiceResults({ invoice }: Props) {
  if (!invoice) return null;

  const ex = invoice.extractedData;
  const cls = invoice.classification;
  const pm = invoice.processingMetadata;

  const uploaded = invoice.uploadDate ? new Date(invoice.uploadDate) : null;

  return (
    <div className="w-full grid gap-6">
      <div className="rounded-xl border border-gray-200 p-6 bg-white shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">Extracted Details</h3>
          {cls?.category && (
            <span className="inline-flex items-center gap-1 text-sm px-2 py-1 rounded-md bg-blue-50 text-blue-700">
              <Tag size={14} /> {cls.category}
            </span>
          )}
        </div>
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4 text-sm">
          <div>
            <div className="text-gray-500">Vendor</div>
            <div className="font-medium text-gray-900">{ex?.vendor || "-"}</div>
          </div>
          <div>
            <div className="text-gray-500">Invoice Number</div>
            <div className="font-medium text-gray-900">{ex?.invoiceNumber || "-"}</div>
          </div>
          <div>
            <div className="text-gray-500">Invoice Date</div>
            <div className="font-medium text-gray-900">{ex?.invoiceDate || "-"}</div>
          </div>
          <div>
            <div className="text-gray-500">Total Amount</div>
            <div className="font-medium text-gray-900">
              {ex?.totalAmount != null ? `${ex.totalAmount.toFixed(2)} ${ex?.currency ?? ""}` : "-"}
            </div>
          </div>
          <div>
            <div className="text-gray-500">Uploaded</div>
            <div className="font-medium text-gray-900">{uploaded ? format(uploaded, "PPpp") : "-"}</div>
          </div>
          <div>
            <div className="text-gray-500">Processing Time</div>
            <div className="font-medium text-gray-900">{pm?.processingTime ? `${pm.processingTime.toFixed(2)}s` : "-"}</div>
          </div>
        </div>
        <div className="mt-4 flex items-center gap-3">
          <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-md text-xs ${confidenceColor(cls?.confidence)}`}>
            {cls && cls.confidence >= 0.75 ? <BadgeCheck size={14} /> : <AlertTriangle size={14} />}
            Classification Confidence: {cls?.confidence != null ? Math.round(cls.confidence * 100) : "-"}%
          </span>
          {pm?.formRecognizerConfidence != null && (
            <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-md text-xs ${confidenceColor(pm.formRecognizerConfidence)}`}>
              OCR Confidence: {Math.round(pm.formRecognizerConfidence * 100)}%
            </span>
          )}
        </div>
        {cls?.reasoning && (
          <div className="mt-4 text-sm text-gray-700">
            <div className="text-gray-500 mb-1">Reasoning</div>
            <p className="whitespace-pre-line">{cls.reasoning}</p>
          </div>
        )}
      </div>

      {ex?.lineItems && ex.lineItems.length > 0 && (
        <div className="rounded-xl border border-gray-200 p-6 bg-white shadow-sm">
          <h4 className="text-md font-semibold text-gray-900 mb-3">Line Items</h4>
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="text-left text-gray-600">
                  <th className="py-2 pr-4">Description</th>
                  <th className="py-2 pr-4">Qty</th>
                  <th className="py-2 pr-4">Unit Price</th>
                  <th className="py-2">Amount</th>
                </tr>
              </thead>
              <tbody>
                {ex.lineItems.map((li, i) => (
                  <tr key={i} className="border-t">
                    <td className="py-2 pr-4">{li.description}</td>
                    <td className="py-2 pr-4">{li.quantity ?? "-"}</td>
                    <td className="py-2 pr-4">{li.unitPrice != null ? li.unitPrice.toFixed(2) : "-"}</td>
                    <td className="py-2">{li.amount.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

