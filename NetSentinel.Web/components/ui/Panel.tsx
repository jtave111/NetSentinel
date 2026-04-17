"use client";
export function Panel({ children, className="", style }:{ children:React.ReactNode;className?:string;style?:React.CSSProperties }) {
  return <div className={`flex flex-col ${className}`} style={{background:"var(--s0)",...style}}>{children}</div>;
}
export function PanelHeader({ title, right }:{ title:string;right?:React.ReactNode }) {
  return (
    <div className="flex items-center gap-3 px-4 h-10 flex-shrink-0" style={{borderBottom:"1px solid var(--b0)"}}>
      <h2 className="text-[12.5px] font-semibold flex-1" style={{color:"var(--t0)"}}>{title}</h2>
      {right&&<div className="flex items-center gap-2">{right}</div>}
    </div>
  );
}