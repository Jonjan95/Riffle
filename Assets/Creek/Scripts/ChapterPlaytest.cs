using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek
{
    // Opt-in full progression verification. All writes go to a unique disposable save path.
    public sealed class ChapterPlaytest : MonoBehaviour
    {
        CreekGame game;
        string folder;
        readonly StringBuilder report=new StringBuilder();
        int failures;
        void Check(bool valid,string label) { report.AppendLine((valid?"PASS: ":"FAIL: ")+label);if(!valid)failures++; }
        IEnumerator Start()
        {
            game=GetComponent<CreekGame>();game.VerificationDrive=true;
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest/Chapter-"+Screen.width+"x"+Screen.height));
            Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(.8f);
            Check(game.Progress.Gold==0&&!game.Progress.HasAutomation(0)&&game.diorama.Camp.VisibleMilestones==0,"Fresh player has an unchanged manual loop and a sparse camp");
            yield return Capture("01-manual-camp.png");
            float scoopTime=0, total=0;int frame=0,purchases=0;
            bool firstReady=false, firstUpgrade=false;
            while(!game.Progress.FullyAssisted && total<3600)
            {
                for(int step=0;step<20;step++)
                {
                    var s=game.Simulation;var p=game.Progress;
                    PanIntent manual;
                    if(p.AutomationOn(0)&&p.AutomationOn(1))manual=default;
                    else if(p.AutomationOn(0))manual=s.CompactedSediment<=.045f?new PanIntent{Wash=1}:default;
                    else manual=new PanIntent{Work=scoopTime%8<3?1:0,Wash=scoopTime%8>=3?1:0};
                    game.TickActions(manual,.05f);scoopTime+=.05f;total+=.05f;
                    if(s.Ready)break;
                }
                frame++;yield return null;
                if(!game.Simulation.Ready)continue;
                if(!firstReady) {firstReady=true;yield return new WaitForSeconds(.4f);yield return Capture("02-first-gold.png");}
                int before=game.Progress.Gold, amount=game.Simulation.GoldValue;
                game.Collect();
                Check(game.Progress.Gold==before+amount,"Pan "+game.Progress.CollectedPans+" credits its exact gold once");
                int once=game.Progress.Gold;game.Collect();Check(game.Progress.Gold==once,"Fresh load cannot be collected twice");
                scoopTime=0;
                int tool=game.Progress.RecommendedUpgrade, assist=game.Progress.NextAutomation;
                if(tool>=0 && game.Progress.CanBuy(tool))
                {
                    game.Buy(tool);purchases++;
                    if(!firstUpgrade) {firstUpgrade=true;yield return Capture("03-first-improvement.png");}
                }
                else if(tool<0 && assist>=0 && game.Progress.AutomationAvailable(assist) && game.Progress.Gold>=game.Progress.AutomationCost(assist))
                {
                    game.BuyAutomation(assist);purchases++;
                    game.GetComponent<CreekUI>().ShowAssistance();
                    yield return new WaitForSeconds(.4f);
                    Check(game.diorama.Camp.WorkVisible,"Work helper is visible after purchase");
                    if(assist>=1)Check(game.diorama.Camp.WashVisible,"Water header appears with Auto Wash");
                    if(assist==2)Check(game.diorama.Camp.CollectVisible,"Catch tray appears with Auto Collect");
                    yield return Capture("0"+(4+assist)+"-assistance-"+(assist+1)+".png");
                }
            }
            Check(game.Progress.FullyAssisted&&purchases==12,"Full earned chapter path buys nine tool levels and three ordered helpers");
            int completed=game.Progress.CollectedPans;
            for(int i=0;i<1600&&game.Progress.CollectedPans<completed+3;i++)
            {
                for(int k=0;k<10;k++)game.TickActions(default,.05f);
                yield return null;
            }
            Check(game.Progress.CollectedPans>=completed+3,"Fully assisted pan completes three more loads without manual input");
            game.ToggleAutomation(0);game.ToggleAutomation(1);game.ToggleAutomation(2);
            game.TickActions(new PanIntent{Work=1},.05f);
            Check(game.Intent.Work==1&&game.Intent.Wash==0,"Manual Work survives all automation purchases");
            game.TickActions(new PanIntent{Wash=1},.05f);
            Check(game.Intent.Wash==1&&game.Intent.Work==0,"Manual Wash survives all automation purchases");
            string saved=SaveStore.Encode(game.Progress);
            Check(game.ReloadSaveForVerification()&&SaveStore.Encode(game.Progress)==saved,"Runtime save/load restores earned progression and helper toggles");
            Check(game.diorama.Camp.VisibleMilestones==6&&game.diorama.sluiceFlow.gameObject.activeSelf,"Loaded save restores all six camp milestones and sluice flow");
            game.ToggleAutomation(0);game.ToggleAutomation(1);
            for(int i=0;i<60;i++)game.TickActions(default,.05f);
            yield return Capture("07-complete-camp.png");
            for(int i=0;i<800&&!game.Simulation.Ready;i++)
            {
                for(int k=0;k<10&&!game.Simulation.Ready;k++)game.TickActions(default,.05f);
                yield return null;
            }
            Check(game.Simulation.Ready,"Loaded fully upgraded pan still reaches a clear gold reveal");
            yield return new WaitForSeconds(.5f);
            yield return Capture("08-assisted-gold.png");
            Check(game.ResetSave()&&game.Progress.Gold==0&&game.Progress.LifetimeGold==0&&!game.Progress.HasAutomation(0),"Pause reset clears progression");
            Check(game.diorama.Camp.VisibleMilestones==0&&!game.diorama.sluiceFlow.gameObject.activeSelf,"Reset hides every added milestone and stops the sluice");
            Check(game.ReloadSaveForVerification()&&game.Progress.Gold==0,"Reset stays reset after loading again");
            game.ShowHelp=true;yield return Capture("09-field-guide.png");
            report.AppendLine("Simulated processing seconds: "+total.ToString("F1")+"; accelerated through real game commands, not wall-clock playtime.");
            report.AppendLine("Save isolation: "+game.SavePath);
            File.WriteAllText(Path.Combine(folder,"runtime-chapter-checks.txt"),report.ToString());
            Application.Quit(failures==0?0:2);
        }
        IEnumerator Capture(string name)
        {
            game.pan.Render(game.Simulation,game.Intent,game.Progress,.05f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return null;
        }
    }
}
