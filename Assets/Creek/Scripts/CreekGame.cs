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
        public bool ShowHelp {get;set;}
        public bool Paused {get;private set;}
        public bool VerificationDrive {get;set;}
        readonly PanInput input=new PanInput();
        CreekUI ui;
        bool wasReady;

        void Awake()
        {
            Application.targetFrameRate=120;
            QualitySettings.vSyncCount=1;
            Simulation=new PanSimulation();Progress=new Progression();
            pan.Initialize();diorama.Initialize();
            Sound=gameObject.AddComponent<CreekAudio>();Sound.Initialize();
            Simulation.StoneExited+=Sound.Stone;
            ui=gameObject.AddComponent<CreekUI>();ui.Initialize(this);
            ShowHelp=false;
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--smoke-test")>=0)gameObject.AddComponent<SmokePlaytest>();
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--input-check")>=0)gameObject.AddComponent<InputProbe>();
            Scoop();
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) { if(ShowHelp)ShowHelp=false;else Paused=!Paused; }
            if(Input.GetKeyDown(KeyCode.H))ShowHelp=!ShowHelp;
            if(Input.GetKeyDown(KeyCode.M))Sound.ToggleMute();
            float dt=Mathf.Min(Time.deltaTime,.05f);
            if(Paused || ShowHelp) {input.Read(sceneCamera,pan.transform,true,true);Intent=default;Sound.Tick(default,false,dt);return;}
            if(!VerificationDrive)
            {
                if(Input.GetKeyDown(KeyCode.E)) {if(Simulation.Ready)Collect();else if(!Simulation.Loaded)Scoop();}
                ApplyPanActions(input.Read(sceneCamera,pan.transform,ui.BlocksPointer(),false),dt);
            }
            pan.Render(Simulation,Intent,Progress,dt);
            diorama.Animate(Progress.SluiceActive,dt);
            Sound.Tick(Intent,Simulation.Loaded&&!Simulation.Ready,dt);
            ToastTime=Mathf.Max(0,ToastTime-dt);
            if(Simulation.Ready&&!wasReady) {Sound.Ready();Notify("Gold is ready. Press E to collect; leftover stones are fine.");}
            wasReady=Simulation.Ready;
        }
        public void Scoop()
        {
            if(Simulation.Loaded)return;
            Simulation.Scoop(Progress.ScoopLevel);Sound.Scoop();wasReady=false;
        }
        public void Collect()
        {
            if(!Simulation.Ready)return;
            int amount=Simulation.Collect();bool unlock=Progress.Credit(amount);Sound.Gold(unlock);
            Notify(unlock?"40 gold collected! Your little sluice is flowing.":"+"+amount+" gold. A little richer, a little better.");
            // Collection is still manual. A fresh load is prepared immediately for the next held action.
            Scoop();
        }
        public void Buy(int index)
        {
            if(Progress.Buy(index)) {Sound.Upgrade();Notify(index==0?"A smoother pan. Material separates faster.":index==1?"Deeper riffles. A steadier concentrate.":"A better scoop. More gold in your next load.");}
        }
        public void Notify(string message) {Toast=message;ToastTime=5;}
        public void ApplyPanActions(PanIntent intent,float dt)
        {
            Intent=intent;Simulation.Step(intent,dt,Progress.PanLevel,Progress.RiffleLevel);
        }
        public string Stage => !Simulation.Loaded?"A fresh start":Simulation.Ready?"Gold ready — collect":Simulation.NeedsWork&&Intent.Work==0?"Loosen what remains":Intent.Wash>0?(Simulation.Sediment<.18f?"Gold coming through":"Washing loose sediment"):Intent.Work>0?(Simulation.Looseness<.98f||Simulation.Stones>0?"Loosening and separating":"Ready to wash"):Simulation.Looseness<.65f||Simulation.Stones>2?"Hold LMB to work":"Hold Space / RMB to wash";
        public string Hint => !Simulation.Loaded?"Press E for a scoop.":Simulation.Ready?"Press E to collect gold and prepare the next scoop. Leftover stones are fine.":Simulation.NeedsWork&&Intent.Work==0?"The loose dirt has washed away. Hold left mouse to loosen the material that remains.":Intent.Wash>0?"Water carries loose dirt over the front riffles. Work again when the dirt stops clearing.":Intent.Work>0&&(Simulation.Looseness<.98f||Simulation.Stones>0)?"Left mouse loosens dirt and moves stones. Space or right mouse washes the loose material away.":Simulation.Looseness<.65f||Simulation.Stones>2?"Hold left mouse on the pan to loosen and shake. Keep the cursor still.":"The material is worked. Release left mouse, then hold Space or right mouse to wash.";
    }
}
