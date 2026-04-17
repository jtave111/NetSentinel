import { Monitor, AlertTriangle, Bell, ShieldCheck } from "lucide-react";
import type { Device } from "@/types";
import { deriveStatus } from "@/types";
export function StatStrip({devices}:{devices:Device[]}){
  const critical=devices.filter(d=>deriveStatus(d)==="critical").length;
  const warning=devices.filter(d=>deriveStatus(d)==="warning").length;
  const safe=devices.filter(d=>deriveStatus(d)==="safe").length;
  const pct=devices.length>0?Math.round(safe/devices.length*100):0;
  const STATS=[
    {label:"Total de Endpoints",value:String(devices.length),sub:`${safe} seguros`,Icon:Monitor,accent:"bg-brand",num:""},
    {label:"Crítico — Ação Imediata",value:String(critical),sub:"CVSS ≥ 9.0 · patch disponível",Icon:AlertTriangle,accent:"bg-danger",num:"text-danger"},
    {label:"Alertas Ativos",value:String(warning),sub:"Aguardando patch de segurança",Icon:Bell,accent:"bg-warn",num:"text-warn"},
    {label:"Endpoints Seguros",value:String(safe),sub:`${pct}% do parque tecnológico`,Icon:ShieldCheck,accent:"bg-ok",num:"text-ok"},
  ];
  return(
    <div className="grid" style={{gridTemplateColumns:"repeat(4,1fr)",border:"1px solid var(--b0)",background:"var(--b0)",gap:"1px"}}>
      {STATS.map(s=>(
        <div key={s.label} className="relative px-[18px] py-4" style={{background:"var(--s0)"}}>
          <div className={`absolute top-0 left-0 right-0 h-[2px] ${s.accent}`}/>
          <div className="flex items-start justify-between mb-2">
            <span className="text-[11px]" style={{color:"var(--t2)"}}>{s.label}</span>
            <s.Icon size={15} style={{color:"var(--t3)"}}/>
          </div>
          <div className={`text-[26px] font-semibold tracking-tight leading-none mb-1 ${s.num}`} style={!s.num?{color:"var(--t0)"}:undefined}>{s.value}</div>
          <div className="font-mono text-[11px]" style={{color:"var(--t2)"}}>{s.sub}</div>
        </div>
      ))}
    </div>
  );
}