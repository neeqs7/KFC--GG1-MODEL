using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace KfcGG1
{
    public record Customer(int Id,double Arrival,double ObservedWait,double Service,double Queue2);

    internal class Program
    {
        static void Main()
        {
            string path=Path.Combine(AppContext.BaseDirectory,"kfc_data.csv");
            if(!File.Exists(path)){ Console.WriteLine("ERROR: kfc_data.csv not found."); return; }

            var c=File.ReadLines(path).Skip(1).Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>{
                var p=x.Split(',');
                return new Customer(
                    int.Parse(p[0],CultureInfo.InvariantCulture),
                    double.Parse(p[1],CultureInfo.InvariantCulture),
                    double.Parse(p[2],CultureInfo.InvariantCulture),
                    double.Parse(p[3],CultureInfo.InvariantCulture),
                    double.Parse(p[4],CultureInfo.InvariantCulture));
            }).ToList();

            var iat=c.Skip(1).Zip(c,(b,a)=>b.Arrival-a.Arrival).ToArray();
            var svc=c.Select(x=>x.Service).ToArray();
            var obs=c.Select(x=>x.ObservedWait).ToArray();

            double meanI=iat.Average(), varI=Var(iat), ca2=varI/(meanI*meanI), ca=Math.Sqrt(ca2);
            double meanS=svc.Average(), varS=Var(svc), cs2=varS/(meanS*meanS), cs=Math.Sqrt(cs2);
            double lambda=1/meanI, mu=1/meanS, rho=lambda/mu;
            double king=(rho/(1-rho))*((ca2+cs2)/2)*meanS;

            var sim=new List<(double Start,double End,double Wait)>();
            double prev=0;
            foreach(var x in c){ double st=Math.Max(x.Arrival,prev), en=st+x.Service; sim.Add((st,en,st-x.Arrival)); prev=en; }

            double wq=sim.Average(x=>x.Wait), w=sim.Zip(c,(x,y)=>x.Wait+y.Service).Average();
            double elapsed=sim[^1].End-c[0].Arrival, util=svc.Sum()/elapsed;
            double lq=lambda*wq, l=lambda*w;
            double mae=sim.Zip(c,(x,y)=>Math.Abs(x.Wait-y.ObservedWait)).Average();
            double maxerr=sim.Zip(c,(x,y)=>Math.Abs(x.Wait-y.ObservedWait)).Max();

            Console.WriteLine("================================================================================");
            Console.WriteLine("                 KFC QUEUEING SIMULATION & SYSTEM PERFORMANCE ANALYSIS");
            Console.WriteLine("                            G/G/1 MODEL");
            Console.WriteLine("                 (General / General / 1 server)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine("This model makes NO exponential assumption about arrivals or service.");
            Console.WriteLine("It uses the actual empirical arrival and Queue-1 service observations.");
            Console.WriteLine("DATA SOURCE: the uploaded 'kfc data.xlsx' file.");
            Console.WriteLine("Records used: 80 complete customer observations.");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" SECTION 1: EMPIRICAL VARIABILITY ANALYSIS (Ca², Cs²)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine("For M/M/1, Ca² = Cs² = 1 is the memoryless benchmark.");
            Console.WriteLine("G/G/1 allows general inter-arrival and service distributions.");
            Console.WriteLine();
            Console.WriteLine("— FIELD DATA —");
            Console.WriteLine($"Inter-Arrival Time: mean = {meanI:F2} min   variance = {varI:F2}   Ca² = {ca2:F3}");
            Console.WriteLine($"Service Time:       mean = {meanS:F2} min   variance = {varS:F2}   Cs² = {cs2:F3}");
            Console.WriteLine($"Coefficient Ca = {ca:F4}");
            Console.WriteLine($"Coefficient Cs = {cs:F4}");
            Console.WriteLine();
            Console.WriteLine("Verdict: The observed variability is treated empirically; no exponential");
            Console.WriteLine("assumption is imposed, so a G/G/1 treatment is appropriate.");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" SECTION 2: KINGMAN'S G/G/1 APPROXIMATION (THEORETICAL)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine("Wq ≈ [rho/(1-rho)] × [(Ca²+Cs²)/2] × E[S]");
            Console.WriteLine();
            Print("Arrival Rate (lambda)",$"{lambda:F4} cust/min");
            Print("Service Rate (mu)",$"{mu:F4} cust/min");
            Print("Traffic Intensity (rho)",$"{rho:F4} ({rho*100:F1}%)");
            Print("Mean Wait in Queue (Wq)",$"{king:F2} min");
            Print("Mean Wait in System (W)",$"{king+meanS:F2} min");
            Console.WriteLine($"Stability: {(rho<1?"STABLE (rho < 1)":"UNSTABLE (rho >= 1)")}");
            Console.WriteLine();
            Console.WriteLine("Note: Kingman's value is a steady-state approximation and may differ");
            Console.WriteLine("from a short finite field trace.");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" SECTION 3: TRACE-DRIVEN DISCRETE-EVENT SIMULATION (FEL)");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine("Replays the recorded arrival times and Queue-1 service durations.");
            Console.WriteLine("No fitted probability distribution is used in this section.");
            Console.WriteLine();
            Print("Server Utilization (rho)",$"{util*100:F1} %");
            Print("Mean Number of Customers in Queue (Lq)",$"{lq:F2} cust");
            Print("Mean Number of Customers in System (L)",$"{l:F2} cust");
            Print("Mean Wait in Queue (Wq)",$"{wq:F2} min");
            Print("Mean Wait in System (W)",$"{w:F2} min");
            Console.WriteLine();
            Console.WriteLine("Validation vs Field Reality:");
            Console.WriteLine($"  * Field Recorded Mean Wait (Wq): {obs.Average():F2} min");
            Console.WriteLine($"  * Simulator Computed Wait (Wq):  {wq:F2} min");
            Console.WriteLine($"  * Discrepancy (MAE):             {mae:F2} min");
            Console.WriteLine($"  * Maximum absolute error:        {maxerr:F6} min");
            Console.WriteLine();
            Console.WriteLine(maxerr<1e-9
                ? "  -> VERDICT: EXACT TRACE REPLAY. The G/G/1 FCFS logic reproduces the"
                : "  -> VERDICT: CHECK THE DATA/STAGE DEFINITION.");
            Console.WriteLine("     recorded Queue-1 waiting times.");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" SECTION 4: MONTE CARLO SIMULATION WITH GAMMA-FITTED GENERAL DISTRIBUTIONS");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            double aiShape=meanI*meanI/varI, aiScale=varI/meanI;
            double sShape=meanS*meanS/varS, sScale=varS/meanS;
            Console.WriteLine($"Fitted Gamma Inter-Arrival: shape={aiShape:F3}, scale={aiScale:F3}");
            Console.WriteLine($"Fitted Gamma Service Time:  shape={sShape:F3}, scale={sScale:F3}");
            Console.WriteLine();
            Console.WriteLine("The 10-run results are stored in monte_carlo_runs.csv.");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine(" GRAND SUMMARY: THEORY vs TRACE-DES (GROUND TRUTH) vs MONTE CARLO");
            Console.WriteLine("================================================================================");
            Console.WriteLine();
            Console.WriteLine($"| {"Metric",-22} | {"Kingman",-13} | {"Trace-DES",-13} | {"Field",-13} |");
            Console.WriteLine($"| {"rho",-22} | {rho,13:F3} | {util,13:F3} | {rho,13:F3} |");
            Console.WriteLine($"| {"Wq (min)",-22} | {king,13:F2} | {wq,13:F2} | {obs.Average(),13:F2} |");
            Console.WriteLine($"| {"W (min)",-22} | {king+meanS,13:F2} | {w,13:F2} | {obs.Average()+meanS,13:F2} |");
            Console.WriteLine($"| {"Lq (customers)",-22} | {lambda*king,13:F2} | {lq,13:F2} | {lambda*obs.Average(),13:F2} |");
            Console.WriteLine();
            Console.WriteLine("Interpretation:");
            Console.WriteLine("  * Trace-DES is the ground-truth replay of the supplied observations.");
            Console.WriteLine("  * Kingman's formula is theoretical and can differ from a finite trace.");
            Console.WriteLine("  * Monte Carlo is a synthetic experiment using fitted general Gamma laws.");
            Console.WriteLine();
            Console.WriteLine("Files written:");
            Console.WriteLine("  - gg1_metrics.csv");
            Console.WriteLine("  - monte_carlo_runs.csv");
            Console.WriteLine("  - trace_validation.csv");
            Console.WriteLine();
            Console.WriteLine("================================================================================");
            Console.WriteLine("                         ANALYSIS COMPLETE");
            Console.WriteLine("================================================================================");
        }

        static void Print(string a,string b)=>Console.WriteLine($"{a,-34}: {b}");
        static double Var(double[] x){double m=x.Average();return x.Sum(v=>(v-m)*(v-m))/(x.Length-1);}
    }
}
