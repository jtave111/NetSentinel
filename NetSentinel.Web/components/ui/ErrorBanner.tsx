"use client";
import { AlertTriangle, RefreshCw } from "lucide-react";
export function ErrorBanner({message,onRetry}:{message:string;onRetry?:()=>void}){
  return(
    <div className="flex items-center gap-3 px-4 py-3 mb-4" style={{background:"#fdf3f4",border:"1px solid rgba(197,15,31,.2)"}}>
      <AlertTriangle size={14} className="text-danger flex-shrink-0"/>
      <span className="text-[12.5px] flex-1 text-danger">{message}</span>
      {onRetry&&<button onClick={onRetry} className="flex items-center gap-1 font-mono text-[11px] text-danger"><RefreshCw size={11}/>Tentar novamente</button>}
    </div>
  );
}