import type { Config } from "tailwindcss";
const config: Config = {
  darkMode: "class",
  content: ["./app/**/*.{ts,tsx}","./components/**/*.{ts,tsx}","./context/**/*.{ts,tsx}","./hooks/**/*.{ts,tsx}"],
  theme: { extend: {
    fontFamily: { sans:["IBM Plex Sans","system-ui","sans-serif"], mono:["IBM Plex Mono","monospace"] },
    colors: {
      brand:  { DEFAULT:"#0f6cbd", hover:"#115ea3", light:"#ebf3fb" },
      danger: { DEFAULT:"#c50f1f", light:"#fdf3f4" },
      warn:   { DEFAULT:"#bc4b09", light:"#fdf6f3" },
      ok:     { DEFAULT:"#0e7a0d", light:"#f1faf1" },
    },
  }},
  plugins: [],
};
export default config;