"use client";
import { useEffect, useState } from "react";
import { useRouter }  from "next/navigation";
import { useAuth }    from "@/context/AuthContext";
import { useToast }   from "@/context/ToastContext";
import { Sidebar }    from "@/components/layout/Sidebar";
import { Topbar }     from "@/components/layout/Topbar";
import { apiRegisterUser } from "@/lib/api";
import { useDevices }  from "@/hooks/useDevices";
import { PageSpinner } from "@/components/ui/Spinner";
import { Users, Plus, X } from "lucide-react";
export default function UsersPage() {
  const {user,isLoading:al}=useAuth(); const {showToast}=useToast(); const router=useRouter();
  const {devices,loading}=useDevices();
  const [showForm,setShowForm]=useState(false); const [saving,setSaving]=useState(false);
  const [form,setForm]=useState({name:"",username:"",email:"",password:"",department:"",roleId:2});
  useEffect(()=>{ if(!al&&!user) router.replace("/login"); },[user,al,router]);
  if(al) return <PageSpinner/>; if(!user) return null;
  const users=Array.from(new Map(devices.filter(d=>d.user).map(d=>[d.user!.id,d.user!])).values());
  async function handleSave(e:React.FormEvent){ e.preventDefault(); setSaving(true);
    try { await apiRegisterUser(form); showToast("Usuário criado com sucesso.","success"); setShowForm(false); setForm({name:"",username:"",email:"",password:"",department:"",roleId:2}); }
    catch(err){ showToast(err instanceof Error?err.message:"Erro ao criar usuário.","error"); } finally{ setSaving(false); }
  }
  const fs="w-full h-9 px-3 font-mono text-[12.5px] outline-none"; const fst={background:"var(--s1)",border:"1px solid var(--b0)",color:"var(--t0)"};
  return(
    <div className="flex min-h-screen"><Sidebar/>
      <div className="flex flex-col flex-1 ml-[200px]"><Topbar/>
        <main className="flex-1 p-5">
          <div className="mb-4 pb-4 flex items-center justify-between" style={{borderBottom:"1px solid var(--b1)"}}>
            <div><h1 className="text-[15px] font-semibold tracking-tight" style={{color:"var(--t0)"}}>Usuários</h1><p className="font-mono text-[11.5px] mt-0.5" style={{color:"var(--t3)"}}>{users.length} usuários associados a endpoints</p></div>
            <button onClick={()=>setShowForm(p=>!p)} className="flex items-center gap-1.5 h-8 px-3 text-[12px] font-medium text-white bg-brand hover:bg-brand-hover">{showForm?<X size={13}/>:<Plus size={13}/>}{showForm?"Cancelar":"Novo Usuário"}</button>
          </div>
          {showForm&&(
            <form onSubmit={handleSave} className="mb-5 p-4 grid grid-cols-2 gap-3" style={{border:"1px solid var(--b0)",background:"var(--s0)"}}>
              <div className="col-span-2 font-mono text-[10px] uppercase tracking-[0.5px] mb-1" style={{color:"var(--t3)"}}>Novo Usuário — POST /api/auth/users</div>
              {[["name","Nome completo","text"],["username","Username","text"],["email","Email","email"],["password","Senha","password"],["department","Departamento","text"]].map(([k,ph,type])=>(
                <div key={k}><label className="block font-mono text-[10px] uppercase tracking-[0.5px] mb-1" style={{color:"var(--t2)"}}>{ph}</label>
                  <input type={type} placeholder={ph} required className={fs} style={fst} value={(form as Record<string,unknown>)[k] as string} onChange={e=>setForm(p=>({...p,[k]:e.target.value}))}/></div>
              ))}
              <div><label className="block font-mono text-[10px] uppercase tracking-[0.5px] mb-1" style={{color:"var(--t2)"}}>Perfil</label>
                <select className={fs} style={fst} value={form.roleId} onChange={e=>setForm(p=>({...p,roleId:Number(e.target.value)}))}>
                  <option value={1}>Admin</option><option value={2}>Employee</option>
                </select></div>
              <div className="col-span-2 flex justify-end"><button type="submit" disabled={saving} className="h-9 px-6 text-[12.5px] font-medium text-white bg-brand disabled:opacity-60">{saving?"Salvando…":"Criar Usuário"}</button></div>
            </form>
          )}
          {loading&&users.length===0?<PageSpinner/>:(
            <div style={{border:"1px solid var(--b0)",background:"var(--s0)"}}>
              <div className="flex items-center px-4 h-10 flex-shrink-0" style={{borderBottom:"1px solid var(--b0)"}}><Users size={13} style={{color:"var(--t3)"}} className="mr-2"/><h2 className="text-[12.5px] font-semibold flex-1" style={{color:"var(--t0)"}}>Usuários Registrados</h2></div>
              <table className="w-full border-collapse">
                <thead><tr style={{background:"var(--s1)",borderBottom:"1px solid var(--b0)"}}>
                  {["Nome","Username","Email","Departamento","Perfil","Endpoints"].map(h=><th key={h} className="font-mono text-[10px] font-medium uppercase tracking-[0.5px] px-4 py-[7px] text-left" style={{color:"var(--t2)"}}>{h}</th>)}
                </tr></thead>
                <tbody>
                  {users.length===0?<tr><td colSpan={6} className="text-center py-8 font-mono text-[12px]" style={{color:"var(--t3)"}}>Nenhum usuário encontrado.</td></tr>:users.map(u=>{
                    const ec=devices.filter(d=>d.userId===u.id).length;
                    return(<tr key={u.id} className="transition-colors" style={{borderBottom:"1px solid var(--b1)"}} onMouseEnter={e=>(e.currentTarget.style.background="var(--hov)")} onMouseLeave={e=>(e.currentTarget.style.background="")}>
                      <td className="px-4 py-2.5"><div className="flex items-center gap-2"><div className="w-6 h-6 flex items-center justify-center bg-brand text-white font-mono text-[10px]">{u.name?.slice(0,2).toUpperCase()??"??"}</div><span className="text-[12.5px] font-medium" style={{color:"var(--t0)"}}>{u.name}</span></div></td>
                      <td className="px-4 py-2.5 font-mono text-[11.5px]" style={{color:"var(--t1)"}}>{u.username}</td>
                      <td className="px-4 py-2.5 font-mono text-[11.5px]" style={{color:"var(--t2)"}}>{u.email}</td>
                      <td className="px-4 py-2.5 text-[12px]" style={{color:"var(--t1)"}}>{u.department}</td>
                      <td className="px-4 py-2.5"><span className="font-mono text-[10.5px] px-[7px] py-[2px] border" style={{background:"#ebf3fb",color:"#0f6cbd",borderColor:"rgba(15,108,189,.2)"}}>{u.role?.name??"Employee"}</span></td>
                      <td className="px-4 py-2.5 font-mono text-[11.5px]" style={{color:"var(--t2)"}}>{ec} endpoint{ec!==1?"s":""}</td>
                    </tr>);
                  })}
                </tbody>
              </table>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}