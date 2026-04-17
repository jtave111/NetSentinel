"use client";
import { usePathname, useRouter } from "next/navigation";
import { LayoutDashboard, Monitor, ShieldAlert, Users, LogOut } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { cn } from "@/lib/utils";
const NAV=[{label:"Dashboard",icon:LayoutDashboard,href:"/"},{label:"Endpoints",icon:Monitor,href:"/devices"},{label:"Vulnerabilidades",icon:ShieldAlert,href:"/vulnerabilities"},{label:"Usuários",icon:Users,href:"/users"}];
export function Sidebar(){
  const pathname=usePathname(); const router=useRouter(); const {user,logout}=useAuth();
  return(
    <aside className="fixed inset-y-0 left-0 w-[200px] flex flex-col z-50" style={{background:"var(--s0)",borderRight:"1px solid var(--b0)"}}>
      <div className="h-12 flex items-center gap-2.5 px-4 flex-shrink-0" style={{borderBottom:"1px solid var(--b0)"}}>
        <svg width="22" height="26" viewBox="0 0 22 26" fill="none">
          <path d="M11 1L1 5v8C1 18.1 5.2 22.6 11 24.2c5.8-1.6 10-6.1 10-11.2V5L11 1z" fill="#0f6cbd" fillOpacity=".12" stroke="#0f6cbd" strokeWidth="1.2" strokeLinejoin="round"/>
          <path d="M7 18V10l7 8V10" stroke="#0f6cbd" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"/>
        </svg>
        <span className="text-[14px] font-semibold tracking-tight" style={{color:"var(--t0)"}}><span className="text-brand">Net</span>Sentinel</span>
      </div>
      <nav className="flex-1 py-2 overflow-y-auto">
        <span className="block text-[10px] font-medium uppercase tracking-[0.8px] px-4 py-2" style={{color:"var(--t3)"}}>Monitoramento</span>
        {NAV.map(item=>{const Icon=item.icon;const on=pathname===item.href;return(
          <button key={item.href} onClick={()=>router.push(item.href)} className={cn("w-full flex items-center gap-2 px-4 py-[7px] text-[12.5px] transition-colors relative text-left",on?"font-medium":"font-normal")}
            style={{background:on?"var(--hov)":"transparent",color:on?"#0f6cbd":"var(--t1)"}}>
            {on&&<span className="absolute left-0 top-0 bottom-0 w-0.5 bg-brand"/>}
            <Icon size={14} className="flex-shrink-0 opacity-75"/>{item.label}
          </button>
        );})}
      </nav>
      <div className="flex-shrink-0" style={{borderTop:"1px solid var(--b0)"}}>
        {user&&<div className="flex items-center gap-2.5 px-3.5 py-2.5">
          <div className="w-7 h-7 flex items-center justify-center flex-shrink-0 bg-brand text-white text-[11px] font-semibold">{user.username.slice(0,2).toUpperCase()}</div>
          <div className="flex-1 min-w-0"><div className="text-[12px] font-medium truncate" style={{color:"var(--t0)"}}>{user.username}</div><div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>{user.role}</div></div>
        </div>}
        <button onClick={()=>{logout();router.push("/login");}} className="w-full flex items-center gap-2 px-4 py-2.5 text-[12px] transition-colors"
          style={{borderTop:"1px solid var(--b1)",color:"var(--t2)"}}
          onMouseEnter={e=>(e.currentTarget.style.color="#c50f1f")} onMouseLeave={e=>(e.currentTarget.style.color="var(--t2)")}>
          <LogOut size={13}/>Sair
        </button>
      </div>
    </aside>
  );
}