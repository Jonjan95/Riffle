using UnityEngine;

namespace RiffleCreek
{
    // Supplies the existing intents. Manual input has priority; no separate processing or payout.
    public sealed class PanAutomation
    {
        public const float CollectDelay = .6f;
        public const float MinimumPhaseDuration = .15f;
        bool washing;
        float phaseTime, readyTime;
        int scoop;
        public string Activity { get; private set; } = "Waiting / manual panning";
        public int Switches { get; private set; }
        public void Reset() { washing=false; phaseTime=0; readyTime=0; scoop=0; Switches=0; Activity="Waiting / manual panning"; }
        void TrackScoop(PanSimulation sim)
        {
            if(scoop==sim.ScoopNumber)return;
            scoop=sim.ScoopNumber; washing=false; phaseTime=0; readyTime=0;
        }
        public PanIntent Resolve(PanSimulation sim, Progression progress, PanIntent manual, float dt, bool blocked = false)
        {
            dt = Mathf.Clamp(dt,0,.05f);
            if (blocked) { Activity="Waiting / paused"; return default; }
            TrackScoop(sim);
            if (!sim.Loaded || sim.Ready)
            {
                Activity=sim.Ready ? (progress.AutomationOn(2)?"Collect / gold revealed":"Waiting / E to collect") : "Waiting / next scoop";
                return default;
            }
            bool work=progress.AutomationOn(0), wash=progress.AutomationOn(1);
            if (manual.Work > 0 || manual.Wash > 0)
            {
                // Full-strength manual input wins this frame. On release, re-evaluate without a handoff timer.
                Activity=manual.Work>0 ? (manual.Wash>0?"Work + Wash / your hands":"Work / your hands") : "Wash / your hands";
                washing=manual.Wash>0 && manual.Work==0; phaseTime=MinimumPhaseDuration;
                return manual;
            }
            if (!work && !wash) { Activity="Waiting / manual panning"; return default; }
            if (work && wash)
            {
                phaseTime+=dt;
                bool old=washing;
                // Widely separated sediment thresholds supply hysteresis. Never wait at a clear plateau.
                if (washing && sim.NeedsWork) washing=false;
                else if (!washing && (sim.CompactedSediment<=.045f ||
                    phaseTime>=MinimumPhaseDuration && sim.LooseSediment>=.42f && sim.Stratification>=.55f)) washing=true;
                if (old != washing) { phaseTime=0; Switches++; }
                Activity=washing ? "Wash / assisted" : "Work / assisted";
                return new PanIntent { Work=washing?0:1, Wash=washing?1:0 };
            }
            if (work)
            {
                bool prepared=sim.CompactedSediment<=.045f && sim.Capture>=.95f && sim.Stones==0;
                Activity=prepared ? "Waiting / ready for your Wash" : "Work / assisted";
                return new PanIntent { Work=prepared?0:1 };
            }
            bool plateau=sim.NeedsWork;
            Activity=plateau ? "Waiting / needs your Work" : "Wash / assisted";
            return new PanIntent { Wash=plateau?0:1 };
        }
        public bool ShouldCollect(PanSimulation sim, Progression progress, float dt, bool blocked=false)
        {
            if (blocked) { Activity="Waiting / paused"; return false; }
            TrackScoop(sim);
            if (!sim.Ready || !progress.AutomationOn(2))
            {
                readyTime=0;
                if(sim.Ready)Activity="Waiting / E to collect";
                return false;
            }
            Activity="Collect / gold revealed";
            // A brief glint, without making an actively watched pan feel stalled. E bypasses this timer.
            readyTime+=Mathf.Clamp(dt,0,.05f);
            if (readyTime<CollectDelay) return false;
            readyTime=0;
            return true;
        }
    }
}
