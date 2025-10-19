import InvoiceList from "@/components/InvoiceList";

export default function DashboardPage() {
  return (
    <main className="mx-auto max-w-6xl px-6 py-10">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Processed Invoices</h1>
        <p className="text-gray-600">Recent invoices with extracted data and classification.</p>
      </div>
      <InvoiceList />
    </main>
  );
}

