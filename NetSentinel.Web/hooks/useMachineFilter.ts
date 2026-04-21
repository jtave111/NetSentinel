"use client";
import { useState, useMemo } from "react";
import type { Device, FilterStatus } from "@/types";
import { deriveStatus } from "@/types";
export function useMachineFilter(devices: Device[]) {
  const [filter,setFilter]     = useState<FilterStatus>("all");
  const [query,setQuery]       = useState("");
  const [selected,setSelected] = useState<Device|null>(null);
  const rows = useMemo(()=>{
    const q = query.toLowerCase();
    return devices.filter(d=>{
      const ok1 = filter==="all"||deriveStatus(d)===filter;
      const ok2 = !q||d.hostname.toLowerCase().includes(q)||(d.ipv4Address?.toLowerCase()??"").includes(q)||(d.operatingSystem?.toLowerCase()??"").includes(q)||((d.user?.name??"").toLowerCase().includes(q));
      return ok1&&ok2;
    });
  },[devices,filter,query]);
  return { filter,setFilter,query,setQuery,rows,selected,setSelected };
}