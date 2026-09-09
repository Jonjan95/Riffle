using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek
{
    // Explicit command-line verification; no automation is enabled in normal play.
    public sealed class SmokePlaytest : MonoBehaviour
    {
        CreekGame game;
        string dir;
        bool failed;
        readonly StringBuilder report=new StringBuilder();
        void Check(bool valid,string text)
        {
            report.AppendLine((valid?"PASS: ":"FAIL: ")+text);if(!valid)failed=true;
        }
        IEnumerator Start()
        {
            game=GetComponent<CreekGame>();game.VerificationDrive=true;
            dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest"));Directory.CreateDirectory(dir);
            report.AppendLine("Front riffles and separate Work/Wash: stationary button samples, no gestures.");
            yield return new WaitForSeconds(.6f);yield return Capture("01-fresh-scoop.png");
            var riffles=game.pan.riffleAccent;
            var riffleMeshes=riffles.GetComponentsInChildren<MeshFilter>();
            float maxRiffleZ=-10;foreach(var filter in riffleMeshes)foreach(var vertex in filter.sharedMesh.vertices)maxRiffleZ=Mathf.Max(maxRiffleZ,game.pan.transform.InverseTransformPoint(filter.transform.TransformPoint(vertex)).z);
            Check(riffleMeshes.Length>0 && maxRiffleZ<-.9f,"Actual saved riffle mesh lies entirely on the front/player-facing side");
            float restingFrontScreenY=game.sceneCamera.WorldToScreenPoint(game.pan.transform.TransformPoint(new Vector3(0,.8f,-2.88f))).y;
            string[] cases={"LMB then Space","LMB then RMB at rear","LMB then RMB at front","holds with release pauses","LMB and RMB together"};
            for(int load=0;load<cases.Length;load++)
            {
                var input=new PanInput();
                Vector3 cursor=game.sceneCamera.WorldToScreenPoint(game.pan.Anchor+new Vector3(.3f,.35f,load==1?1.25f:load==2?-1.25f:0));
                PanIntent Read(bool left,bool right,bool space) => input.Sample(game.sceneCamera,game.pan.transform,cursor,left,right,space,true,false);
                int workFrames=0;
                float dirtStart=game.Simulation.Grains[0].Position.y;
                while(workFrames<720)
                {
                    bool pause=load==3 && workFrames%180>130;
                    game.ApplyPanActions(Read(!pause,load==4&&!pause,false),1f/60);
                    workFrames++;
                    if(load==0&&workFrames<=24)yield return new WaitForSeconds(1f/60);
                    else if(workFrames%4==0)yield return null;
                    if(load==0&&workFrames==12)
                    {
                        yield return null;
                        Check(game.pan.ForwardTilt<-.10f&&game.Simulation.Grains[0].Position.y<dirtStart,"Stationary LMB immediately lowers the front and moves sediment toward the player");
                        Check(game.sceneCamera.WorldToScreenPoint(game.pan.transform.TransformPoint(new Vector3(0,.8f,-2.88f))).y<restingFrontScreenY,"Working front lip also moves downward on screen");
                    }
                    if(load==0&&workFrames==180)yield return Capture("02-hold-work.png");
                    if(game.Simulation.Ready)break;
                }
                Check(game.Simulation.Stones==0,cases[load]+": stones clear through held work");
                if(load==0)Check(game.Simulation.Sediment==1&&!game.Simulation.Ready&&game.pan.VisibleFlow==0,"LMB work leaves all fine sediment and produces no outflow");
                game.ApplyPanActions(Read(false,false,false),1f/60);
                for(int i=0;i<60;i++) {game.ApplyPanActions(default,1f/60);if(i%4==0)yield return null;}
                if(load==0)yield return new WaitForSeconds(.5f);
                if(load==0)Check(game.Simulation.Agitation==0&&Mathf.Abs(game.pan.ForwardTilt)<.20f,"Release settles the pan toward rest");
                int washFrames=0;
                while(!game.Simulation.Ready&&washFrames<3600)
                {
                    bool pause=load==3&&washFrames%180>130;
                    game.ApplyPanActions(Read(false,load!=0&&!pause,load==0&&!pause),1f/60);
                    washFrames++;
                    if(load==0&&washFrames<=24)yield return new WaitForSeconds(1f/60);
                    else if(washFrames%4==0)yield return null;
                    if(load==0&&washFrames==24)
                    {
                        yield return null;
                        Check(game.pan.ForwardTilt<-.6f&&game.pan.VisibleFlow>.9f,"Space immediately pours visible water while lowering the front further");
                        Check(game.pan.OutflowEnd.y<game.pan.OutflowMouth.y&&game.pan.OutflowEnd.z<game.pan.OutflowMouth.z&&game.sceneCamera.WorldToScreenPoint(game.pan.OutflowEnd).y<game.sceneCamera.WorldToScreenPoint(game.pan.OutflowMouth).y,"Rendered outflow falls down in world space and toward the bottom of the screen");
                        Check(game.Simulation.Agitation==0,"Pour alone has no automatic Work rocking drive");
                        yield return Capture("03-hold-pour.png");
                    }
                    if(load==0&&washFrames==180)yield return Capture("04-concentrate.png");
                }
                Check(game.Simulation.Ready,cases[load]+": collectable after "+(workFrames/60f).ToString("F1")+"s work and "+(washFrames/60f).ToString("F1")+"s wash");
                if(load==0) {float goldZ=0;for(int i=140;i<154;i++)goldZ+=game.Simulation.Grains[i].Position.y/14;Check(goldZ<-1.3f && game.pan.blackBed.localPosition.z<-1.3f,"Gold and black sand gather just inside the front working riffles");}
                game.ApplyPanActions(Read(false,false,false),0);
                if(load==0) {for(int i=0;i<60;i++){game.ApplyPanActions(default,1f/60);if(i%4==0)yield return null;}yield return new WaitForSeconds(.5f);yield return Capture("05-gold-ready.png");}
                int before=game.Progress.Gold,value=game.Simulation.GoldValue,scoop=game.Simulation.ScoopNumber;
                game.Collect();
                Check(game.Progress.Gold==before+value&&game.Simulation.Loaded&&!game.Simulation.Ready&&game.Simulation.ScoopNumber==scoop+1,"Manual Collect pays once and prepares the next scoop");
                int balance=game.Progress.Gold;game.Collect();Check(game.Progress.Gold==balance,"A second Collect cannot pay for an unfinished fresh scoop");
                if(load==0)game.Buy(0);else if(load==1)game.Buy(1);else if(load==2)game.Buy(2);
            }
            Check(game.Progress.SluiceActive,"Existing milestone and upgrades still work");
            var probe=new PanInput();var center=game.sceneCamera.WorldToScreenPoint(game.pan.Anchor+Vector3.up*.35f);
            Check(!probe.Sample(game.sceneCamera,game.pan.transform,center,true,true,true,true,false,true).Working,"Help/pause blocks held input");
            game.ShowHelp=true;yield return Capture("06-hold-guide.png");game.ShowHelp=false;
            // Wash-first and short alternating holds exercise the diminishing returns in the player.
            var distinctInput=new PanInput();
            PanIntent Action(bool work,bool wash) => distinctInput.Sample(game.sceneCamera,game.pan.transform,center,work,false,wash,true,false);
            for(int i=0;i<900;i++) {game.ApplyPanActions(Action(false,true),1f/60);if(i%4==0)yield return null;}
            Check(game.Simulation.NeedsWork && game.Simulation.Stones==10 && game.Simulation.Looseness==0 && game.Simulation.Sediment>=.819f && !game.Simulation.Ready,"Space-only clears loose surface dirt then stalls with compacted dirt and all stones");
            yield return Capture("08-wash-only-plateau.png");
            for(int i=0;i<180;i++) {game.ApplyPanActions(Action(true,false),1f/60);if(i%4==0)yield return null;}
            float beforeWash=game.Simulation.Sediment;
            for(int i=0;i<900;i++) {game.ApplyPanActions(Action(false,true),1f/60);if(i%4==0)yield return null;}
            Check(game.Simulation.Sediment<beforeWash-.1f && game.Simulation.NeedsWork && !game.Simulation.Ready,"Short LMB hold makes more dirt washable, then pouring naturally slows again");
            yield return Capture("09-partial-work-plateau.png");
            for(int cycle=0;cycle<12&&!game.Simulation.Ready;cycle++)
            {
                for(int i=0;i<120;i++) {game.ApplyPanActions(Action(true,false),1f/60);if(i%4==0)yield return null;}
                for(int i=0;i<240&&!game.Simulation.Ready;i++) {game.ApplyPanActions(Action(false,true),1f/60);if(i%4==0)yield return null;}
            }
            Check(game.Simulation.Ready,"Repeated stationary Work/Wash holds recover from both plateaus");
            game.Collect();
            // Intentionally park two stones, proving they cannot block the cleaned concentrate.
            for(int i=0;i<3000&&!game.Simulation.Ready;i++)
            {
                for(int k=96;k<98;k++) {var g=game.Simulation.Grains[k];g.Position=new Vector2((k-96)*.7f-.3f,1.6f);g.Velocity=Vector2.zero;game.Simulation.Grains[k]=g;}
                game.ApplyPanActions(new PanIntent {Work=i<720?1:0,Wash=i>=720?1:0},1f/60);
                if(i%4==0)yield return null;
            }
            Check(game.Simulation.Ready&&game.Simulation.Stones==2,"Separate work then wash reveals collectable gold with two obstructed stones");
            for(int i=0;i<60;i++){game.ApplyPanActions(default,1f/60);if(i%4==0)yield return null;}yield return new WaitForSeconds(.5f);yield return Capture("07-hold-ready-with-stones.png");
            File.WriteAllText(Path.Combine(dir,"runtime-checks.txt"),report.ToString());
            yield return new WaitForSeconds(.2f);Application.Quit(failed?2:0);
        }
        IEnumerator Capture(string file)
        {
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,file));yield return null;
        }
    }
}
