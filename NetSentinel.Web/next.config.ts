import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // 1. Configuração para gerar arquivos estáticos (cria a pasta /out)
  output: 'export',
  
 
  images: {
    unoptimized: true,
  },

  allowedDevOrigins: [
    "192.168.5.*",     
    "192.168.111.*",    
    "localhost",
    "127.0.0.1",
  ],
};

export default nextConfig;