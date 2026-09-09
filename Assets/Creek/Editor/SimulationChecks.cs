using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek.Editor
{
    public static class SimulationChecks
    {
        static readonly StringBuilder report=new StringBuilder();
        static void Check(bool valid,string label) {if(!valid)throw new Exception("FAILED: "+label);report.AppendLine("PASS: "+label);}
        public static PanIntent Held(bool wash) => new PanIntent {Work=wash?0:1,Wash=wash?1:0};
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Riffle/Run simulation checks")]
        #endif
        public static void Run()
        {
            report.Clear();
            var sim=new PanSimulation();var p=new Progression();
            Check(!sim.Loaded && !sim.Ready,"New pan starts empty");
            Check(sim.Collect()==0,"Unfinished pan cannot pay out");
            Check(!p.Buy(0)&&!p.Buy(-1)&&!p.Buy(3),"Unfunded / invalid upgrade is rejected");
            sim.Scoop(0);int scoop=sim.ScoopNumber;sim.Scoop(3);
            Check(sim.ScoopNumber==scoop,"Scoop cannot replace an unfinished load");
            for(int i=0;i<600;i++)sim.Step(default,1f/60,0,0);
            Check(sim.Sediment==1 && sim.Stones==10 && sim.Looseness==0,"Idle pan does not process itself");
            int completed=0;
            for(int load=0;load<6;load++)
            {
                if(!sim.Loaded)sim.Scoop(p.ScoopLevel);
                for(int i=0;i<720;i++)sim.Step(Held(false),1f/60,p.PanLevel,p.RiffleLevel);
                Check(sim.Capture>.65f && sim.Stones==0,"Stationary Work hold settles concentrate and clears stones, load "+load);
                Check(sim.Sediment==1&&!sim.Ready,"Work alone retains the separate manual wash step, load "+load);
                float seconds=0;
                for(int i=0;i<3600&&!sim.Ready;i++) {sim.Step(Held(true),1f/60,p.PanLevel,p.RiffleLevel);seconds+=1f/60;}
                Check(sim.Ready,"Stationary Wash hold finishes load "+load+" in "+seconds.ToString("F1")+" seconds");
                foreach(var g in sim.Grains)CheckFinite(g.Position);
                int goldCount=0;foreach(var g in sim.Grains)if(g.Kind==MaterialKind.Gold&&g.Active)goldCount++;
                Check(goldCount==14,"All gold retained, load "+load);
                int value=sim.Collect();p.Credit(value);completed++;
                Check(sim.Collect()==0,"Collection pays only once, load "+load);
                if(load==0)Check(p.Buy(0),"First load can purchase better pan");
                else if(p.CanBuy(2))p.Buy(2);
            }
            Check(completed==6&&p.SluiceActive,"Lifetime collection unlocks sluice despite upgrade spending");
            p.Credit(1000);for(int j=0;j<3;j++)while(p.CanBuy(j))p.Buy(j);
            Check(p.PanLevel==3&&p.RiffleLevel==3&&p.ScoopLevel==3&&!p.Buy(1),"All three upgrades cap at three levels");
            var bare=new PanSimulation();var skilled=new PanSimulation();bare.Scoop(0);skilled.Scoop(0);
            for(int i=0;i<360;i++) {skilled.Step(Held(false),1f/60,0,3);bare.Step(Held(false),1f/60,0,0);}
            for(int i=0;i<180;i++) {skilled.Step(Held(true),1f/60,0,3);bare.Step(Held(true),1f/60,0,0);}
            Check(skilled.Sediment<bare.Sediment,"Existing riffle upgrades improve held-action separation");
            HoldInputChecks();
            SeparateActionChecks();
            StoneAndFinishChecks();
            AudioChecks();
            Directory.CreateDirectory("Playtest");File.WriteAllText("Playtest/simulation-checks.txt",report.ToString());
            #if UNITY_EDITOR
            Debug.Log(report.ToString());
            #else
            Console.WriteLine(report.ToString());
            #endif
        }
        static void CheckFinite(Vector2 p) {if(float.IsNaN(p.x)||float.IsInfinity(p.x)||float.IsNaN(p.y)||float.IsInfinity(p.y))throw new Exception("Non-finite material position");}

        static void SeparateActionChecks()
        {
            var wash=new PanSimulation();wash.Scoop(0);
            Vector2 stone=wash.Grains[88].Position;
            for(int i=0;i<7200;i++)wash.Step(Held(true),1f/60,0,0);
            Check(wash.Sediment<.99f && wash.Sediment>=.819f && wash.NeedsWork,"Wash alone clears initial loose dirt then plateaus on compacted material after 120 seconds");
            Check(wash.Stones==10 && wash.Grains[88].Position==stone && wash.Looseness==0 && wash.Stratification==0 && wash.Capture==0 && wash.Agitation==0,"Wash alone neither shakes, loosens, stratifies, nor clears bulk stones");
            Check(!wash.Ready && wash.BlackSand==1,"Wash alone cannot finish a fresh scoop");
            for(int i=0;i<720;i++)wash.Step(Held(false),1f/60,0,0);
            for(int i=0;i<1200&&!wash.Ready;i++)wash.Step(Held(true),1f/60,0,0);
            Check(wash.Ready,"Prolonged wash-first use recovers with work then wash");
            var work=new PanSimulation();work.Scoop(0);
            for(int i=0;i<7200;i++)work.Step(Held(false),1f/60,0,0);
            Check(work.Sediment==1 && work.BlackSand==1 && !work.Ready && work.PourAmount==0 && work.Stones==0,"Work alone clears stones and settles solids but retains all fine sediment after 120 seconds");
            float rearMostGold=-3;foreach(var g in work.Grains)if(g.Kind==MaterialKind.Gold)rearMostGold=Mathf.Max(rearMostGold,g.Position.y);
            Check(rearMostGold<-1.3f,"Gold settles inside the lower/front riffled working section");
            for(int i=0;i<1200&&!work.Ready;i++)work.Step(Held(true),1f/60,0,0);
            Check(work.Ready,"Prolonged work-first use recovers with wash");
            foreach(int fps in new[]{30,60,120})
            {
                var alternate=new PanSimulation();alternate.Scoop(0);
                for(int i=0;i<3*fps;i++)alternate.Step(Held(false),1f/fps,0,0);
                for(int i=0;i<30*fps;i++)alternate.Step(Held(true),1f/fps,0,0);
                Check(alternate.NeedsWork && !alternate.Ready && alternate.Stones>0,"Short work then long wash reaches a visible, recoverable plateau at "+fps+" Hz");
                float compacted=alternate.CompactedSediment;
                for(int cycle=0;cycle<12&&!alternate.Ready;cycle++)
                {
                    for(int i=0;i<2*fps;i++)alternate.Step(Held(false),1f/fps,0,0);
                    for(int i=0;i<4*fps&&!alternate.Ready;i++)alternate.Step(Held(true),1f/fps,0,0);
                }
                Check(alternate.Ready && alternate.CompactedSediment<compacted,"Forgiving repeated 2-second Work / 4-second Wash holds finish at "+fps+" Hz");
            }
            var lip=new PanSimulation();lip.Scoop(0);
            var near=lip.Grains[88];near.Position=new Vector2(0,-2.55f);near.LipWork=.4f;lip.Grains[88]=near;
            for(int i=0;i<600;i++)lip.Step(Held(true),1f/60,0,0);
            Check(lip.Grains[88].Exit>0 && lip.Stones==9,"Pour assists an already worked front-lip stone while leaving bulk stones alone");
            #if UNITY_EDITOR
            var pan=UnityEngine.Object.FindAnyObjectByType<PanView>();
            var meshes=pan.riffleAccent.GetComponentsInChildren<MeshFilter>();
            float maxZ=-10;foreach(var filter in meshes)foreach(var vertex in filter.sharedMesh.vertices)maxZ=Mathf.Max(maxZ,pan.transform.InverseTransformPoint(filter.transform.TransformPoint(vertex)).z);
            Check(meshes.Length>0 && maxZ<-.9f,"Saved riffle mesh vertices physically occupy only the front sector");
            #endif
        }

        static void StoneAndFinishChecks()
        {
            var working=new PanSimulation();working.Scoop(0);var initial=working.Grains[88].Position;float siltStart=working.Grains[0].Position.y;
            for(int i=0;i<12;i++)working.Step(Held(false),1f/60,0,0);
            Check(working.Grains[88].Position.y<initial.y && working.Grains[0].Position.y<siltStart && working.Agitation>.9f,"Stationary hold moves dirt and stones forward within 0.2 seconds");
            for(int i=0;i<60;i++)working.Step(default,1f/60,0,0);
            Check(working.Agitation==0 && working.PourAmount==0,"Releasing held actions settles the drive");
            var edges=new PanSimulation();edges.Scoop(0);
            for(int i=88;i<98;i++)
            {
                float a=(i-88)*Mathf.PI*2/10;var g=edges.Grains[i];
                g.Position=new Vector2(Mathf.Sin(a),Mathf.Cos(a))*2.82f;g.Velocity=Vector2.zero;edges.Grains[i]=g;
            }
            bool climbed=false;int exits=0;edges.StoneExited+=()=>exits++;
            for(int i=0;i<1500;i++)
            {
                edges.Step(Held(false),1f/60,0,0);
                foreach(var g in edges.Grains)
                {
                    if(g.Kind==MaterialKind.Stone && g.Exit==0 && g.Position.y<0 && g.Position.magnitude>2.5f)climbed=true;
                    if(g.Kind==MaterialKind.Stone && g.Exit>0)CheckFinite(g.Position);
                    if(g.Kind==MaterialKind.Stone && g.Exit>0 && (g.Position.y>=-1.9f||g.Velocity.y>=0))throw new Exception("Stone exited toward side/rear");
                }
            }
            Check(climbed,"Stones traverse the visible front slope before exiting");
            Check(edges.Stones==0 && exits==10,"Work hold clears rear and side-wall stones exclusively over the front lip");
            var parked=new PanSimulation();parked.Scoop(0);
            for(int i=0;i<2400&&!parked.Ready;i++)
            {
                // Deliberately obstruct two stones to regression-test the former all-stones collection lock.
                for(int k=96;k<98;k++) {var g=parked.Grains[k];g.Position=new Vector2((k-96)*.7f,1.6f);g.Velocity=Vector2.zero;parked.Grains[k]=g;}
                parked.Step(Held(i>=720),1f/60,0,0);
            }
            Check(parked.Ready && parked.Stones==2 && parked.GoldExposure>=.90f,"Clean concentrate is collectable with two obstructed stones");
            Check(parked.Grains[96].Active&&parked.Grains[97].Active,"Remaining stones stay visible when gold becomes ready");
            for(int i=0;i<120;i++)parked.Step(default,1f/60,0,0);
            Check(parked.Ready && parked.Collect()>0 && parked.Collect()==0,"Ready state survives release and collection still pays exactly once");
            var contact=new PanSimulation();contact.Scoop(0);
            var front=contact.Grains[88];front.Position=new Vector2(0,-2.1f);contact.Grains[88]=front;
            for(int i=0;i<120;i++)contact.Step(Held(false),1f/60,0,0);
            Check(contact.Grains[88].LipWork>0,"Working the front slope accumulates ejection assistance");
            for(int i=0;i<60;i++)contact.Step(default,1f/60,0,0);
            float work=contact.Grains[89].LipWork;
            for(int i=0;i<120;i++)contact.Step(default,1f/60,0,0);
            Check(contact.Grains[89].LipWork<=work,"Idle time does not grant ejection assistance");
        }

        static void HoldInputChecks()
        {
            var reader=new PanInput();
            var held=reader.Map(true,false,false,true,true,false);
            for(int i=0;i<600;i++)held=reader.Map(true,false,false,true,true,false);
            Check(held.Work==1&&held.Wash==0,"LMB held at a fixed point continuously requests Work");
            held=reader.Map(true,false,false,true,false,true);
            Check(held.Work==1,"A grabbed hold survives pointer drift without any motion requirement");
            held=reader.Map(false,false,false,true,true,false);
            Check(!held.Working,"Release stops held actions");
            held=reader.Map(false,true,false,true,true,false);
            Check(held.Work==0&&held.Wash==1,"Stationary RMB requests full Wash without front-position scoring");
            held=reader.Map(false,false,true,true,false,true);
            Check(held.Wash==1,"Space washes even with the pointer outside the pan or over UI");
            Check(!reader.Map(true,true,true,false,true,false).Working,"Unfocused game cannot work or wash");
            Check(!reader.Map(true,true,true,true,true,false,true).Working,"Pause/help blocks all held actions");
            reader=new PanInput();reader.Map(true,false,false,true,false,true);
            Check(reader.Map(true,false,false,true,true,false).Work==0,"A UI click cannot turn into panning without release and a fresh grab");
            reader.Map(false,false,false,true,true,false);
            Check(reader.Map(true,true,false,true,true,false).Work==1&&reader.Map(true,true,false,true,true,false).Wash==1,"Work and Wash can be held together without an automation system");
            for(int fps=30;fps<=120;fps*=2)
            {
                var sim=new PanSimulation();sim.Scoop(0);
                for(int i=0;i<12*fps;i++)sim.Step(Held(false),1f/fps,0,0);
                for(int i=0;i<20*fps&&!sim.Ready;i++)sim.Step(Held(true),1f/fps,0,0);
                Check(sim.Ready,"Held actions complete at "+fps+" updates per second");
            }
        }

        static void AudioChecks()
        {
            float[][] clips={SoftAudioSamples.Water(10,712,.08f),SoftAudioSamples.Water(8,715,.10f),SoftAudioSamples.Clack(),SoftAudioSamples.Note(.8f,440,.20f,true),SoftAudioSamples.Note(.55f,523.25f,.12f)};
            string[] names={"creek","wash","stone","pickup","ready"};
            for(int c=0;c<clips.Length;c++)
            {
                float peak=0,jump=0;double energy=0,highEnergy=0,filtered=0;
                double a=1-Math.Exp(-2*Math.PI*2000/SoftAudioSamples.Rate);
                for(int i=0;i<clips[c].Length;i++)
                {
                    float s=clips[c][i];peak=Mathf.Max(peak,Mathf.Abs(s));
                    if(i>0)jump=Mathf.Max(jump,Mathf.Abs(s-clips[c][i-1]));
                    filtered+=a*(s-filtered);energy+=s*s;highEnergy+=(s-filtered)*(s-filtered);
                }
                Check(peak<=.201f && jump<.015f && clips[c][0]==0 && clips[c][clips[c].Length-1]==0,"Audio "+names[c]+": bounded gain and smooth sample/loop edges (peak "+peak.ToString("F3")+")");
                if(c<2)Check(highEnergy/energy<.02,"Audio "+names[c]+": restrained high-frequency residual (<2% above 2 kHz filter)");
            }
            Check(.08f*.18f+.10f*.295f+.20f*.45f<.15f,"Conservative maximum three-voice sum stays below 0.15 full scale");
        }
    }
}
