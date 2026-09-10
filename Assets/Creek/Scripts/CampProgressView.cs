using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek
{
    // A handful of additions around the existing saved camp; the creek and pan are never rebuilt.
    public sealed class CampProgressView
    {
        readonly Transform root, firstPan, toolRack, workHelper, washHelper, catchTray, screenBasket;
        readonly Transform crank, spout, trayGold;
        readonly Transform[] toolMarks=new Transform[3];
        readonly Material teal, brass, timber, canvas, water, gold;
        public int VisibleMilestones {get;private set;}
        public bool WorkVisible => workHelper.gameObject.activeSelf;
        public bool WashVisible => washHelper.gameObject.activeSelf;
        public bool CollectVisible => catchTray.gameObject.activeSelf;

        public CampProgressView(Transform parent)
        {
            root=Group("Camp improvements",parent,Vector3.zero);
            var shader=Shader.Find("Creek/Matte");
            teal=Geometry.Mat("Camp enamel","#356F6C",shader);
            brass=Geometry.Mat("Camp brass","#CEA563",shader);
            timber=Geometry.Mat("Camp timber","#926B46",shader);
            canvas=Geometry.Mat("Camp canvas","#E6AD57",shader);
            water=Geometry.Mat("Helper water","#63B2AB",Shader.Find("Creek/Water"));
            gold=Geometry.Mat("Catch tray gold","#F4C764",shader);
            firstPan=Group("First pan / tidy supply crate",root,new Vector3(-4.4f,.02f,3.65f));
            Shape("Small cedar crate",firstPan,PrimitiveType.Cube,Vector3.up*.22f,new Vector3(.95f,.44f,.75f),timber);
            for(int i=0;i<3;i++)Shape("Brass crate band",firstPan,PrimitiveType.Cube,new Vector3(-.38f+i*.38f,.25f,-.39f),new Vector3(.06f,.49f,.035f),brass);
            Shape("Folded marigold cloth",firstPan,PrimitiveType.Cube,new Vector3(.08f,.48f,.05f),new Vector3(.75f,.08f,.6f),canvas);
            toolRack=Group("Richer scoop / organized tools",root,new Vector3(-3.0f,.02f,3.6f));
            Shape("Tool rest",toolRack,PrimitiveType.Cube,new Vector3(0,.42f,0),new Vector3(1.1f,.10f,.5f),timber);
            for(int i=0;i<2;i++)Shape("Rack leg",toolRack,PrimitiveType.Cube,new Vector3(i==0?-.42f:.42f,.20f,0),new Vector3(.10f,.4f,.4f),timber);
            for(int i=0;i<3;i++)
            {
                toolMarks[i]=Group("Scoop tool "+(i+1),toolRack,new Vector3(-.36f+i*.36f,.48f,0));
                Geometry.Beam("Scoop handle",toolMarks[i],Vector3.zero,new Vector3(0,.65f,.12f),.06f,brass);
                Shape("Enamel scoop",toolMarks[i],PrimitiveType.Sphere,new Vector3(0,.14f,-.1f),new Vector3(.29f,.12f,.38f),teal);
            }
            workHelper=Group("Auto Work / gentle crank",root,new Vector3(2.20f,.12f,-.7f));
            Shape("Helper base",workHelper,PrimitiveType.Cube,new Vector3(0,.13f,0),new Vector3(.82f,.26f,.95f),timber);
            Shape("Enamel housing",workHelper,PrimitiveType.Cube,new Vector3(0,.52f,0),new Vector3(.46f,.65f,.53f),teal);
            crank=Group("Slow brass crank",workHelper,new Vector3(-.31f,.72f,0));
            Shape("Crank disk",crank,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.56f,.045f,.56f),brass,new Vector3(0,0,90));
            Shape("Crank grip",crank,PrimitiveType.Cylinder,new Vector3(-.1f,.18f,0),new Vector3(.09f,.14f,.09f),timber,new Vector3(0,0,90));
            Geometry.Beam("Gentle pan linkage",workHelper,new Vector3(-.4f,.8f,0),new Vector3(-.86f,.9f,-.15f),.075f,brass);
            washHelper=Group("Auto Wash / little water header",root,new Vector3(2.45f,.18f,.5f));
            Shape("Header legs",washHelper,PrimitiveType.Cube,new Vector3(0,.4f,0),new Vector3(.10f,.8f,.15f),timber);
            Shape("Jade header",washHelper,PrimitiveType.Cylinder,new Vector3(0,.95f,0),new Vector3(.65f,.35f,.65f),teal);
            Shape("Brass header lip",washHelper,PrimitiveType.Cylinder,new Vector3(0,1.31f,0),new Vector3(.69f,.025f,.69f),brass);
            Geometry.Beam("Water spout",washHelper,new Vector3(-.28f,1.03f,0),new Vector3(-.78f,.97f,-.12f),.12f,brass);
            spout=Geometry.Beam("Visible assisted water",washHelper,new Vector3(-.78f,.97f,-.12f),new Vector3(-1.02f,.57f,-.25f),.085f,water);
            spout.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            catchTray=Group("Auto Collect / small catch tray",root,new Vector3(2.65f,.13f,-2.15f));
            Shape("Catch table",catchTray,PrimitiveType.Cube,new Vector3(0,.30f,0),new Vector3(1.08f,.12f,.72f),timber);
            for(int i=0;i<2;i++)Shape("Catch table leg",catchTray,PrimitiveType.Cube,new Vector3(i==0?-.42f:.42f,.12f,0),new Vector3(.1f,.25f,.5f),timber);
            Shape("Brass catch tray",catchTray,PrimitiveType.Cube,new Vector3(0,.40f,0),new Vector3(.76f,.10f,.46f),brass);
            trayGold=Group("A day's little findings",catchTray,new Vector3(0,.49f,0));
            for(int i=0;i<7;i++)Shape("Caught flake",trayGold,PrimitiveType.Sphere,new Vector3(Mathf.Sin(i*2.3f)*.25f,0,Mathf.Cos(i*3.1f)*.14f),new Vector3(.09f,.035f,.075f),gold);
            screenBasket=Group("Sluice / pre-screen basket",root,new Vector3(3.65f,.08f,.40f));
            Shape("Gravel basket",screenBasket,PrimitiveType.Cube,new Vector3(0,.12f,0),new Vector3(.86f,.24f,.62f),teal);
            for(int i=0;i<5;i++)Shape("Screen crossbar",screenBasket,PrimitiveType.Cube,new Vector3(-.34f+i*.17f,.25f,0),new Vector3(.055f,.03f,.61f),brass);
        }
        static Transform Group(string name,Transform parent,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        static void Shape(string name,Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material mat,Vector3 rotation=default)
        { Geometry.Shape(name,parent,type,position,scale,mat,rotation); }
        public void Apply(Progression p)
        {
            firstPan.gameObject.SetActive(p.CollectedPans>0);
            toolRack.gameObject.SetActive(p.ScoopLevel>0);
            for(int i=0;i<3;i++)toolMarks[i].gameObject.SetActive(i<p.ScoopLevel);
            workHelper.gameObject.SetActive(p.HasAutomation(0));washHelper.gameObject.SetActive(p.HasAutomation(1));catchTray.gameObject.SetActive(p.HasAutomation(2));
            screenBasket.gameObject.SetActive(p.SluiceActive);
            VisibleMilestones=(p.CollectedPans>0?1:0)+(p.ScoopLevel>0?1:0)+(p.SluiceActive?1:0)+(p.HasAutomation(0)?1:0)+(p.HasAutomation(1)?1:0)+(p.HasAutomation(2)?1:0);
        }
        public void Animate(PanIntent intent,bool active,float dt)
        {
            if(workHelper.gameObject.activeSelf && active && intent.Work>0)crank.Rotate(Vector3.right,dt*90,Space.Self);
            spout.gameObject.SetActive(washHelper.gameObject.activeSelf && active && intent.Wash>0);
        }
        public void Dispose()
        {
            foreach(var m in new[]{teal,brass,timber,canvas,water,gold})if(m)Object.Destroy(m);
        }
    }
}
