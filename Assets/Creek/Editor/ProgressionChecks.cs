using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek.Editor
{
    public static class ProgressionChecks
    {
        static readonly StringBuilder report=new StringBuilder();
        static void Check(bool ok,string name) { if(!ok)throw new Exception("FAILED: "+name);report.AppendLine("PASS: "+name); }
        static Progression Equipped(int stage)
        {
            var p=new Progression();p.Credit(10000);
            for(int i=0;i<3;i++)while(p.Level(i)<3)p.Buy(i);
            for(int i=0;i<stage;i++)p.BuyAutomation(i);
            return p;
        }
        public static void Run()
        {
            report.Clear();
            var p=new Progression();
            Check(!p.HasAutomation(0)&&!p.HasAutomation(1)&&!p.HasAutomation(2),"Fresh camp starts entirely manual");
            p.Credit(10000);
            Check(!p.BuyAutomation(0)&&!p.BuyAutomation(1)&&!p.BuyAutomation(2),"Gold alone cannot skip the progression order");
            for(int level=1;level<=3;level++)
            {
                for(int i=0;i<3;i++)Check(p.Buy(i),"Tool "+i+" buys level "+level);
                Check(p.AutomationAvailable(level-1),"The matching tool tier unlocks assistance "+level);
                int gold=p.Gold, price=p.AutomationCost(level-1);
                Check(p.BuyAutomation(level-1)&&p.Gold==gold-price&&p.AutomationOn(level-1),"Assistance purchase charges once and starts enabled");
                Check(!p.BuyAutomation(level-1),"Owned assistance cannot be purchased twice");
            }
            Check(!p.Buy(3)&&!p.Buy(-1)&&!p.ToggleAutomation(8),"Invalid purchases and toggles are rejected");
            p=Equipped(3);var a=new PanAutomation();var sim=new PanSimulation();sim.Scoop(0);
            var manual=a.Resolve(sim,p,new PanIntent {Work=1},.05f);
            Check(manual.Work==1&&manual.Wash==0,"Manual Work takes priority over all assistance");
            manual=a.Resolve(sim,p,new PanIntent {Wash=1},.05f);
            Check(manual.Wash==1&&manual.Work==0,"Manual Wash takes priority over all assistance");
            Check(!a.Resolve(sim,p,new PanIntent {Work=1,Wash=1},.05f,true).Working,"Blocked input suppresses manual and automated actions");
            for(int i=0;i<400;i++)CheckNotCollect(a,sim,p);
            Check(!sim.Ready&&sim.Collect()==0,"Auto Collect cannot pay for an unfinished load");
            foreach(int fps in new[]{30,60,120})
            {
                foreach(int tier in new[]{1,2,3})
                {
                    p=new Progression();p.Credit(10000);
                    for(int i=0;i<3;i++)for(int k=0;k<tier;k++)p.Buy(i);
                    p.BuyAutomation(0);if(tier>=2)p.BuyAutomation(1);if(tier==3)p.BuyAutomation(2);
                    if(tier==1) { // Work alone can prepare, but never finish or remove the wash step.
                        sim=new PanSimulation();sim.Scoop(0);a=new PanAutomation();
                        for(int i=0;i<30*fps;i++)sim.Step(a.Resolve(sim,p,default,1f/fps),1f/fps,p.PanLevel,p.RiffleLevel);
                        Check(sim.Sediment==1&&!sim.Ready&&sim.CompactedSediment<.05f,"Auto Work prepares and plateaus without washing at "+fps+" Hz");
                        for(int i=0;i<20*fps&&!sim.Ready;i++)sim.Step(a.Resolve(sim,p,new PanIntent {Wash=1},1f/fps),1f/fps,p.PanLevel,p.RiffleLevel);
                        Check(sim.Ready,"Manual Wash finishes an Auto Work load at "+fps+" Hz");
                    }
                    else
                    {
                        sim=new PanSimulation();sim.Scoop(0);a=new PanAutomation();
                        bool both=false;
                        for(int i=0;i<90*fps&&!sim.Ready;i++)
                        {
                            var intent=a.Resolve(sim,p,default,1f/fps);both|=intent.Work>0&&intent.Wash>0;
                            sim.Step(intent,1f/fps,p.PanLevel,p.RiffleLevel);
                        }
                        Check(sim.Ready&&!both&&a.Switches>=3,"Combined assistance alternates without simultaneous holds or deadlock at tier "+tier+" / "+fps+" Hz");
                        int safeGold=0;foreach(var g in sim.Grains)if(g.Kind==MaterialKind.Gold&&g.Active)safeGold++;
                        Check(safeGold==14,"Automated separation retains every gold grain");
                    }
                }
            }
            p=Equipped(3);p.ToggleAutomation(0);sim=new PanSimulation();sim.Scoop(0);a=new PanAutomation();
            for(int i=0;i<600;i++)sim.Step(a.Resolve(sim,p,default,.05f),.05f,3,3);
            Check(sim.NeedsWork&&!sim.Ready&&sim.Stones==10,"Auto Wash alone clears loose dirt and waits for manual Work");
            for(int i=0;i<400;i++)sim.Step(a.Resolve(sim,p,new PanIntent {Work=1},.05f),.05f,3,3);
            for(int i=0;i<800&&!sim.Ready;i++)sim.Step(a.Resolve(sim,p,default,.05f),.05f,3,3);
            Check(sim.Ready,"Auto Wash resumes and finishes material prepared by manual Work");
            Check(!a.ShouldCollect(sim,p,1),"Auto Collect shows the revealed gold first");
            bool pausedCollect=false;for(int i=0;i<100;i++)pausedCollect|=a.ShouldCollect(sim,p,.05f,true);
            Check(!pausedCollect,"Paused collect delay stays frozen");
            bool collected=false;for(int i=0;i<50;i++)collected|=a.ShouldCollect(sim,p,.05f);
            Check(collected,"Ready gold is collected after its reveal delay");
            int value=sim.Collect();Check(value>0&&sim.Collect()==0,"Automated collection uses the same pay-once command");
            p.ToggleAutomation(1);
            sim=new PanSimulation();sim.Scoop(0);
            Check(!a.Resolve(sim,p,default,.05f).Working,"Turning off Work and Wash immediately restores manual panning");
            SaveChecks();
            VariationChecks();
            report.AppendLine(ChapterBalance.Simulate(false));
            report.AppendLine(ChapterBalance.Simulate(true));
            Directory.CreateDirectory("Playtest");
            File.WriteAllText("Playtest/progression-checks.txt",report.ToString());
            #if UNITY_EDITOR
            Debug.Log(report.ToString());
            #else
            Console.WriteLine(report.ToString());
            #endif
        }
        static void CheckNotCollect(PanAutomation a,PanSimulation s,Progression p)
        { if(a.ShouldCollect(s,p,.05f))throw new Exception("Premature automatic collection"); }
        static void VariationChecks()
        {
            var ordinary=new PanSimulation();ordinary.Scoop(0,0);
            var rich=new PanSimulation();rich.Scoop(3,0);
            Check(rich.GoldValue==ordinary.GoldValue+9,"Scoop levels add predictable gold to the same load");
            var nugget=new PanSimulation();nugget.Scoop(0,19);
            Check(nugget.HasNugget&&nugget.Grains[140].Size>ordinary.Grains[140].Size,"Every twentieth scoop includes a visibly larger small nugget");
            var restart=new PanSimulation();restart.Scoop(0,19);
            Check(restart.GoldValue==nugget.GoldValue,"Restarting a scoop cannot reroll its payout");
            var screened=new PanSimulation();screened.Scoop(0,0,true);
            Check(screened.CompactedSediment<ordinary.CompactedSediment&&screened.GoldValue==ordinary.GoldValue,"Sluice prepares material without passive money or extra gold");
        }
        static void SaveChecks()
        {
            string folder=Path.GetFullPath("Playtest/Verification/"+Guid.NewGuid().ToString("N"));
            var store=new SaveStore(Path.Combine(folder,"save.txt"));
            var p=Equipped(3);p.ToggleAutomation(1);p.RecordCollection();
            Check(store.Save(p),"Progress writes to a local versioned save");
            var loaded=store.Load();
            Check(SaveStore.Encode(p)==SaveStore.Encode(loaded),"Save/load restores gold, lifetime earnings, all levels, ownership, toggles and collection milestone");
            int before=loaded.Gold;loaded=store.Load();
            Check(before==loaded.Gold,"Reloading earns no offline gold");
            Check(store.Save(p),"Second write atomically replaces the primary save and creates a backup");
            File.WriteAllText(store.Path,"broken");
            Check(store.Load().Gold==p.Gold&&store.Message!=null,"Corrupt primary recovers from a valid backup");
            File.WriteAllText(store.Path+".bak","also broken");
            store.Load();
            Check(store.WriteBlocked&&!store.Save(p)&&File.ReadAllText(store.Path)=="broken","Unreadable saves are preserved and never silently overwritten");
            Check(store.Reset()&&!File.Exists(store.Path)&&!File.Exists(store.Path+".bak"),"Reset removes primary, backup and temporary save");
            Check(store.Load().Gold==0&&!store.WriteBlocked,"Reset starts a fresh manual camp");
            bool rejected=false;
            try { SaveStore.Decode(SaveStore.Encode(p).Replace("Pan=3","Pan=99")); } catch(ArgumentException) {rejected=true;}
            Check(rejected,"Invalid upgrade levels cannot enter a loaded game");
        }
    }

    public static class ChapterBalance
    {
        // A state-based approximation of a player who alternates as taught; no artificial economy timers.
        public static PanIntent Manual(PanSimulation s, Progression p, float elapsed)
        {
            if(p.AutomationOn(0)&&p.AutomationOn(1))return default;
            if(p.AutomationOn(0))return s.CompactedSediment<=.045f ? new PanIntent {Wash=1} : default;
            return new PanIntent {Work=(elapsed%8)<3?1:0,Wash=(elapsed%8)>=3?1:0};
        }
        public static string Simulate(bool leisurely)
        {
            var p=new Progression();var s=new PanSimulation();var a=new PanAutomation();s.Scoop(0,0);
            var trace=new StringBuilder();float elapsed=0, scoopTime=0, readyWait=0, overhead=leisurely?70:30, lastPurchase=0, maxGap=0;
            int purchases=0;
            while(!p.FullyAssisted && elapsed<3600)
            {
                var manual=Manual(s,p,scoopTime);
                s.Step(a.Resolve(s,p,manual,.05f),.05f,p.PanLevel,p.RiffleLevel);
                elapsed+=.05f;scoopTime+=.05f;
                if(!s.Ready)continue;
                readyWait+=.05f;if(readyWait<(leisurely?4:1.5f))continue;
                p.Credit(s.Collect());p.RecordCollection();overhead+=leisurely?3:1;
                int tool=p.RecommendedUpgrade, assist=p.NextAutomation;string purchase=null;
                if(tool>=0 && p.Buy(tool))purchase=Progression.UpgradeNames[tool]+" "+p.Level(tool);
                else if(tool<0 && assist>=0 && p.BuyAutomation(assist))purchase=Progression.AssistNames[assist];
                if(purchase!=null)
                {
                    overhead+=leisurely?12:5;purchases++;
                    float now=elapsed+overhead;maxGap=Math.Max(maxGap,now-lastPurchase);lastPurchase=now;
                    trace.AppendLine("  "+(now/60).ToString("F1")+" min: "+purchase+" (pan "+p.CollectedPans+")");
                }
                s.Scoop(p.ScoopLevel,p.CollectedPans,p.SluiceActive);scoopTime=0;readyWait=0;
            }
            if(!p.FullyAssisted || purchases!=12)throw new Exception("Chapter path could not complete all tools and assistance.");
            trace.Insert(0,"BALANCE "+(leisurely?"unhurried":"focused")+": "+((elapsed+overhead)/60).ToString("F1")+" minutes; "+p.CollectedPans+" pans; longest purchase gap "+(maxGap/60).ToString("F1")+" min.\n");
            return trace.ToString();
        }
    }
}
