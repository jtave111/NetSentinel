"use client";
import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth }  from "@/context/AuthContext";
import { useToast } from "@/context/ToastContext";
import { Eye, EyeOff, ShieldCheck } from "lucide-react";
export default function LoginPage() {
  const {login,user,isLoading}=useAuth(); const {showToast}=useToast(); const router=useRouter();
  const [u,setU]=useState(""); const [p,setP]=useState(""); const [show,setShow]=useState(false);
  const [loading,setLoading]=useState(false); const [err,setErr]=useState("");
  useEffect(()=>{ if(!isLoading&&user) router.replace("/"); },[user,isLoading,router]);
  async function onSubmit(e:React.FormEvent){ e.preventDefault(); setErr(""); setLoading(true);
    try { await login(u,p); showToast("Login realizado com sucesso.","success"); router.replace("/"); }
    catch { setErr("Usuário ou senha inválidos."); } finally { setLoading(false); }
  }
  const fs="w-full h-9 px-3 font-mono text-[12.5px] outline-none transition-colors";
  const fst={background:"var(--s1)",border:"1px solid var(--b0)",color:"var(--t0)"};
  return(
    <div className="min-h-screen flex items-center justify-center" style={{background:"var(--bg)"}}>
      <div className="w-[360px]" style={{border:"1px solid var(--b0)",background:"var(--s0)"}}>
        <div className="flex items-center gap-3 px-6 py-5" style={{borderBottom:"1px solid var(--b0)"}}>
          <div className="w-9 h-9 flex items-center justify-center bg-brand"><ShieldCheck size={18} className="text-white"/></div>
          <div><div className="text-[14px] font-semibold" style={{color:"var(--t0)"}}><span className="text-brand">Net</span>Sentinel</div><div className="font-mono text-[10px]" style={{color:"var(--t3)"}}>Security Operations Center</div></div>
        </div>
        <form onSubmit={onSubmit} className="p-6 flex flex-col gap-4">
          <div>
            <label className="block font-mono text-[10.5px] uppercase tracking-[0.5px] mb-1.5" style={{color:"var(--t2)"}}>Usuário</label>
            <input value={u} onChange={e=>setU(e.target.value)} className={fs} style={fst} placeholder="username" required autoFocus/>
          </div>
          <div>
            <label className="block font-mono text-[10.5px] uppercase tracking-[0.5px] mb-1.5" style={{color:"var(--t2)"}}>Senha</label>
            <div className="relative">
              <input value={p} onChange={e=>setP(e.target.value)} type={show?"text":"password"} className={fs} style={fst} placeholder="••••••••" required/>
              <button type="button" onClick={()=>setShow(x=>!x)} className="absolute right-2.5 top-1/2 -translate-y-1/2" style={{color:"var(--t3)"}}>{show?<EyeOff size={13}/>:<Eye size={13}/>}</button>
            </div>
          </div>
          {err&&<div className="font-mono text-[11.5px] px-3 py-2 text-danger" style={{background:"#fdf3f4",border:"1px solid rgba(197,15,31,.2)"}}>{err}</div>}
          <button type="submit" disabled={loading} className="h-9 font-medium text-[13px] text-white bg-brand hover:bg-brand-hover disabled:opacity-60 mt-1">{loading?"Autenticando…":"Entrar"}</button>
        </form>
        <div className="px-6 pb-5 font-mono text-[10px] text-center" style={{color:"var(--t3)"}}>NetSentinel v2.0 · C# API · NVD/NIST Integration</div>
      </div>
    </div>
  );
}