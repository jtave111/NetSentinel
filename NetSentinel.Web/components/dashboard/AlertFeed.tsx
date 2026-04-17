"use client";
import type { Device } from "@/types";
import { deriveStatus, topCves } from "@/types";
import { relativeTime } from "@/lib/utils";
import { Panel, PanelHeader } from "@/components/ui/Panel";
export function AlertFeed({devices}:{devices:Device[]}){
  const alerts=devices.flatMap(d=>topCves(d).filter(v=>v.cvssScore>=7).map(v=>({
    sev:v.cvssScore>=9?"critical":"warning",
    title:`${v.cveId} detectado`,
    desc:`${d.hostname} · ${devices.flatMap(x=>x.installedApplications).find(a=>a.id===v.installedApplicationId)?.name??"app"}`,
    ts:relativeTime(d.lastSync)+" atrás",
  }))).sort((a,b)=>a.sev==="critical"&&b.sev!=="critical"?-1:1).slice(0,10);
  const SEV:Record<string,string>={critical:"bg-danger",warning:"bg-warn",info:"bg-brand"};
  return(
    <Panel style={{borderLeft:"1px solid var(--b0)"}}>
      <PanelHeader title="Alertas Recentes" right={<span className="font-mono text-[10px] font-medium px-[7px] py-[2px] border" style={{background:"#fdf3f4",color:"#c50f1f",borderColor:"rgba(197,15,31,.2)"}}>{alerts.length} ativos</span>}/>
      <div className="overflow-y-auto">
        {alerts.length===0?<p className="px-4 py-6 text-center font-mono text-[12px]" style={{color:"var(--t3)"}}>Nenhum alerta ativo.</p>:alerts.map((a,i)=>(
          <div key={i} className="flex gap-2.5 items-start px-3.5 py-2.5 transition-colors" style={{borderBottom:"1px solid var(--b1)"}} onMouseEnter={e=>(e.currentTarget.style.background="var(--hov)")} onMouseLeave={e=>(e.currentTarget.style.background="")}>
            <span className={`w-[3px] self-stretch rounded-[1px] flex-shrink-0 mt-px ${SEV[a.sev]??"bg-brand"}`}/>
            <div className="flex-1 min-w-0"><div className="text-[12px] font-medium mb-[2px] leading-snug" style={{color:"var(--t0)"}}>{a.title}</div><div className="font-mono text-[10.5px] truncate" style={{color:"var(--t2)"}}>{a.desc}</div></div>
            <span className="font-mono text-[10px] flex-shrink-0 mt-px" style={{color:"var(--t3)"}}>{a.ts}</span>
          </div>
        ))}
      </div>
    </Panel>
  );
}