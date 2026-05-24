import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  
  output: 'standalone',
  
 
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