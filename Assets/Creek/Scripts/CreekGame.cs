using System;
using System.IO;
using UnityEngine;

namespace RiffleCreek
{
    public sealed class CreekGame : MonoBehaviour
    {
        public Camera sceneCamera;
        public PanView pan;
        public Diorama diorama;
        public PanSimulation Simulation {get;private set;}
        public Progression Progress {get;private set;}
        public CreekAudio Sound {get;private set;}
        public PanIntent Intent {get;private set;}
        public string Toast {get;private set;}
        public float ToastTime {get;private set;}
        public int LastGoldGain {get;private set;}
        public float GoldGainTime {get;private set;}
        public bool ShowHelp {get;set;}
        public bool Paused {get;private set;}
        public bool VerificationDrive {get;set;}
        public bool VerificationMode {get;private set;}
        public string SaveMessage => save == null ? null : save.Message;
        public string SavePath => save == null ? "" : save.Path;
        public PanAutomation Assistance {get;} = new PanAutomation();
        readonly PanInput input=new PanInput();
        CreekUI ui;
        SaveStore save;
        bool wasReady;

        void Awake()
        {
            Application.targetFrameRate=120; QualitySettings.vSyncCount=1;
            var args=Environment.GetCommandLineArgs();
            bool smoke=Array.IndexOf(args,"--smoke-test")>=0;
            bool chapter=Array.IndexOf(args,"--progression-check")>=0;
            VerificationMode=smoke || chapter || Array.IndexOf(args,"--input-check")>=0;
            VerificationDrive=smoke || chapter;
            string path=VerificationMode ? Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest/Verification/"+Guid.NewGuid().ToString("N")+"/progress.txt"))
                : Path.Combine(Application.persistentDataPath,"alder-creek-v1.txt");
            save=new SaveStore(path); Progress=save.Load(); Simulation=new PanSimulation();
            pan.Initialize(); diorama.Initialize(); diorama.ApplyProgress(Progress);
            Sound=gameObject.AddComponent<CreekAudio>(); Sound.Initialize();
            Simulation.StoneExited+=Sound.Stone;
            ui=gameObject.AddComponent<CreekUI>(); ui.Initialize(this);
            Scoop();
            if(save.Message!=null)Notify(save.Message);
            else if(Progress.CollectedPans>0)Notify("Welcome back. Your camp is just as you left it.");
            if(smoke)gameObject.AddComponent<SmokePlaytest>();
            if(chapter)gameObject.AddComponent<ChapterPlaytest>();
            if(Array.IndexOf(args,"--input-check")>=0)gameObject.AddComponent<InputProbe>();
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) { if(ShowHelp)ShowHelp=false; else Paused=!Paused; }
            if(Input.GetKeyDown(KeyCode.H))ShowHelp=!ShowHelp;
            if(Input.GetKeyDown(KeyCode.M))Sound.ToggleMute();
            float dt=Mathf.Min(Time.deltaTime,.05f);
            if(Paused || ShowHelp || !Application.isFocused)
            {
                input.Read(sceneCamera,pan.transform,true,true); Intent=default;
                Assistance.Resolve(Simulation,Progress,default,dt,true);
                Sound.Tick(default,false,dt); return;
            }
            if(!VerificationDrive)
            {
                if(Input.GetKeyDown(KeyCode.E)) { if(Simulation.Ready)Collect(); else if(!Simulation.Loaded)Scoop(); }
                TickActions(input.Read(sceneCamera,pan.transform,ui.BlocksPointer(),false),dt);
            }
            pan.Render(Simulation,Intent,Progress,dt);
            diorama.Animate(Progress,Intent,Simulation.Loaded&&!Simulation.Ready,dt);
            Sound.Tick(Intent,Simulation.Loaded&&!Simulation.Ready,dt);
            ToastTime=Mathf.Max(0,ToastTime-dt); GoldGainTime=Mathf.Max(0,GoldGainTime-dt);
            AnnounceReady();
        }
        public void TickActions(PanIntent manual,float dt,bool blocked=false)
        {
            if(blocked || Paused || ShowHelp) { Intent=default; return; }
            ApplyPanActions(Assistance.Resolve(Simulation,Progress,manual,dt),dt);
            AnnounceReady();
            if(Assistance.ShouldCollect(Simulation,Progress,dt))Collect();
        }
        void AnnounceReady()
        {
            if(Simulation.Ready&&!wasReady)
            {
                Sound.Ready();
                Notify(Progress.AutomationOn(2) ? "Gold revealed. The catch tray collects after a little glint." :
                    Simulation.HasNugget ? "A small nugget in the black sand. Press E to collect." : "Gold in the black sand. Press E to collect.");
            }
            wasReady=Simulation.Ready;
        }
        public void Scoop()
        {
            if(Simulation.Loaded)return;
            Simulation.Scoop(Progress.ScoopLevel,Progress.CollectedPans,Progress.SluiceActive);
            Sound.Scoop(); wasReady=false;
        }
        public void Collect()
        {
            if(!Simulation.Ready)return;
            int amount=Simulation.Collect(); bool unlock=Progress.Credit(amount); Progress.RecordCollection();
            LastGoldGain=amount; GoldGainTime=3.5f; Sound.Gold(unlock);
            Notify(unlock ? "The sluice is flowing. It pre-screens a little dirt from each new load." :
                Progress.CollectedPans==1 ? "Your first pan. A little gold for better tools." : "+"+amount+" gold. A fresh scoop is waiting.");
            diorama.ApplyProgress(Progress); Persist(); Scoop();
        }
        public void Buy(int index)
        {
            if(!Progress.Buy(index))return;
            Sound.Upgrade();
            Notify(index==0 ? "Smoother enamel. Work and Wash clear material faster." :
                index==1 ? "Improved riffles. Heavy concentrate settles sooner." : "A richer scoop. More gold in your next load.");
            diorama.ApplyProgress(Progress); Persist();
        }
        public void BuyAutomation(int kind)
        {
            if(!Progress.BuyAutomation(kind))return;
            Assistance.Reset(); Sound.Upgrade();
            Notify(kind==0 ? "Auto Work is on. You still wash and collect." :
                kind==1 ? "Auto Wash is on. The pan alternates Work and Wash; you collect." :
                "Alder Creek is in good hands. The catch tray now collects revealed gold.");
            diorama.ApplyProgress(Progress); Persist();
        }
        public void ToggleAutomation(int kind) { if(Progress.ToggleAutomation(kind)) { Assistance.Reset(); Persist(); } }
        public void Resume() { Paused=false; }
        public bool ResetSave()
        {
            if(!save.Reset()) { Notify(save.Message); return false; }
            Simulation.StoneExited-=Sound.Stone;
            Simulation=new PanSimulation(); Simulation.StoneExited+=Sound.Stone;
            Progress=new Progression(); Assistance.Reset(); wasReady=false; Intent=default; GoldGainTime=0;
            diorama.ApplyProgress(Progress); Persist(); Scoop(); Notify("A fresh morning at Alder Creek.");
            return true;
        }
        public bool ReloadSaveForVerification()
        {
            if(!VerificationMode)throw new InvalidOperationException("Verification mode required.");
            Simulation.StoneExited-=Sound.Stone; Progress=save.Load();
            Simulation=new PanSimulation(); Simulation.StoneExited+=Sound.Stone;
            Assistance.Reset(); wasReady=false; Intent=default; diorama.ApplyProgress(Progress); Scoop();
            return !save.WriteBlocked;
        }
        void Persist() { if(!save.Save(Progress))Notify(save.Message); }
        void OnApplicationQuit() { if(save!=null && Progress!=null)Persist(); }
        public void Notify(string message) {Toast=message;ToastTime=5;}
        public void ApplyPanActions(PanIntent intent,float dt)
        {
            Intent=intent; Simulation.Step(intent,dt,Progress.PanLevel,Progress.RiffleLevel);
        }
        public string Stage => !Simulation.Loaded?"A fresh start":Simulation.Ready?"Gold ready — collect":Simulation.NeedsWork&&Intent.Work==0?"Loosen what remains":Intent.Wash>0?(Simulation.Sediment<.18f?"Gold coming through":"Washing loose sediment"):Intent.Work>0?(Simulation.Looseness<.98f||Simulation.Stones>0?"Loosening and separating":"Ready to wash"):Simulation.Looseness<.65f||Simulation.Stones>2?"Hold LMB to work":"Hold Space / RMB to wash";
        public string Hint => Simulation.Ready ? (Progress.AutomationOn(2)?"Take a moment for the gold. The catch tray will collect shortly.":"Press E to collect gold and prepare the next scoop. Leftover stones are fine.") :
            Progress.AutomationOn(0)&&Progress.AutomationOn(1) ? "Your pan is assisted. Hold either action to take over; release to let the camp help." :
            Progress.AutomationOn(0) ? "Auto Work prepares the dirt. Hold Space / RMB to wash; E collects." :
            Progress.AutomationOn(1) ? "Hold LMB to prepare the dirt. Release for Auto Wash; E collects." :
            Simulation.NeedsWork&&Intent.Work==0 ? "Loose dirt has washed away. Hold left mouse to loosen what remains." :
            Intent.Wash>0 ? "Water carries loose dirt over the front. Work again when it stops clearing." :
            "Hold LMB to work; then Space / RMB to wash. Alternate as needed. No precise timing.";
    }
}
