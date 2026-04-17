"use client";
import { useEffect } from "react";
import { X } from "lucide-react";
import type { Device } from "@/types";
import { deriveStatus, maxCvss, topCves } from "@/types";
import { StatusTag, SeverityTag } from "./Tag";
import { ScoreBar } from "./ScoreBar";
import { relativeTime } from "@/lib/utils";

interface Props { device:Device|null; onClose:()=>void; }

export function DeviceModal({ device, onClose }:Props){
  useEffect(()=>{ const fn=(e:KeyboardEvent)=>{if(e.key==="Escape")onClose();}; window.addEventListener("keydown",fn); return()=>window.removeEventListener("keydown",fn); },[onClose]);
  if(!device) return null;
  const status=deriveStatus(device); const score=maxCvss(device); const cves=topCves(device);
  return(
    <div className="fixed inset-0 z-[500] flex items-center justify-center" style={{background:"rgba(0,0,0,.5)"}}
      onClick={e=>{if(e.target===e.currentTarget)onClose();}}>
      <div className="w-[560px] max-h-[84vh] overflow-y-auto flex flex-col" style={{background:"var(--s0)",border:"1px solid var(--b0)",boxShadow:"0 8px 40px rgba(0,0,0,.3)"}}>
        <div className="flex items-center gap-3 px-4 py-3 sticky top-0 z-10" style={{background:"var(--s0)",borderBottom:"1px solid var(--b0)"}}>
          <span className="font-mono text-[13px] font-semibold flex-1" style={{color:"var(--t0)"}}>{device.hostname}</span>
          <StatusTag status={status}/> {score>0&&<ScoreBar score={score}/>}
          <button onClick={onClose} className="w-6 h-6 flex items-center justify-center" style={{border:"1px solid var(--b0)",background:"var(--s1)",color:"var(--t2)"}}><X size={12}/></button>
        </div>
        <div className="p-4 flex flex-col gap-3">
          <Block title="Informações">
            {[["Hostname",device.hostname],["IPv4",device.ipv4Address],["IPv6",device.ipv6Address||"—"],["MAC",device.macAddress],["Sistema",device.operatingSystem],["Usuário",device.user?.name??"—"],["Departamento",device.user?.department??"—"],["Primeiro sync",relativeTime(device.firstSync)+" atrás"],["Último sync",relativeTime(device.lastSync)+" atrás"]].map(([k,v])=><KV key={k} k={k} v={v}/>)}
          </Block>
          <Block title={`Vulnerabilidades (${cves.length})`}>
            {cves.length===0?<p className="px-3.5 py-3 font-mono text-[11px] text-faint">Nenhuma vulnerabilidade.</p>:cves.map(v=>(
              <div key={v.id} className="flex items-start gap-3 px-3.5 py-2.5" style={{borderBottom:"1px solid var(--b1)"}}>
                <SeverityTag severity={v.severity}/>
                <div className="flex-1 min-w-0">
                  <div className="font-mono text-[11.5px] font-medium" style={{color:"var(--t0)"}}>{v.cveId}</div>
                  <div className="text-[11px] mt-0.5 leading-snug" style={{color:"var(--t2)"}}>{v.description}</div>
                </div>
                <span className={`font-mono text-[12px] font-semibold flex-shrink-0 ${v.cvssScore>=9?"text-danger":"text-warn"}`}>{v.cvssScore.toFixed(1)}</span>
              </div>
            ))}
          </Block>
          <Block title={`Apps Instalados (${device.installedApplications.length})`}>
            {device.installedApplications.length===0?<p className="px-3.5 py-3 font-mono text-[11px] text-faint">Sem dados.</p>:device.installedApplications.map(a=>{
              const vuln=(a.vulnerabilities??[]).length>0;
              return(
                <div key={a.id} className="flex items-center justify-between px-3.5 py-2" style={{borderBottom:"1px solid var(--b1)"}}>
                  <div>
                    <span className="text-[12px]" style={{color:"var(--t0)"}}>{a.name}</span>
                    {vuln&&<span className="ml-2 font-mono text-[9px] px-1 py-px text-danger bg-danger-light border border-danger/20">VULN</span>}
                  </div>
                  <div className="text-right">
                    <div className="font-mono text-[11px]" style={{color:"var(--t2)"}}>{a.version}</div>
                    <div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{a.publisher}</div>
                  </div>
                </div>
              );
            })}
          </Block>
        </div>
      </div>
    </div>
  );
}
function Block({title,children}:{title:string;children:React.ReactNode}){
  return <div style={{border:"1px solid var(--b0)"}}><div className="font-mono text-[10px] font-medium uppercase tracking-[0.5px] px-3.5 py-2" style={{background:"var(--s1)",borderBottom:"1px solid var(--b0)",color:"var(--t2)"}}>{title}</div>{children}</div>;
}
function KV({k,v}:{k:string;v:string}){
  return <div className="flex items-center justify-between px-3.5 py-[7px]" style={{borderBottom:"1px solid var(--b1)"}}><span className="font-mono text-[11px]" style={{color:"var(--t2)"}}>{k}</span><span className="font-mono text-[11px] font-medium" style={{color:"var(--t0)"}}>{v}</span></div>;
}