"use client";
import type { FilterStatus, Device } from "@/types";
import { deriveStatus, maxCvss, topCves } from "@/types";
import { useMachineFilter } from "@/hooks/useMachineFilter";
import { StatusTag } from "@/components/ui/Tag";
import { ScoreBar } from "@/components/ui/ScoreBar";
import { DeviceModal } from "@/components/ui/Modal";
import { Panel, PanelHeader } from "@/components/ui/Panel";
import { cn, relativeTime } from "@/lib/utils";
const STRIPE:Record<string,string>={critical:"bg-danger",warning:"bg-warn",safe:"bg-ok",offline:"bg-[var(--t3)]"};
const TABS:{label:string;value:FilterStatus;dot?:string}[]=[
  {label:"Todos",value:"all"},{label:"Crítico",value:"critical",dot:"bg-danger"},
  {label:"Alerta",value:"warning",dot:"bg-warn"},{label:"Seguro",value:"safe",dot:"bg-ok"},
  {label:"Offline",value:"offline",dot:"bg-[var(--t3)]"},
];
export function MachineTable({devices}:{devices:Device[]}){
  const {filter,setFilter,query,setQuery,rows,selected,setSelected}=useMachineFilter(devices);
  return(
    <>
      <Panel>
        <PanelHeader title="Inventário de Endpoints"
          right={<input value={query} onChange={e=>setQuery(e.target.value)} placeholder="Buscar…" className="h-6 px-2 font-mono text-[11px] w-[140px] bg-transparent outline-none" style={{border:"1px solid var(--b0)",color:"var(--t0)"}}/>}
        />
        <div className="flex flex-shrink-0" style={{borderBottom:"1px solid var(--b0)"}}>
          {TABS.map(t=>{const on=filter===t.value;const count=t.value==="all"?devices.length:devices.filter(d=>deriveStatus(d)===t.value).length;return(
            <button key={t.value} onClick={()=>setFilter(t.value)} className="flex items-center gap-1.5 px-3.5 h-9 font-mono text-[11px] transition-colors border-r last:border-r-0"
              style={{borderColor:"var(--b0)",background:on?"var(--s1)":"transparent",color:on?"var(--t0)":"var(--t2)",fontWeight:on?500:400}}>
              {t.dot&&<span className={cn("w-[5px] h-[5px] rounded-full flex-shrink-0",t.dot)}/>}{t.label} ({count})
            </button>
          );})}
        </div>
        <div className="overflow-x-auto flex-1">
          <table className="w-full border-collapse">
            <thead><tr style={{background:"var(--s1)",borderBottom:"1px solid var(--b0)"}}>
              {["Endpoint","Sistema","Status","CVEs","CVSS Máx","Último Sync",""].map(h=>(
                <th key={h} className="font-mono text-[10px] font-medium uppercase tracking-[0.5px] px-4 py-[7px] text-left whitespace-nowrap" style={{color:"var(--t2)"}}>{h}</th>
              ))}
            </tr></thead>
            <tbody>
              {rows.length===0?<tr><td colSpan={7} className="text-center py-8 font-mono text-[12px]" style={{color:"var(--t3)"}}>Nenhum resultado.</td></tr>:rows.map(d=><Row key={d.id} device={d} onSelect={setSelected}/>)}
            </tbody>
          </table>
        </div>
      </Panel>
      <DeviceModal device={selected} onClose={()=>setSelected(null)}/>
    </>
  );
}
function Row({device:d,onSelect}:{device:Device;onSelect:(d:Device)=>void}){
  const status=deriveStatus(d); const score=maxCvss(d); const cves=topCves(d);
  return(
    <tr onClick={()=>onSelect(d)} className="cursor-pointer transition-colors" style={{borderBottom:"1px solid var(--b1)"}}
      onMouseEnter={e=>(e.currentTarget.style.background="var(--hov)")} onMouseLeave={e=>(e.currentTarget.style.background="")}>
      <td className="px-4 py-[10px]"><div className="flex items-center gap-2.5">
        <span className={cn("w-[3px] h-7 rounded-[1px] flex-shrink-0",STRIPE[status])}/>
        <div><div className="font-mono text-[12px] font-medium" style={{color:"var(--t0)"}}>{d.hostname}</div><div className="font-mono text-[10.5px]" style={{color:"var(--t2)"}}>{d.ipv4Address}</div></div>
      </div></td>
      <td className="px-4 py-[10px]"><div className="text-[12px]" style={{color:"var(--t0)"}}>{d.operatingSystem}</div><div className="font-mono text-[10px]" style={{color:"var(--t2)"}}>{d.user?.name??"—"}</div></td>
      <td className="px-4 py-[10px]"><StatusTag status={status}/></td>
      <td className="px-4 py-[10px]">
        {cves.length>0?<div className="flex items-center gap-1"><span className="font-mono text-[10.5px] font-medium px-[5px] py-[2px] border" style={{background:"#fdf3f4",color:"#c50f1f",borderColor:"rgba(197,15,31,.15)"}}>{cves[0].cveId}</span>{cves.length>1&&<span className="font-mono text-[10.5px]" style={{color:"var(--t2)"}}>+{cves.length-1}</span>}</div>:<span className="font-mono text-[10.5px]" style={{color:"var(--t3)"}}>—</span>}
      </td>
      <td className="px-4 py-[10px] min-w-[100px]">{score>0?<ScoreBar score={score}/>:<span className="font-mono text-[10.5px]" style={{color:"var(--t3)"}}>—</span>}</td>
      <td className="px-4 py-[10px]"><span className="font-mono text-[10.5px]" style={{color:"var(--t3)"}}>{relativeTime(d.lastSync)} atrás</span></td>
      <td className="px-4 py-[10px]"><button onClick={e=>{e.stopPropagation();onSelect(d);}} className="font-mono text-[11px]" style={{color:"#0f6cbd",borderBottom:"1px solid transparent"}} onMouseEnter={e=>((e.currentTarget as HTMLElement).style.borderBottomColor="#0f6cbd")} onMouseLeave={e=>((e.currentTarget as HTMLElement).style.borderBottomColor="transparent")}>detalhes</button></td>
    </tr>
  );
}