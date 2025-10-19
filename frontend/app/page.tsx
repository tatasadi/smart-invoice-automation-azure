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

      <section>
        <InvoiceResults invoice={invoice} />
      </section>
    </main>
  );
}
