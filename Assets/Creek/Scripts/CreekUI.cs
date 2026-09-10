using UnityEngine;

namespace RiffleCreek
{
    public sealed class CreekUI : MonoBehaviour
    {
        CreekGame game;
        int shopTab; bool resetArmed;
        Texture2D round, circle;
        GUIStyle body, small, title, heading, large, button;

        static readonly string[] upgradeInfo={"Faster Work + Wash. Brass inlays.","Quicker settling. Raised front riffles.","+3 gold / load. A better camp tool."};
        readonly Color ink=new Color(.16f,.24f,.23f), muted=new Color(.39f,.44f,.39f), cream=new Color(.97f,.94f,.85f), teal=new Color(.20f,.43f,.42f), gold=new Color(.92f,.67f,.27f);
        float Scale => Mathf.Min(Screen.width/1600f,Screen.height/900f);
        Vector2 Offset => new Vector2((Screen.width-1600*Scale)/2,(Screen.height-900*Scale)/2);
        Vector2 Pointer => (new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)-Offset)/Scale;
        public void Initialize(CreekGame owner) {game=owner;}
        public void ShowAssistance() {shopTab=1;}
        public bool BlocksPointer()
        {
            var p=Pointer;
            return game.ShowHelp || game.Paused || p.y<136 || p.y>784 || (p.x>1250&&p.y>180&&p.y<735);
        }
        void Setup()
        {
            if(round!=null)return;
            round=Rounded(32,9);circle=Rounded(64,32);
            Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            body=new GUIStyle {font=font,fontSize=17,normal={textColor=ink},wordWrap=true};
            small=new GUIStyle(body) {fontSize=13};
            title=new GUIStyle(body) {fontSize=34,fontStyle=FontStyle.Bold};
            heading=new GUIStyle(body) {fontSize=20,fontStyle=FontStyle.Bold};
            large=new GUIStyle(body) {fontSize=30,fontStyle=FontStyle.Bold};
            button=new GUIStyle(body) {alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold,fontSize=17};
        }
        static Texture2D Rounded(int size,float radius)
        {
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);tex.name="Soft UI corner";tex.filterMode=FilterMode.Bilinear;
            var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(radius-x-.5f,x+.5f-(size-radius)),dy=Mathf.Max(radius-y-.5f,y+.5f-(size-radius));
                float distance=new Vector2(Mathf.Max(0,dx),Mathf.Max(0,dy)).magnitude;
                colors[y*size+x]=new Color(1,1,1,Mathf.Clamp01(radius-distance));
            }
            tex.SetPixels(colors);tex.Apply();return tex;
        }
        void Box(Rect r,Color color,bool shadow=false)
        {
            float radius=Mathf.Min(9,r.height*.5f);
            if(shadow)GUI.DrawTexture(new Rect(r.x,r.y+4,r.width,r.height),Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,new Color(.08f,.16f,.14f,.13f),0,radius);
            GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,radius);
        }
        void Label(float x,float y,float w,float h,string text,GUIStyle style,Color? color=null)
        {
            Color old=style.normal.textColor;style.normal.textColor=color??ink;GUI.Label(new Rect(x,y,w,h),text,style);style.normal.textColor=old;
        }
        void Bar(float x,float y,float w,float value,Color c,float h=5)
        {
            Box(new Rect(x,y,w,h),new Color(.20f,.30f,.26f,.13f));
            if(value>.001f)Box(new Rect(x,y,w*Mathf.Clamp01(value),h),c);
        }
        bool Button(Rect r,string text,bool enabled=true,bool primary=false,bool inOverlay=false)
        {
            enabled=enabled && (inOverlay || (!game.ShowHelp && !game.Paused));
            bool hover=r.Contains(Pointer)&&enabled;
            Color bg=enabled?(primary?teal:new Color(.88f,.85f,.74f)):new Color(.88f,.87f,.80f);
            if(hover)bg=Color.Lerp(bg,Color.white,.12f);
            Box(r,bg);
            button.normal.textColor=enabled?(primary?cream:ink):muted;
            GUI.enabled=enabled;
            bool clicked=GUI.Button(r,text,button);
            GUI.enabled=true;
            return clicked&&enabled;
        }
        void OnGUI()
        {
            if(game==null)return;Setup();
            GUI.matrix=Matrix4x4.TRS(new Vector3(Offset.x,Offset.y,0),Quaternion.identity,Vector3.one*Scale);
            var sim=game.Simulation;var p=game.Progress;
            Box(new Rect(32,28,349,94),cream,true);
            GUI.color=gold;GUI.DrawTexture(new Rect(49,48,51,51),circle);GUI.color=Color.white;
            Label(63,51,35,40,"r",title,cream);
            Label(116,39,242,41,"R I F F L E",title);
            Label(118,83,235,22,"ALDER CREEK  /  FIRST LIGHT",small,muted);
            Box(new Rect(535,28,430,94),cream,true);
            Label(555,43,215,20,"SCOOP "+(p.CollectedPans+1).ToString("00"),small,muted);
            Label(555,65,310,30,game.Stage,heading);
            Label(882,65,65,30,Mathf.RoundToInt(sim.Clarity*100)+"%",heading,teal);
            Bar(555,105,388,sim.Clarity,teal,4);
            Box(new Rect(1258,28,310,94),cream,true);
            Label(1278,42,190,23,"YOUR GOLD",small,muted);
            Label(1277,64,195,40,p.Gold.ToString("N0"),large);
            Label(1461,76,90,23,"gold",body,muted);

            Box(new Rect(1258,194,310,378),cream,true);
            if(Button(new Rect(1278,209,126,34),"Tools",true,shopTab==0))shopTab=0;
            if(Button(new Rect(1413,209,134,34),"Assistance",true,shopTab==1))shopTab=1;
            Label(1279,252,270,24,shopTab==0?"Good tools. A steadier rhythm.":"Small helpers. Still your pan.",small,muted);
            for(int i=0;i<3;i++)
            {
                float y=287+i*92;
                Box(new Rect(1270,y-5,286,83),new Color(.92f,.90f,.82f));
                if(shopTab==0)
                {
                    Label(1279,y,180,25,Progression.UpgradeNames[i],body);
                    for(int k=0;k<3;k++)Box(new Rect(1279+k*14,y+30,8,5),k<p.Level(i)?teal:new Color(.76f,.76f,.66f));
                    Label(1279,y+44,266,38,upgradeInfo[i],small,muted);
                    bool max=p.Level(i)>=Progression.MaxLevel;
                    if(Button(new Rect(1453,y-2,94,36),max?"MAX":p.Cost(i)+" g",p.CanBuy(i)))
                    {
                        game.Buy(i);
                        if(p.RecommendedUpgrade<0 && p.NextAutomation>=0)shopTab=1;
                    }
                }
                else
                {
                    Label(1279,y,175,25,Progression.AssistNames[i],body);
                    bool owned=p.HasAutomation(i), available=p.AutomationAvailable(i);
                    if(owned)Label(1279,y+61,268,21,p.AutomationOn(i)?"ENABLED / your hands take priority":"PAUSED / tap OFF to enable",small,p.AutomationOn(i)?teal:muted);
                    string[] help={"Prepares dirt. You wash and collect.","Alternates with Work. You collect.","Collects revealed gold; loads again."};
                    Label(1279,y+28,268,35,help[i],small,muted);
                    if(!owned)Label(1279,y+61,268,21,available?"Ready to build":p.AutomationRequirement(i),small,available?teal:muted);
                    if(Button(new Rect(1453,y-2,94,33),owned?(p.AutomationOn(i)?"ON":"OFF"):p.AutomationCost(i)+" g",
                        owned || available&&p.Gold>=p.AutomationCost(i),owned&&p.AutomationOn(i)))
                    { if(owned)game.ToggleAutomation(i);else game.BuyAutomation(i); }
                }
            }
            Box(new Rect(1258,586,310,146),cream,true);
            Label(1279,601,262,22,p.FullyAssisted?"A SMALL PROSPECTING CAMP":"NEXT LITTLE IMPROVEMENT",small,muted);
            Label(1279,628,267,42,p.NextGoal,body);
            Bar(1279,674,267,p.GoalProgress,gold,5);
            Label(1279,691,267,32,p.SluiceActive?"Sluice flowing / gentler prepared loads":Mathf.Min(p.LifetimeGold,40)+" / 40 lifetime gold wakes the sluice",small,muted);
            if(game.GoldGainTime>0)
            {
                Box(new Rect(1258,132,310,36),teal);
                Label(1279,138,275,29,"+"+game.LastGoldGain+" gold collected",heading,cream);
            }
            if(game.SaveMessage!=null)Label(1258,748,310,36,"Save needs attention — Esc",small,teal);

            // Material feedback stays alongside the pan, outside its working area.
            Box(new Rect(32,566,222,164),new Color(cream.r,cream.g,cream.b,.96f),true);
            Label(49,582,188,22,"IN THE PAN",small,muted);
            Meter(49,614,"Washable",sim.LooseSediment,teal);
            Meter(49,647,"Layered",sim.Stratification,teal);
            Meter(49,680,"Settled",sim.Capture,gold);
            if(sim.Loaded)Label(34,745,350,24,sim.Stones+" stones  /  "+Mathf.RoundToInt(sim.Sediment*100)+"% silt  /  "+Mathf.RoundToInt(sim.BlackSand*100)+"% black sand",small,cream);
            Box(new Rect(535,126,430,26),teal);
            GUI.color=game.Assistance.Activity.StartsWith("Waiting")?new Color(.68f,.78f,.70f):gold;
            GUI.DrawTexture(new Rect(546,135,7,7),circle);GUI.color=Color.white;
            Label(562,131,390,21,game.Assistance.Activity,small,cream);
            WorldTag(new Vector3(-5.3f,.8f,-4.4f),sim.Ready?"GOLD READY  /  [E] COLLECT":sim.Sediment<.3f?"BLACK SAND  /  GOLD SETTLING":"FRONT  /  WORKING RIFFLES",sim.Ready||game.Intent.Working);
            if(game.ToastTime>0)
            {
                Box(new Rect(415,160,700,52),teal,true);
                Label(432,168,666,43,game.Toast,body,cream);
            }
            Box(new Rect(32,792,1536,80),cream,true);
            Label(52,806,180,24,sim.Ready?"GOLD REVEALED":!sim.Loaded?"BACK TO THE CREEK":"HOLD TO PAN",small,teal);
            Label(52,833,844,28,game.Hint,body);
            Label(874,808,300,23,"HOLD LMB  work    Esc  pause",small,muted);
            Label(874,836,295,23,"HOLD SPACE / RMB  wash     E  collect",small,muted);
            if(Button(new Rect(1194,810,230,45),sim.Ready?"Collect gold  [E]":sim.Loaded?"Working scoop "+(p.CollectedPans+1):"Scoop the creek  [E]",!sim.Loaded||sim.Ready,true)) {if(sim.Ready)game.Collect();else game.Scoop();}
            if(Button(new Rect(1437,810,48,45),"?"))game.ShowHelp=true;
            if(Button(new Rect(1495,810,54,45),game.Sound.Muted?"Off":"Snd"))game.Sound.ToggleMute();
            if(game.ShowHelp)Help();
            if(!game.Paused)resetArmed=false;
            if(game.Paused)
            {
                Box(new Rect(0,0,1600,900),new Color(.1f,.18f,.17f,.6f));
                Box(new Rect(520,260,560,390),cream,true);
                Label(551,286,500,49,"A moment by the creek",title);
                Label(552,344,493,60,game.SaveMessage ?? "Progress saves after every collection and purchase.\nYour current scoop starts fresh when you return.",body);
                Label(552,415,490,46,resetArmed?"Erase all Alder Creek gold, tools and assistance?":"All helpers pause here. No offline income.",body,resetArmed?teal:muted);
                if(Button(new Rect(552,481,496,44),"Return to the creek  [Esc]",true,true,true)) {resetArmed=false;game.Resume();}
                if(Button(new Rect(552,539,240,43),game.Sound.Muted?"Sound: off":"Sound: on",true,false,true))game.Sound.ToggleMute();
                if(Button(new Rect(806,539,242,43),resetArmed?"Erase and start fresh":"Reset local save",true,false,true))
                {
                    if(resetArmed) { if(game.ResetSave()) {resetArmed=false;game.Resume();} }
                    else resetArmed=true;
                }
                Label(552,600,490,25,resetArmed?"Return to cancel. This cannot be undone.":"H / field guide explains the Work and Wash rhythm.",small,muted);
            }
            GUI.matrix=Matrix4x4.identity;
        }
        void Meter(float x,float y,string label,float value,Color color) {Label(x,y,80,22,label,small,muted);Bar(x+78,y+8,105,value,color,6);}
        void WorldTag(Vector3 world,string text,bool active)
        {
            var s=game.sceneCamera.WorldToScreenPoint(world);var p=(new Vector2(s.x,Screen.height-s.y)-Offset)/Scale;
            Box(new Rect(p.x-113,p.y-13,226,27),active?teal:new Color(.15f,.28f,.27f,.81f));
            Label(p.x-99,p.y-7,210,20,text,small,cream);
        }
        void Help()
        {
            Box(new Rect(0,0,1600,900),new Color(.1f,.18f,.17f,.65f));
            Box(new Rect(410,173,780,540),cream,true);
            Label(447,201,700,24,"THE FIELD GUIDE",small,teal);
            Label(447,238,700,54,"A little work. A little gold.",title);
            Label(447,307,690,70,"01   Hold left mouse to work\nKeep the cursor still. Rock the solids loose, move stones toward the front riffles, and settle the heavy material. Dirt stays until you wash.",body);
            Label(447,391,690,70,"02   Hold Space or right mouse to wash\nPour loose dirt over the front edge. When dirt stops clearing, use left mouse again. Water cannot shake compacted gravel loose.",body);
            Label(447,475,690,70,"03   Press E to collect\nBright gold, small glints, and a quiet cue mark the finish. E collects the gold and prepares your next scoop.",body);
            Label(447,566,690,44,"Work, wash, and repeat as needed. No precise timing or mouse movement.\nHelpers unlock in the camp panel. Toggle them there; manual holds always take priority.",body,muted);
            if(Button(new Rect(447,640,700,43),"Back to the creek   [H / Esc]",true,true,true))game.ShowHelp=false;
        }
    }
}
