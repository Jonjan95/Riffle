using System;
using UnityEngine;

namespace RiffleCreek
{
    public enum MaterialKind { Sediment, Stone, BlackSand, Gold }
    public struct Grain
    {
        public Vector2 Position, Velocity;
        public float Size, Spin, Exit, LipWork;
        public MaterialKind Kind;
        public bool Active;
    }

    [Serializable]
    public sealed class PanSimulation
    {
        public const int GrainCount=154;
        public Grain[] Grains {get;private set;} = new Grain[GrainCount];
        public float Sediment {get;private set;}
        public float CompactedSediment {get;private set;}
        public float LooseSediment => Mathf.Max(0,Sediment-CompactedSediment);
        public bool NeedsWork => CompactedSediment>.065f && LooseSediment<.04f;
        public float BlackSand {get;private set;}
        public float Looseness {get;private set;}
        public float Stratification {get;private set;}
        public float Capture {get;private set;}
        public float Agitation {get;private set;}
        public float PourAmount {get;private set;}
        public float WorkPhase {get;private set;}
        public float FrontLoad {get;private set;}
        public const float FrontLip=2.88f;
        public static readonly Vector2 Front=Vector2.down;
        public float Concentration => Mathf.Max(Capture,Stratification);
        public float GoldExposure => Mathf.Clamp01(1-Sediment*2.2f-BlackSand*(1-Concentration*.7f)*.7f);
        public int Stones {get;private set;}
        public int GoldValue {get;private set;}
        public int ScoopNumber {get;private set;}
        public bool Loaded {get;private set;}
        bool ready;
        public bool Ready => Loaded && ready;
        public float Clarity => Ready ? 1 : Loaded ? Mathf.Clamp01((1-Sediment)*.72f+(1-BlackSand)*.28f) : 0;
        public event Action StoneExited;
        System.Random random=new System.Random(8124);
        float R(float a,float b) => a+(float)random.NextDouble()*(b-a);

        public void Scoop(int scoopLevel)
        {
            if(Loaded)return;
            ScoopNumber++;
            Sediment=1;CompactedSediment=.82f;BlackSand=1;Looseness=0;Stratification=0;Capture=0;Agitation=0;PourAmount=0;FrontLoad=0;WorkPhase=0;Stones=10;ready=false;
            GoldValue=8+scoopLevel*3+random.Next(0,4);Loaded=true;
            for(int i=0;i<GrainCount;i++)
            {
                var kind=i<88?MaterialKind.Sediment:i<98?MaterialKind.Stone:i<140?MaterialKind.BlackSand:MaterialKind.Gold;
                float a=R(0,Mathf.PI*2), r=Mathf.Sqrt(R(0,1))*1.87f;
                Grains[i]=new Grain {Kind=kind,Active=true, Position=new Vector2(Mathf.Sin(a),Mathf.Cos(a))*r,
                    Size=kind==MaterialKind.Stone?R(.15f,.26f):kind==MaterialKind.Gold?R(.085f,.135f):kind==MaterialKind.BlackSand?R(.055f,.10f):R(.08f,.16f),Spin=R(0,360)};
            }
        }

        public int Collect()
        {
            if(!Ready)return 0;
            Loaded=false;
            for(int i=0;i<GrainCount;i++) { var g=Grains[i];g.Active=false;Grains[i]=g; }
            return GoldValue;
        }

        public void Step(PanIntent input, float dt, int panLevel, int riffleLevel)
        {
            if(!Loaded)return;
            dt=Mathf.Clamp(dt,0,.05f);
            float efficiency=1+panLevel*.18f;
            float work=Mathf.Clamp01(input.Work),pour=Mathf.Clamp01(input.Wash);
            Agitation=Mathf.MoveTowards(Agitation,work,dt*(work>Agitation?7:3));
            PourAmount=Mathf.MoveTowards(PourAmount,pour,dt*(pour>PourAmount?9:4));
            WorkPhase+=dt*7.5f;
            FrontLoad=Mathf.Clamp01(FrontLoad+dt*(Agitation*.32f+PourAmount*.04f*Looseness));
            // Work releases a finite pool of compacted dirt. Wash only spends the loose pool;
            // prolonged pouring can never supply the missing work, and no timing window is required.
            CompactedSediment=Mathf.Max(0,CompactedSediment-dt*Agitation*.105f*(.65f+Stratification*.35f)*efficiency);
            Looseness=1-CompactedSediment/.82f;
            Stratification=Mathf.Clamp01(Stratification+dt*Agitation*.28f*Looseness*efficiency);
            Capture=Mathf.Clamp01(Capture+dt*Agitation*.32f*Stratification*(1+riffleLevel*.35f));
            float wash=PourAmount*dt*efficiency*(.20f+Looseness*.80f)*(.65f+Stratification*.35f+Capture*.5f);
            Sediment=Mathf.Max(CompactedSediment,Sediment-wash*.15f);
            if(Sediment<.3f)
            {
                BlackSand=Mathf.Max(0,BlackSand-wash*.16f*(.8f+Capture*.5f));
            }
            for(int i=0;i<GrainCount;i++)
            {
                Grain g=Grains[i];
                if(!g.Active)continue;
                if(g.Exit>0)
                {
                    g.Exit+=dt; g.Position+=g.Velocity*dt;
                    if(g.Exit>(g.Kind==MaterialKind.Stone?.7f:.55f))g.Active=false;
                    Grains[i]=g;continue;
                }
                bool light=g.Kind==MaterialKind.Sediment, stone=g.Kind==MaterialKind.Stone, heavy=g.Kind==MaterialKind.BlackSand;
                if(stone) { StepStone(ref g,dt);Grains[i]=g;continue; }
                if(light && (i+.5f)/88> Sediment) { StartExit(ref g);Grains[i]=g;continue; }
                if(heavy && (i-98+.5f)/42>BlackSand) { StartExit(ref g);Grains[i]=g;continue; }
                float activity=Mathf.Max(Agitation,PourAmount);
                Vector2 force=Vector2.zero;
                if(light)
                {
                    // Light grains visibly move toward the player even during a stationary LMB hold.
                    force=Front*(Agitation*3.6f+PourAmount*3.0f*Mathf.Clamp01(LooseSediment*4));
                    force.x+=Mathf.Sin(WorkPhase+i*.17f)*Agitation*.65f-g.Position.x*activity*.32f;
                }
                if(heavy || g.Kind==MaterialKind.Gold)
                {
                    // Heavy concentrate gathers just inside the front riffles, below the waste exit.
                    Vector2 pocket=new Vector2(Mathf.Sin(i*2.7f)*.70f,-1.62f+Mathf.Cos(i)*.13f);
                    force+=(pocket-g.Position)*Agitation*(heavy?.95f:.85f)*(1+Capture*.6f);
                }
                g.Velocity=(g.Velocity+force*dt)*Mathf.Exp(-dt*3.8f);
                g.Position+=g.Velocity*dt;
                g.Spin+=g.Velocity.magnitude*dt*(stone?90:25);
                float radius=g.Position.magnitude;
                if(radius>2.13f)
                {
                    g.Position=g.Position/radius*2.13f;
                    g.Velocity-=Vector2.Dot(g.Velocity,g.Position.normalized)*g.Position.normalized*1.35f;
                }
                Grains[i]=g;
            }
            // Finish the concentrate, not a stone-count checklist. Latch readiness until collection.
            ready |= Sediment<=.065f && BlackSand<=Mathf.Lerp(.06f,.24f,Concentration) && GoldExposure>=.90f;
        }

        void StepStone(ref Grain g,float dt)
        {
            float radius=g.Position.magnitude;
            bool onFrontSlope=g.Position.y<-.9f && radius>1.85f;
            // Water can help an already worked stone at the lip, but cannot mobilize the bulk gravel.
            float lipAssist=onFrontSlope && g.LipWork>.1f ? PourAmount*.35f : 0;
            float effort=Mathf.Max(lipAssist,Agitation);
            // Repeated work on the slope loosens its purchase. This is contact work, never an idle timer.
            if(onFrontSlope && Agitation>.02f)g.LipWork=Mathf.Clamp01(g.LipWork+dt*Agitation*.45f);
            else if(radius<1.6f)g.LipWork=Mathf.Max(0,g.LipWork-dt*.08f);
            Vector2 force=new Vector2(Mathf.Sin(WorkPhase+g.Size*7)*Agitation*.8f,0);
            force+=Front*(Agitation*1.45f+lipAssist*.85f+g.LipWork*effort*1.8f);
            // A broad downhill channel pulls side-wall stones round toward the working lip.
            force.x-=g.Position.x*(lipAssist*.75f+Agitation*.8f);
            g.Velocity=(g.Velocity+force*dt)*Mathf.Exp(-dt*2.8f);
            g.Position+=g.Velocity*dt;
            g.Spin+=g.Velocity.magnitude*dt*160;
            radius=g.Position.magnitude;
            if(radius>=FrontLip && g.Position.y<-1.9f)
            {
                g.Exit=.001f;
                // Release over the front, preserving only a little lateral momentum.
                g.Velocity=new Vector2(Mathf.Clamp(g.Velocity.x,-.35f,.35f),-2.5f);
                Stones--;StoneExited?.Invoke();
            }
            else if(radius>2.87f && g.Position.y>=-1.9f)
            {
                Vector2 normal=g.Position/radius;
                g.Position=normal*2.87f;
                // Remove only outward speed; preserve sliding along the wall instead of bouncing into a stable pocket.
                g.Velocity-=Mathf.Max(0,Vector2.Dot(g.Velocity,normal))*normal;
            }
        }

        void StartExit(ref Grain g)
        {
            g.Exit=.001f;
            if(g.Kind!=MaterialKind.Stone)g.Position=new Vector2(Mathf.Clamp(g.Position.x,-1.5f,1.5f),-2.12f);
            g.Velocity=new Vector2(g.Position.x*.3f,-3.6f);
        }
    }
}
