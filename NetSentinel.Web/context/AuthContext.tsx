"use client";
import { createContext, useContext, useEffect, useState } from "react";
import { apiLogin } from "@/lib/api";
interface AuthUser { id:number; username:string; email:string; role:string; }
interface Ctx { user:AuthUser|null; token:string|null; login:(u:string,p:string)=>Promise<void>; logout:()=>void; isLoading:boolean; }
const C = createContext<Ctx>({ user:null,token:null,login:async()=>{},logout:()=>{},isLoading:true });
export const useAuth = () => useContext(C);
function parseJwt(t: string): AuthUser|null {
  try {
    const p = JSON.parse(atob(t.split(".")[1]));
    return {
      id:       Number(p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]??0),
      username: p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]??"",
      email:    p["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]??"",
      role:     p["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]??"Employee",
    };
  } catch { return null; }
}
export function AuthProvider({ children }: { children:React.ReactNode }) {
  const [user,setUser]           = useState<AuthUser|null>(null);
  const [token,setToken]         = useState<string|null>(null);
  const [isLoading,setIsLoading] = useState(true);
  useEffect(()=>{ const s=localStorage.getItem("ns_token"); if(s){setToken(s);setUser(parseJwt(s));} setIsLoading(false); },[]);
  async function login(username:string,password:string) {
    const r = await apiLogin(username,password);
    localStorage.setItem("ns_token",r.token); setToken(r.token); setUser(parseJwt(r.token));
  }
  function logout() { localStorage.removeItem("ns_token"); setToken(null); setUser(null); }
  return <C.Provider value={{user,token,login,logout,isLoading}}>{children}</C.Provider>;
}