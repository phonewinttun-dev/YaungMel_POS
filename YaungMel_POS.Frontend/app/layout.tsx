import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Providers } from "./providers";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "Yaung Mel — Point of Sale System",
    template: "%s | Yaung Mel",
  },
  description:
    "Yaung Mel is a modern, premium POS solution for managing products, sales, inventory, and loyalty programs. Experience seamless business management with our advanced analytics and loyalty system.",
  keywords: ["POS", "Point of Sale", "Yaung Mel", "Inventory Management", "Sales Tracking", "Loyalty Program", "Business Analytics"],
  authors: [{ name: "Yaung Mel Team", url: "https://yaungmel.com" }],
  creator: "Yaung Mel",
  publisher: "Yaung Mel Solutions",
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  openGraph: {
    type: "website",
    locale: "en_US",
    url: "https://pos.yaungmel.com",
    siteName: "Yaung Mel POS",
    title: "Yaung Mel — Modern Point of Sale",
    description: "Streamline your business with Yaung Mel's advanced POS and loyalty system.",
    images: [
      {
        url: "/og-image.png",
        width: 1200,
        height: 630,
        alt: "Yaung Mel POS Dashboard",
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title: "Yaung Mel — Modern POS",
    description: "Streamline your business with Yaung Mel's advanced POS.",
    creator: "@yaungmel",
    images: ["/og-image.png"],
  },
  viewport: "width=device-width, initial-scale=1, maximum-scale=1",
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      className={`${inter.variable} h-full`}
      data-theme="dark"
      suppressHydrationWarning
    >
      <body className="min-h-full flex flex-col font-[var(--font-inter)] antialiased">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
