"use client";
import { useEffect } from "react";
import { useRouter }  from "next/navigation";
import { useAuth }    from "@/context/AuthContext";
import { useDevices } from "@/hooks/useDevices";
import { Sidebar }     from "@/components/layout/Sidebar";
import { Topbar }      from "@/components/layout/Topbar";
import { MachineTable } from "@/components/dashboard/MachineTable";
import { StatStrip }    from "@/components/dashboard/StatStrip";
import { PageSpinner }  from "@/components/ui/Spinner";
import { ErrorBanner }  from "@/components/ui/ErrorBanner";
export default function DevicesPage() {
  const {user,isLoading:al}=useAuth(); const router=useRouter();
  const {devices,loading,error,refetch}=useDevices();
  useEffect(()=>{ if(!al&&!user) router.replace("/login"); },[user,al,router]);
  if(al) return <PageSpinner/>; if(!user) return null;
  return(
    <div className="flex min-h-screen"><Sidebar/>
      <div className="flex flex-col flex-1 ml-[200px]"><Topbar onRefresh={refetch} refreshing={loading}/>
        <main className="flex-1 p-5">
          <div className="mb-4 pb-4" style={{borderBottom:"1px solid var(--b1)"}}>
            <h1 className="text-[15px] font-semibold tracking-tight" style={{color:"var(--t0)"}}>Endpoints</h1>
            <p className="font-mono text-[11.5px] mt-0.5" style={{color:"var(--t3)"}}>{devices.length} dispositivos registrados via agente</p>
          </div>
          {error&&<ErrorBanner message={error} onRetry={refetch}/>}
          {loading&&devices.length===0?<PageSpinner/>:<><div className="mb-4"><StatStrip devices={devices}/></div><div style={{border:"1px solid var(--b0)"}}><MachineTable devices={devices}/></div></>}
        </main>
      </div>
    </div>
  );
}