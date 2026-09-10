using UnityEngine;

namespace RiffleCreek
{
    // Supplies the existing intents. Manual input has priority; no separate processing or payout.
    public sealed class PanAutomation
    {
        bool washing;
        float phaseTime, readyTime;
        int scoop;
        public string Activity { get; private set; } = "Manual panning";
        public int Switches { get; private set; }
        public void Reset() { washing=false; phaseTime=0; readyTime=0; scoop=0; Switches=0; Activity="Manual panning"; }
        public PanIntent Resolve(PanSimulation sim, Progression progress, PanIntent manual, float dt, bool blocked = false)
        {
            dt = Mathf.Clamp(dt,0,.05f);
            if (blocked) { Activity="Taking a break"; return default; }
            if (scoop != sim.ScoopNumber) { scoop=sim.ScoopNumber; washing=false; phaseTime=0; readyTime=0; }
            if (!sim.Loaded || sim.Ready) { Activity=sim.Ready ? "Gold ready" : "Waiting for a scoop"; return default; }
            bool work=progress.AutomationOn(0), wash=progress.AutomationOn(1);
            if (manual.Work > 0 || manual.Wash > 0)
            {
                // Releasing manual input resumes assistance from the material that is actually left.
                Activity="Your hands on the pan";
                washing=manual.Wash>0 && manual.Work==0; phaseTime=0;
                return manual;
            }
            if (!work && !wash) { Activity="Manual panning"; return default; }
            if (work && wash)
            {
                phaseTime+=dt;
                bool old=washing;
                if (phaseTime>=.8f)
                {
                    if (washing && sim.NeedsWork) washing=false;
                    else if (!washing && ((sim.LooseSediment>=.42f && sim.Stratification>=.55f) || sim.CompactedSediment<=.045f))
                        washing=true;
                }
                if (old != washing) { phaseTime=0; Switches++; }
                Activity=washing ? "Auto Wash / clearing loose dirt" : "Auto Work / preparing loose dirt";
                return new PanIntent { Work=washing?0:1, Wash=washing?1:0 };
            }
            if (work)
            {
                bool prepared=sim.CompactedSediment<=.045f && sim.Capture>=.95f && sim.Stones==0;
                Activity=prepared ? "Prepared / wash when you like" : "Auto Work / you wash and collect";
                return new PanIntent { Work=prepared?0:1 };
            }
            bool plateau=sim.NeedsWork;
            Activity=plateau ? "Needs Work / hold left mouse" : "Auto Wash / you work and collect";
            return new PanIntent { Wash=plateau?0:1 };
        }
        public bool ShouldCollect(PanSimulation sim, Progression progress, float dt, bool blocked=false)
        {
            if (blocked) return false;
            if (!sim.Ready || !progress.AutomationOn(2)) { readyTime=0; return false; }
            // Leave a visible, audible payoff before the existing Collect command prepares the next scoop.
            readyTime+=Mathf.Clamp(dt,0,.05f);
            if (readyTime<2.2f) return false;
            readyTime=0;
            return true;
        }
    }
}
