export function Spinner({size=16}:{size?:number}){
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" className="animate-spin" style={{color:"var(--t3)"}}>
    <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2" strokeDasharray="60" strokeDashoffset="20" strokeLinecap="round"/>
  </svg>;
}
export function PageSpinner(){
  return <div className="flex-1 flex flex-col items-center justify-center gap-3 py-20">
    <Spinner size={24}/>
    <p className="font-mono text-[12px]" style={{color:"var(--t3)"}}>Carregando dados da API…</p>
  </div>;
}