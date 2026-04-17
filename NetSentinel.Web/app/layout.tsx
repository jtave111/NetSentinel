import type { Metadata } from "next";
import "./globals.css";
import { ThemeProvider } from "@/context/ThemeContext";
import { ToastProvider } from "@/context/ToastContext";
import { AuthProvider }  from "@/context/AuthContext";
export const metadata: Metadata = {
  title: "NetSentinel — Security Operations Center",
  description: "Patch management e monitoramento de vulnerabilidades corporativas",
};
export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body>
        <ThemeProvider><AuthProvider><ToastProvider>{children}</ToastProvider></AuthProvider></ThemeProvider>
      </body>
    </html>
  );
}