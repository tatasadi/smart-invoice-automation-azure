import type { Metadata } from "next";
import Link from "next/link";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Smart Invoice Automation",
  description: "AI-powered invoice extraction and classification",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
		<html lang="en">
			<body
				className={`${geistSans.variable} ${geistMono.variable} antialiased bg-gray-50 min-h-screen`}
			>
				<header className="border-b bg-white">
					<div className="mx-auto max-w-6xl px-6 py-4 flex items-center justify-between">
						<Link href="/" className="text-[#0078D4] font-semibold">
							Smart Invoice Automation
						</Link>
						<nav className="flex items-center gap-4 text-sm">
							<Link href="/" className="text-gray-700 hover:text-[#0078D4]">
								Upload
							</Link>
							<Link href="/dashboard" className="text-gray-700 hover:text-[#0078D4]">
								Dashboard
							</Link>
							<a
								href="https://github.com/tatasadi/smart-invoice-automation-azure"
								target="_blank"
								rel="noreferrer"
								className="text-gray-700 hover:text-[#0078D4]"
							>
								GitHub
							</a>
						</nav>
					</div>
				</header>
				{children}
				<footer className="mt-10 border-t bg-white">
					<div className="mx-auto max-w-6xl px-6 py-6 text-sm text-gray-600">
						Built with Azure AI, Next.js, and shadcn/ui.
					</div>
				</footer>
			</body>
		</html>
	)
}
