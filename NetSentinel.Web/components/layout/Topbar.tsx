"use client";
import { usePathname } from "next/navigation";
import { Sun, Moon, RefreshCw } from "lucide-react";
import { useTheme } from "@/context/ThemeContext";
const TITLES:Record<string,string>={"/":"Dashboard","/devices":"Endpoints","/vulnerabilities":"Vulnerabilidades","/users":"Usuários"};
export function Topbar({onRefresh,refreshing}:{onRefresh?:()=>void;refreshing?:boolean}){
  const {theme,toggle}=useTheme(); const pathname=usePathname();
  return(
    <header className="h-12 flex items-center gap-2.5 px-5 flex-shrink-0 sticky top-0 z-40" style={{background:"var(--s0)",borderBottom:"1px solid var(--b0)"}}>
      <div className="flex-1 text-[13px]" style={{color:"var(--t1)"}}>
        <span className="font-semibold" style={{color:"var(--t0)"}}>{TITLES[pathname]??"NetSentinel"}</span>
        <span style={{color:"var(--t3)"}}> / Security Operations</span>
      </div>
      {onRefresh&&<button onClick={onRefresh} disabled={refreshing} className="flex items-center gap-1.5 h-7 px-3 text-[12px] font-medium disabled:opacity-50 transition-colors" style={{border:"1px solid var(--b0)",background:"var(--s0)",color:"var(--t1)"}}>
        <RefreshCw size={12} className={refreshing?"animate-spin":""}/>{refreshing?"Atualizando…":"Atualizar"}
      </button>}
      <button onClick={toggle} className="w-7 h-7 flex items-center justify-center" style={{border:"1px solid var(--b0)",background:"var(--s0)",color:"var(--t2)"}}>
        {theme==="dark"?<Sun size={14}/>:<Moon size={14}/>}
      </button>
      <div className="flex items-center gap-1.5 h-[22px] px-2 font-mono text-[10.5px] text-ok" style={{background:"#f1faf1",border:"1px solid rgba(14,122,13,.2)"}}>
        <span className="w-[5px] h-[5px] rounded-full bg-ok animate-pulse"/>ao vivo
      </div>
    </header>
  );
}