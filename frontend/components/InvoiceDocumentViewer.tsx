"use client";

import { useEffect, useState } from "react";
import { Loader2, FileText, ExternalLink } from "lucide-react";
import { getInvoiceBlobSasUrl } from "@/lib/api";
import { Button } from "@/components/ui/button";

type Props = {
  blobUrl: string;
  fileName: string;
};

export default function InvoiceDocumentViewer({ blobUrl, fileName }: Props) {
  const [sasUrl, setSasUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!blobUrl) {
      setLoading(false);
      return;
    }

    (async () => {
      try {
        setLoading(true);
        console.log("Fetching SAS URL for blob:", blobUrl);
        const url = await getInvoiceBlobSasUrl(blobUrl);
        console.log("Got SAS URL:", url);
        setSasUrl(url);
      } catch (e: any) {
        console.error("Error fetching SAS URL:", e);
        console.error("Error response:", e?.response);
        const errorMsg = e?.response?.data?.error || e?.message || "Failed to load document";
        setError(`${errorMsg} (Check console for details)`);
      } finally {
        setLoading(false);
      }
    })();
  }, [blobUrl]);

  if (loading) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-8">
        <div className="flex items-center justify-center gap-3 text-gray-600">
          <Loader2 className="animate-spin" size={20} />
          <span>Loading document...</span>
        </div>
      </div>
    );
  }

  if (error || !sasUrl) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-8">
        <div className="text-center">
          <FileText className="mx-auto mb-3 text-gray-400" size={48} />
          <p className="text-red-600 mb-2">{error || "Unable to load document"}</p>
          <p className="text-sm text-gray-500">The document preview is not available</p>
        </div>
      </div>
    );
  }

  // Determine file type from filename or blob URL
  const isPdf = fileName.toLowerCase().endsWith('.pdf') || blobUrl.toLowerCase().includes('.pdf');
  const isImage = /\.(jpg|jpeg|png|gif|bmp|tiff|webp)$/i.test(fileName);

  return (
    <div className="rounded-xl border border-gray-200 bg-white overflow-hidden">
      <div className="p-4 border-b bg-gray-50 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <FileText size={18} className="text-gray-600" />
          <span className="text-sm font-medium text-gray-900">{fileName}</span>
        </div>
        <a href={sasUrl} target="_blank" rel="noopener noreferrer">
          <Button variant="outline" size="sm">
            <ExternalLink size={14} />
            Open in New Tab
          </Button>
        </a>
      </div>

      <div className="bg-gray-100">
        {isPdf ? (
          <iframe
            src={sasUrl}
            className="w-full h-[600px] border-0"
            title={fileName}
          />
        ) : isImage ? (
          <div className="p-4 flex justify-center">
            <img
              src={sasUrl}
              alt={fileName}
              className="max-w-full h-auto max-h-[600px] rounded shadow-lg"
            />
          </div>
        ) : (
          <div className="p-8 text-center">
            <FileText className="mx-auto mb-3 text-gray-400" size={48} />
            <p className="text-gray-600 mb-4">Preview not available for this file type</p>
            <a href={sasUrl} target="_blank" rel="noopener noreferrer">
              <Button>
                <ExternalLink size={16} />
                Download File
              </Button>
            </a>
          </div>
        )}
      </div>
    </div>
  );
}
