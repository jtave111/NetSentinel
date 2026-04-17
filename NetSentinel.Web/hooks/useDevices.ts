"use client";
import { useState, useEffect, useCallback } from "react";
import { apiGetDevices } from "@/lib/api";
import type { Device } from "@/types";
export function useDevices() {
  const [devices,setDevices]     = useState<Device[]>([]);
  const [loading,setLoading]     = useState(true);
  const [error,setError]         = useState<string|null>(null);
  const [lastFetch,setLastFetch] = useState<Date|null>(null);
  const fetch = useCallback(async()=>{
    try { setLoading(true); setError(null); const d=await apiGetDevices(); setDevices(d); setLastFetch(new Date()); }
    catch(e:unknown){ setError(e instanceof Error?e.message:"Erro ao carregar dispositivos"); }
    finally{ setLoading(false); }
  },[]);
  useEffect(()=>{ fetch(); },[fetch]);
  useEffect(()=>{ const id=setInterval(fetch,30_000); return()=>clearInterval(id); },[fetch]);
  return { devices, loading, error, refetch:fetch, lastFetch };
}