"use client";

import { useState } from "react";
import FileUpload from "@/components/FileUpload";
import InvoiceResults from "@/components/InvoiceResults";
import type { Invoice } from "@/lib/api";

export default function Home() {
  const [invoice, setInvoice] = useState<Invoice | null>(null);

  return (
    <main className="mx-auto max-w-5xl px-6 py-10">
      <section className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Smart Invoice Automation</h1>
        <p className="mt-2 text-gray-600">
          Upload a PDF or image to extract invoice details and classify the category using Azure AI.
        </p>
      </section>

      <section className="mb-10">
        <FileUpload onUploaded={setInvoice} />
      </section>

      {invoice ? (
        <section>
          <InvoiceResults invoice={invoice} />
        </section>
      ) : (
        <section className="text-center py-12 px-6 rounded-xl bg-white border border-gray-200">
          <div className="max-w-md mx-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Get Started
            </h3>
            <p className="text-sm text-gray-600 mb-4">
              Upload an invoice above to see AI-powered extraction and classification in action.
            </p>
            <div className="text-xs text-gray-500 space-y-1">
              <p>✓ Extract vendor, amounts, dates, and line items</p>
              <p>✓ Automatic category classification</p>
              <p>✓ Processing in under 5 seconds</p>
            </div>
          </div>
        </section>
      )}
    </main>
  );
}
