"use client";

import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { Upload, Loader2, FileText } from "lucide-react";
import { uploadInvoice, type Invoice } from "@/lib/api";
import { Button } from "@/components/ui/button";

type Props = {
  onUploaded?: (invoice: Invoice) => void;
};

const allowedExt = [".pdf", ".png", ".jpg", ".jpeg", ".tiff", ".bmp"];

export default function FileUpload({ onUploaded }: Props) {
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fileName, setFileName] = useState<string | null>(null);

  const onDrop = useCallback(async (acceptedFiles: File[]) => {
    setError(null);
    if (!acceptedFiles?.length) return;

    const file = acceptedFiles[0];
    const ext = `.${file.name.split(".").pop()?.toLowerCase()}`;
    if (!allowedExt.includes(ext)) {
      setError(`Invalid file type. Allowed: ${allowedExt.join(", ")}`);
      return;
    }

    try {
      setIsUploading(true);
      setFileName(file.name);
      const result = await uploadInvoice(file);
      onUploaded?.(result);
    } catch (e: any) {
      const msg = e?.response?.data?.error || e?.message || "Upload failed";
      setError(msg);
    } finally {
      setIsUploading(false);
    }
  }, [onUploaded]);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    multiple: false,
    accept: {
      "application/pdf": [".pdf"],
      "image/*": [".png", ".jpg", ".jpeg", ".tiff", ".bmp"],
    },
    maxFiles: 1,
  });

  return (
    <div className="w-full">
      <div
        {...getRootProps()}
        className={`border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors ${
          isDragActive ? "border-[#0078D4] bg-blue-50/40" : "border-gray-300 hover:border-[#0078D4]"
        }`}
      >
        <input {...getInputProps()} />
        <div className="flex flex-col items-center gap-3">
          {isUploading ? (
            <Loader2 className="animate-spin text-[#0078D4]" size={36} />
          ) : (
            <Upload className="text-[#0078D4]" size={36} />
          )}
          <p className="text-sm text-gray-600">
            {isDragActive ? "Drop the file here" : "Drag & drop invoice (PDF or image), or click to select"}
          </p>
          <p className="text-xs text-gray-500">PDF, PNG, JPG, JPEG, TIFF, BMP</p>
          <div className="mt-2">
            <Button type="button" disabled={isUploading}>
              {isUploading ? "Uploading..." : "Choose File"}
            </Button>
          </div>
          {fileName && !isUploading && (
            <div className="mt-2 inline-flex items-center gap-2 text-sm text-gray-700">
              <FileText size={16} /> {fileName}
            </div>
          )}
        </div>
      </div>
      {error && (
        <div className="mt-3 text-sm text-red-600" role="alert">
          {error}
        </div>
      )}
    </div>
  );
}

