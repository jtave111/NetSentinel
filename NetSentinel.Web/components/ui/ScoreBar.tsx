import { cn } from "@/lib/utils";
import { cvssColor } from "@/lib/utils";
const F={red:"bg-danger",amber:"bg-warn",green:"bg-ok"};
const N={red:"text-danger",amber:"text-warn",green:"text-ok"};
export function ScoreBar({score}:{score:number}){
  const c=cvssColor(score);
  return(
    <div className="flex items-center gap-2">
      <div className="flex-1 h-[2px] min-w-[40px]" style={{background:"var(--b0)"}}>
        <div className={cn("h-full",F[c])} style={{width:`${score*10}%`}}/>
      </div>
      <span className={cn("font-mono text-[11px] font-medium min-w-[22px]",N[c])}>{score.toFixed(1)}</span>
    </div>
  );
}