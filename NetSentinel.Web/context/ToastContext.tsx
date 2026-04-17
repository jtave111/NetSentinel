"use client";
import { createContext, useContext, useState, useCallback, useRef } from "react";
import { AlertTriangle, CheckCircle, Info, X } from "lucide-react";
import { cn } from "@/lib/utils";
export type ToastType = "success"|"error"|"info";
interface TI { id:number; message:string; type:ToastType; }
interface Ctx { showToast:(m:string,t?:ToastType)=>void; }
const C = createContext<Ctx>({ showToast:()=>{} });
export const useToast = () => useContext(C);
const ICON  = { success:CheckCircle, error:AlertTriangle, info:Info } as const;
const COLOR = { success:"text-ok", error:"text-danger", info:"text-brand" } as const;
export function ToastProvider({ children }: { children:React.ReactNode }) {
  const [ts, setTs] = useState<TI[]>([]);
  const ref = useRef(0);
  const showToast = useCallback((message:string, type:ToastType="info") => {
    const id = ++ref.current;
    setTs(p=>[...p,{id,message,type}]);
    setTimeout(()=>setTs(p=>p.filter(t=>t.id!==id)),4000);
  },[]);
  return (
    <C.Provider value={{ showToast }}>
      {children}
      <div className="fixed bottom-4 right-4 z-[9999] flex flex-col gap-1.5 pointer-events-none">
        {ts.map(t=>{const Icon=ICON[t.type];return(
          <div key={t.id} className="flex items-center gap-2 px-3.5 py-2.5 text-[12.5px] max-w-xs pointer-events-auto"
            style={{background:"var(--s0)",border:"1px solid var(--b0)",boxShadow:"0 4px 20px rgba(0,0,0,.18)"}}>
            <Icon size={14} className={cn("flex-shrink-0",COLOR[t.type])}/>
            <span className="flex-1" style={{color:"var(--t0)"}}>{t.message}</span>
            <button onClick={()=>setTs(p=>p.filter(x=>x.id!==t.id))} style={{color:"var(--t3)"}}><X size={12}/></button>
          </div>);})}
      </div>
    </C.Provider>
  );
}