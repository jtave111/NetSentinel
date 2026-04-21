

export interface Role {
  id: number;
  name: string;
  description: string;
}

export interface User {
  id: number;
  name: string;
  username: string;
  email: string;
  department: string;
  roleId: number;
  role?: Role;
}

export interface SoftwareVulnerability {
  id: number;
  cveId: string;
  description: string;
  cvssScore: number;
  severity: "CRITICAL" | "HIGH" | "MEDIUM" | "LOW";
}

export interface InstalledApplication {
  id: number;
  name: string;
  version: string | null;      
  publisher: string | null;    
  vulnerabilities: SoftwareVulnerability[];  
}

export interface Device {
  id: number;
  userId: number | null;
  user?: User;
  hostname: string;
  ipv4Address: string | null;
  ipv6Address: string | null;
  macAddress: string | null;
  operatingSystem: string | null;
  firstSync: string;           
  lastSync: string;            
  installedApplications: InstalledApplication[];
}


export type DeviceStatus = "critical" | "warning" | "safe" | "offline";
export type FilterStatus = "all" | DeviceStatus;


/**
 * Deriva o status de um device com base no lastSync e no CVSS máximo.
 * - offline  → sem sync há mais de 60 minutos
 * - critical → CVSS >= 9.0
 * - warning  → CVSS >= 5.0
 * - safe     → sem vulnerabilidades relevantes
 */
export function deriveStatus(d: Device): DeviceStatus {
  if (!d.lastSync) return "offline";

  const diffMinutes = (Date.now() - new Date(d.lastSync).getTime()) / 60_000;
  if (diffMinutes > 60) return "offline";

  const max = maxCvss(d);
  if (max >= 9.0) return "critical";
  if (max >= 5.0) return "warning";
  return "safe";
}

export function maxCvss(d: Device): number {
  return d.installedApplications
    .flatMap(a => a.vulnerabilities)
    .reduce((max, v) => Math.max(max, v.cvssScore), 0);
}

export function topCves(d: Device): SoftwareVulnerability[] {
  return d.installedApplications
    .flatMap(a => a.vulnerabilities)
    .sort((a, b) => b.cvssScore - a.cvssScore)
    .slice(0, 5);
}

export function severityColor(severity: SoftwareVulnerability["severity"]): string {
  const map: Record<SoftwareVulnerability["severity"], string> = {
    CRITICAL : "#ef4444",   // red-500
    HIGH     : "#f97316",   // orange-500
    MEDIUM   : "#eab308",   // yellow-500
    LOW      : "#22c55e",   // green-500
  };
  return map[severity] ?? "#6b7280";
}

export function statusColor(status: DeviceStatus): string {
  const map: Record<DeviceStatus, string> = {
    critical : "#ef4444",
    warning  : "#f97316",
    safe     : "#22c55e",
    offline  : "#6b7280",
  };
  return map[status];
}

export function countBySeverity(d: Device): Record<SoftwareVulnerability["severity"], number> {
  const counts = { CRITICAL: 0, HIGH: 0, MEDIUM: 0, LOW: 0 };
  d.installedApplications
    .flatMap(a => a.vulnerabilities)
    .forEach(v => counts[v.severity]++);
  return counts;
}

export interface ApiLoginResponse {
  token: string;
  message: string;
}

export interface ApiMessageResponse {
  message: string;
}

export interface ApiErrorResponse {
  message?: string;
  error?: string;
}