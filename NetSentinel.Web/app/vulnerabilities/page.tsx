"use client";
import { useEffect, useState } from "react";
import { useRouter }  from "next/navigation";
import { useAuth }    from "@/context/AuthContext";
import { useDevices } from "@/hooks/useDevices";
import { Sidebar }    from "@/components/layout/Sidebar";
import { Topbar }     from "@/components/layout/Topbar";
import { SeverityTag } from "@/components/ui/Tag";
import { ScoreBar }    from "@/components/ui/ScoreBar";
import { PageSpinner } from "@/components/ui/Spinner";
import { ErrorBanner } from "@/components/ui/ErrorBanner";
import { Search } from "lucide-react";
export default function VulnerabilitiesPage() {
  const {user,isLoading:al}=useAuth(); const router=useRouter();
  const {devices,loading,error,refetch}=useDevices();
  const [query,setQuery]=useState(""); const [sev,setSev]=useState("ALL");
  useEffect(()=>{ if(!al&&!user) router.replace("/login"); },[user,al,router]);
  if(al) return <PageSpinner/>; if(!user) return null;
  const allV=devices.flatMap(d=>d.installedApplications.flatMap(a=>(a.vulnerabilities??[]).map(v=>({...v,hostname:d.hostname,ip:d.ipv4Address,appName:a.name,appVersion:a.version})))).sort((a,b)=>b.cvssScore-a.cvssScore);
  const SEVS=["ALL","CRITICAL","HIGH","MEDIUM","LOW"];
  const counts:Record<string,number>={};
  SEVS.forEach(s=>{counts[s]=s==="ALL"?allV.length:allV.filter(v=>v.severity===s).length;});
  const filtered=allV.filter(v=>{ const okS=sev==="ALL"||v.severity===sev; const q=query.toLowerCase(); const okQ=!q||v.cveId.toLowerCase().includes(q)||v.hostname.toLowerCase().includes(q)||v.appName.toLowerCase().includes(q); return okS&&okQ; });
  return(
    <div className="flex min-h-screen"><Sidebar/>
      <div className="flex flex-col flex-1 ml-[200px]"><Topbar onRefresh={refetch} refreshing={loading}/>
        <main className="flex-1 p-5">
          <div className="mb-4 pb-4" style={{borderBottom:"1px solid var(--b1)"}}>
            <h1 className="text-[15px] font-semibold tracking-tight" style={{color:"var(--t0)"}}>Vulnerabilidades</h1>
            <p className="font-mono text-[11.5px] mt-0.5" style={{color:"var(--t3)"}}>{allV.length} vulnerabilidades em {devices.length} endpoints · Fonte: NVD/NIST</p>
          </div>
          {error&&<ErrorBanner message={error} onRetry={refetch}/>}
          {loading&&devices.length===0?<PageSpinner/>:(
            <>
              <div className="flex items-center gap-2 mb-4 flex-wrap">
                <div className="flex items-center gap-1.5 h-8 px-2.5 w-[220px]" style={{background:"var(--s0)",border:"1px solid var(--b0)"}}>
                  <Search size={12} style={{color:"var(--t3)"}}/>
                  <input value={query} onChange={e=>setQuery(e.target.value)} placeholder="CVE-ID, hostname, aplicação…" className="bg-transparent border-none outline-none font-mono text-[11.5px] w-full" style={{color:"var(--t0)"}}/>
                </div>
                {SEVS.map(s=><button key={s} onClick={()=>setSev(s)} className="flex items-center gap-1.5 h-8 px-3 font-mono text-[11px] transition-colors" style={{border:"1px solid var(--b0)",background:sev===s?"var(--t0)":"var(--s0)",color:sev===s?"var(--s0)":"var(--t2)"}}>{s} ({counts[s]})</button>)}
              </div>
              <div style={{border:"1px solid var(--b0)",background:"var(--s0)"}}>
                <table className="w-full border-collapse">
                  <thead><tr style={{background:"var(--s1)",borderBottom:"1px solid var(--b0)"}}>
                    {["CVE-ID","Severidade","CVSS","Descrição","Aplicação","Endpoint"].map(h=><th key={h} className="font-mono text-[10px] font-medium uppercase tracking-[0.5px] px-4 py-[7px] text-left whitespace-nowrap" style={{color:"var(--t2)"}}>{h}</th>)}
                  </tr></thead>
                  <tbody>
                    {filtered.length===0?<tr><td colSpan={6} className="text-center py-8 font-mono text-[12px]" style={{color:"var(--t3)"}}>Nenhum resultado.</td></tr>:filtered.map((v,i)=>(
                      <tr key={`${v.id}-${i}`} className="transition-colors" style={{borderBottom:"1px solid var(--b1)"}} onMouseEnter={e=>(e.currentTarget.style.background="var(--hov)")} onMouseLeave={e=>(e.currentTarget.style.background="")}>
                        <td className="px-4 py-2.5"><span className="font-mono text-[11.5px] font-semibold text-danger">{v.cveId}</span></td>
                        <td className="px-4 py-2.5"><SeverityTag severity={v.severity}/></td>
                        <td className="px-4 py-2.5 min-w-[110px]"><ScoreBar score={v.cvssScore}/></td>
                        <td className="px-4 py-2.5 max-w-[280px]"><span className="text-[12px] line-clamp-2" style={{color:"var(--t1)"}}>{v.description}</span></td>
                        <td className="px-4 py-2.5"><div className="font-mono text-[11px]" style={{color:"var(--t0)"}}>{v.appName}</div><div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{v.appVersion}</div></td>
                        <td className="px-4 py-2.5"><div className="font-mono text-[11px]" style={{color:"var(--t0)"}}>{v.hostname}</div><div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{v.ip}</div></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </main>
      </div>
    </div>
  );
}