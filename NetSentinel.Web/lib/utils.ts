import { clsx, type ClassValue } from "clsx";
import type { DeviceStatus } from "@/types";
export function cn(...i: ClassValue[]) { return clsx(i); }
export function cvssColor(s: number): "red"|"amber"|"green" {
  if (s >= 9) return "red"; if (s >= 5) return "amber"; return "green";
}
export const STATUS_LABEL: Record<DeviceStatus,string> = {
  critical:"Crítico", warning:"Alerta", safe:"Seguro", offline:"Offline"
};
export function relativeTime(iso: string): string {
  const s = (Date.now() - new Date(iso).getTime()) / 1000;
  if (s < 60) return `${Math.round(s)}s`;
  if (s < 3600) return `${Math.round(s/60)} min`;
  if (s < 86400) return `${Math.round(s/3600)}h`;
  return `${Math.round(s/86400)}d`;
}