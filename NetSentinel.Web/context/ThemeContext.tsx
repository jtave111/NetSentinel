"use client";
import { createContext, useContext, useEffect, useState } from "react";
type Theme = "light"|"dark";
interface Ctx { theme:Theme; toggle:()=>void; }
const C = createContext<Ctx>({ theme:"light", toggle:()=>{} });
export const useTheme = () => useContext(C);
export function ThemeProvider({ children }: { children:React.ReactNode }) {
  const [theme, set] = useState<Theme>("light");
  useEffect(() => { const s = localStorage.getItem("ns_theme") as Theme|null; if(s) apply(s); }, []);
  function apply(t: Theme) { set(t); document.documentElement.classList.toggle("dark",t==="dark"); localStorage.setItem("ns_theme",t); }
  return <C.Provider value={{ theme, toggle:()=>apply(theme==="dark"?"light":"dark") }}>{children}</C.Provider>;
}