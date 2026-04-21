"use client";
import { useEffect } from "react";
import { useRouter }  from "next/navigation";
import { useAuth }    from "@/context/AuthContext";
import { useDevices } from "@/hooks/useDevices";
import { Sidebar }    from "@/components/layout/Sidebar";
import { Topbar }     from "@/components/layout/Topbar";
import { StatStrip }         from "@/components/dashboard/StatStrip";
import { MachineTable }      from "@/components/dashboard/MachineTable";
import { AlertFeed }         from "@/components/dashboard/AlertFeed";
import { VulnerabilityFeed } from "@/components/dashboard/VulnerabilityFeed";
import { PageSpinner }  from "@/components/ui/Spinner";
import { ErrorBanner }  from "@/components/ui/ErrorBanner";
import { relativeTime } from "@/lib/utils";
//TODO: Colocar o usuario no Device e usar isso pra mostrar o nome do usuário na tabela e permitir filtrar por usuário, além de mostrar no resumo do parque quais usuários tem mais máquinas e vulnerabilidades associadas
export default function DashboardPage() {
  const {user,isLoading:al}=useAuth(); const router=useRouter();
  const {devices,loading,error,refetch,lastFetch}=useDevices();
  useEffect(()=>{ if(!al&&!user) router.replace("/login"); },[user,al,router]);
  if(al) return <PageSpinner/>; if(!user) return null;
  return(
    <div className="flex min-h-screen">
      <Sidebar/>
      <div className="flex flex-col flex-1 ml-[200px]">
        <Topbar onRefresh={refetch} refreshing={loading}/>
        <main className="flex-1 p-5">
          <div className="mb-4 pb-4" style={{borderBottom:"1px solid var(--b1)"}}>
            <h1 className="text-[15px] font-semibold tracking-tight" style={{color:"var(--t0)"}}>Security Operations Center</h1>
            <p className="font-mono text-[11.5px] mt-0.5" style={{color:"var(--t3)"}}>
              {lastFetch?`Atualizado ${relativeTime(lastFetch.toISOString())} atrás`:"Carregando…"} · {devices.length} endpoints · NVD Integration ativo
            </p>
          </div>
          {error&&<ErrorBanner message={error} onRetry={refetch}/>}
          {loading&&devices.length===0?<PageSpinner/>:(
            <>
              <StatStrip devices={devices}/>
              <div className="grid mt-4" style={{gridTemplateColumns:"1fr 280px",border:"1px solid var(--b0)",background:"var(--b0)",gap:"1px"}}>
                <MachineTable devices={devices}/><AlertFeed devices={devices}/>
              </div>
              <div className="grid" style={{gridTemplateColumns:"1fr 1fr",border:"1px solid var(--b0)",borderTop:"none",background:"var(--b0)",gap:"1px"}}>
                <VulnerabilityFeed devices={devices}/>
                <div style={{background:"var(--s0)",borderLeft:"1px solid var(--b0)",padding:"16px"}}>
                  <div className="font-mono text-[10px] uppercase tracking-[0.5px] mb-3" style={{color:"var(--t3)"}}>Resumo do Parque</div>
                  {devices.slice(0,8).map(d=>{
                    const apps=d.installedApplications.length;
                    const vulns=d.installedApplications.flatMap(a=>a.vulnerabilities??[]).length;
                    return(
                      <div key={d.id} className="flex items-center justify-between py-[6px]" style={{borderBottom:"1px solid var(--b1)"}}>
                        <div><div className="font-mono text-[11.5px] font-medium" style={{color:"var(--t0)"}}>{d.hostname}</div><div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{d.ipv4Address}</div></div>
                        <div className="text-right">
                          <div className="font-mono text-[10.5px]" style={{color:"var(--t2)"}}>{apps} apps</div>
                          {vulns>0&&<div className="font-mono text-[10px] text-danger">{vulns} vulns</div>}
                          <div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{relativeTime(d.lastSync)} atrás</div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </>
          )}
        </main>
      </div>
    </div>
  );
}