import { cn } from "@/lib/utils";
import type { DeviceStatus } from "@/types";
const SL:Record<DeviceStatus,string>={critical:"Crítico",warning:"Alerta",safe:"Seguro",offline:"Offline"};
const SC:Record<DeviceStatus,string>={critical:"bg-danger-light text-danger border-danger/20",warning:"bg-warn-light text-warn border-warn/20",safe:"bg-ok-light text-ok border-ok/20",offline:"border-[var(--b1)] text-faint"};
export function StatusTag({status}:{status:DeviceStatus}){
  return <span className={cn("inline-flex items-center font-mono text-[10.5px] font-medium px-[7px] py-[2px] border",SC[status])} style={status==="offline"?{background:"var(--s2)"}:undefined}>{SL[status]}</span>;
}
const EVC:Record<string,string>={CRITICAL:"bg-danger-light text-danger border-danger/20",HIGH:"bg-warn-light text-warn border-warn/20",MEDIUM:"bg-brand-light text-brand border-brand/20",LOW:"border-[var(--b1)] text-faint"};
const EVL:Record<string,string>={CRITICAL:"Crítico",HIGH:"Alto",MEDIUM:"Médio",LOW:"Baixo"};
export function SeverityTag({severity}:{severity:string}){
  const s=severity.toUpperCase();
  return <span className={cn("inline-flex items-center font-mono text-[9.5px] font-medium uppercase tracking-[0.3px] px-[6px] py-[2px] border",EVC[s]??"text-faint border-[var(--b1)]")} style={!EVC[s]?{background:"var(--s2)"}:undefined}>{EVL[s]??s}</span>;
}
const LC:Record<string,string>={install:"bg-brand-light text-brand border-brand/20",alert:"bg-danger-light text-danger border-danger/20",update:"bg-ok-light text-ok border-ok/20",scan:"border-[var(--b0)] text-faint"};
export function LogTag({type}:{type:string}){
  return <span className={cn("flex-shrink-0 font-mono text-[9px] font-medium uppercase tracking-[0.3px] px-[6px] py-[2px] border",LC[type]??"border-[var(--b0)] text-faint")} style={type==="scan"?{background:"var(--s2)"}:undefined}>{type}</span>;
}