using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek
{
    // A handful of additions around the existing saved camp; the creek and pan are never rebuilt.
    public sealed class CampProgressView
    {
        readonly Transform root, firstPan, toolRack, workHelper, washHelper, catchTray, screenBasket;
        readonly Transform crank, crankWeight, pumpRod, spout, washGauge, trayGold, catchLever;
        readonly Transform[] toolMarks=new Transform[3];
        readonly Material teal, tealLight, brass, timber, timberEnd, canvas, water, gold, joinery, iron;
        readonly Vector3 spoutScale, pumpRodRest;
        readonly Quaternion catchLeverRest;
        float crankDrive,waterDrive;
        public int VisibleMilestones {get;private set;}
        public bool WorkVisible => workHelper.gameObject.activeSelf;
        public bool WashVisible => washHelper.gameObject.activeSelf;
        public bool CollectVisible => catchTray.gameObject.activeSelf;

        public CampProgressView(Transform parent)
        {
            root=Group("Camp improvements",parent,Vector3.zero);
            var shader=Shader.Find("Creek/Matte");
            teal=Geometry.Mat("Camp enamel","#356F6C",shader);
            tealLight=Geometry.Mat("Camp enamel highlight","#5B9187",shader);
            brass=Geometry.Mat("Camp brass","#CEA563",shader);
            timber=Geometry.Mat("Camp timber","#9A7857",shader);
            timberEnd=Geometry.Mat("Camp end grain","#C39B70",shader);
            joinery=Geometry.Mat("Camp joints","#685745",shader);
            iron=Geometry.Mat("Camp blue iron","#405D62",shader);
            canvas=Geometry.Mat("Camp canvas","#E6AD57",shader);
            water=Geometry.Mat("Helper water","#63B2AB",Shader.Find("Creek/Water"));
            gold=Geometry.Mat("Catch tray gold","#F4C764",shader);
            firstPan=Group("First pan / tidy supply crate",root,new Vector3(-4.4f,.02f,3.65f));
            Shape("Small cedar crate",firstPan,PrimitiveType.Cube,Vector3.up*.22f,new Vector3(.95f,.44f,.75f),timber);
            for(int i=0;i<3;i++)Shape("Brass crate band",firstPan,PrimitiveType.Cube,new Vector3(-.38f+i*.38f,.25f,-.39f),new Vector3(.06f,.49f,.035f),brass);
            Shape("Folded marigold cloth",firstPan,PrimitiveType.Cube,new Vector3(.08f,.48f,.05f),new Vector3(.75f,.08f,.6f),canvas);
            for(int i=0;i<2;i++)Shape("Crate slat seam",firstPan,PrimitiveType.Cube,new Vector3(0,.15f+i*.14f,-.405f),new Vector3(.92f,.018f,.014f),joinery);
            Shape("Canvas folded edge",firstPan,PrimitiveType.Cube,new Vector3(.08f,.526f,-.21f),new Vector3(.71f,.014f,.035f),brass);
            toolRack=Group("Richer scoop / organized tools",root,new Vector3(-3.0f,.02f,3.6f));
            Shape("Tool rest",toolRack,PrimitiveType.Cube,new Vector3(0,.42f,0),new Vector3(1.1f,.10f,.5f),timber);
            for(int i=0;i<2;i++)Shape("Rack leg",toolRack,PrimitiveType.Cube,new Vector3(i==0?-.42f:.42f,.20f,0),new Vector3(.10f,.4f,.4f),timber);
            for(int i=0;i<3;i++)
            {
                toolMarks[i]=Group("Scoop tool "+(i+1),toolRack,new Vector3(-.36f+i*.36f,.48f,0));
                Geometry.Beam("Scoop handle",toolMarks[i],Vector3.zero,new Vector3(0,.65f,.12f),.06f,timber);
                Shape("Enamel scoop",toolMarks[i],PrimitiveType.Sphere,new Vector3(0,.14f,-.1f),new Vector3(.29f,.12f,.38f),teal);
            }
            workHelper=Group("Auto Work / camp crank unit",root,new Vector3(2.10f,.10f,-.72f));
            for(int i=0;i<2;i++)
            {
                float z=i==0?-.38f:.38f;
                Shape("Cedar sledge rail",workHelper,PrimitiveType.Cube,new Vector3(0,.10f,z),new Vector3(1.16f,.18f,.14f),timber);
                Shape("Pale rail end",workHelper,PrimitiveType.Cube,new Vector3(-.59f,.10f,z),new Vector3(.035f,.18f,.135f),timberEnd);
            }
            Shape("Enamel gear barrel",workHelper,PrimitiveType.Cylinder,new Vector3(.05f,.56f,0),new Vector3(.52f,.42f,.52f),teal,new Vector3(0,0,90));
            Shape("Gear barrel side cap",workHelper,PrimitiveType.Cylinder,new Vector3(-.40f,.56f,0),new Vector3(.39f,.035f,.39f),tealLight,new Vector3(0,0,90));
            Shape("Forged gearbox foot",workHelper,PrimitiveType.Cube,new Vector3(.05f,.25f,0),new Vector3(.55f,.18f,.57f),iron);
            for(int i=0;i<2;i++)Shape("Barrel hoop",workHelper,PrimitiveType.Cylinder,new Vector3(-.18f+i*.45f,.56f,0),new Vector3(.555f,.028f,.555f),brass,new Vector3(0,0,90));
            crank=Group("Turning flywheel",workHelper,new Vector3(-.50f,.62f,0));
            var flywheel=Geometry.MeshObject("Open brass flywheel",crank,Geometry.Lathe("Camp flywheel ring",new[]{new Vector2(.43f,-.035f),new Vector2(.50f,-.035f),new Vector2(.50f,.035f),new Vector2(.43f,.035f)},28),brass,Vector3.zero).transform;
            flywheel.localEulerAngles=new Vector3(0,0,90);
            Shape("Flywheel hub",crank,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.17f,.07f,.17f),iron,new Vector3(0,0,90));
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3, y=Mathf.Sin(a)*.23f,z=Mathf.Cos(a)*.23f;
                Geometry.Beam("Flywheel spoke",crank,Vector3.zero,new Vector3(0,y,z),.035f,brass);
            }
            crankWeight=Shape("Wooden crank handle",crank,PrimitiveType.Cylinder,new Vector3(-.12f,.34f,.18f),new Vector3(.09f,.16f,.09f),timber,new Vector3(0,0,90));
            pumpRod=Geometry.Beam("Iron pan linkage",workHelper,new Vector3(-.48f,.65f,.02f),new Vector3(-.98f,.84f,-.18f),.055f,iron);
            pumpRodRest=pumpRod.localPosition;
            washHelper=Group("Auto Wash / camp water header",root,new Vector3(2.48f,.10f,.58f));
            for(int i=0;i<2;i++)
            {
                float z=i==0?-.29f:.29f;
                Geometry.Beam("Splayed cedar header leg",washHelper,new Vector3(-.40f,0,z),new Vector3(-.22f,.82f,z),.075f,timber);
                Geometry.Beam("Splayed cedar header leg",washHelper,new Vector3(.40f,0,z),new Vector3(.22f,.82f,z),.075f,timber);
                Geometry.Beam("Header trestle tie",washHelper,new Vector3(-.36f,.28f,z),new Vector3(.36f,.28f,z),.055f,joinery);
            }
            Shape("Enamel header tank",washHelper,PrimitiveType.Cylinder,new Vector3(0,.93f,0),new Vector3(.60f,.34f,.60f),teal);
            Shape("Rounded header shoulder",washHelper,PrimitiveType.Sphere,new Vector3(0,1.20f,0),new Vector3(.60f,.17f,.60f),tealLight);
            Shape("Tank lid",washHelper,PrimitiveType.Cylinder,new Vector3(0,1.34f,0),new Vector3(.26f,.055f,.26f),iron);
            for(int i=0;i<2;i++)Shape("Tank retaining hoop",washHelper,PrimitiveType.Cylinder,new Vector3(0,.72f+i*.44f,0),new Vector3(.635f,.027f,.635f),brass);
            washGauge=Shape("Floating water gauge",washHelper,PrimitiveType.Sphere,new Vector3(.34f,1.20f,-.46f),new Vector3(.10f,.10f,.045f),brass);
            Geometry.Beam("Bent iron water neck",washHelper,new Vector3(-.30f,1.03f,-.02f),new Vector3(-.70f,.98f,-.12f),.10f,iron);
            Geometry.Beam("Bent iron water neck",washHelper,new Vector3(-.70f,.98f,-.12f),new Vector3(-.84f,.74f,-.19f),.10f,iron);
            spout=Geometry.Beam("Visible assisted water",washHelper,new Vector3(-.84f,.72f,-.19f),new Vector3(-1.02f,.45f,-.28f),.075f,water);
            spout.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            spoutScale=spout.localScale;
            catchTray=Group("Auto Collect / assay catch bench",root,new Vector3(2.55f,.10f,-2.12f));
            Shape("Cedar assay bench",catchTray,PrimitiveType.Cube,new Vector3(0,.36f,0),new Vector3(1.22f,.13f,.76f),timber);
            for(int i=0;i<2;i++)
            {
                float x=i==0?-.47f:.47f;
                Geometry.Beam("Splayed assay leg",catchTray,new Vector3(x+(i==0?-.08f:.08f),0,-.28f),new Vector3(x,.34f,-.25f),.065f,joinery);
                Geometry.Beam("Splayed assay leg",catchTray,new Vector3(x+(i==0?-.08f:.08f),0,.28f),new Vector3(x,.34f,.25f),.065f,joinery);
            }
            Shape("Recessed enamel catch",catchTray,PrimitiveType.Cube,new Vector3(0,.44f,0),new Vector3(.84f,.075f,.47f),teal);
            for(int i=0;i<2;i++)
            {
                Shape("Catch side rail",catchTray,PrimitiveType.Cube,new Vector3(i==0?-.44f:.44f,.50f,0),new Vector3(.04f,.10f,.54f),brass);
                Shape("Catch end rail",catchTray,PrimitiveType.Cube,new Vector3(0,.50f,i==0?-.27f:.27f),new Vector3(.88f,.10f,.04f),brass);
            }
            catchLever=Geometry.Beam("Catch release lever",catchTray,new Vector3(.56f,.45f,.10f),new Vector3(.78f,.72f,.10f),.045f,iron);
            catchLeverRest=catchLever.localRotation;
            Shape("Wooden lever knob",catchLever,PrimitiveType.Sphere,Vector3.up,new Vector3(.12f,.12f,.12f),timberEnd);
            trayGold=Group("A day's little findings",catchTray,new Vector3(0,.50f,0));
            for(int i=0;i<7;i++)Shape("Caught flake",trayGold,PrimitiveType.Sphere,new Vector3(Mathf.Sin(i*2.3f)*.25f,0,Mathf.Cos(i*3.1f)*.14f),new Vector3(.09f,.035f,.075f),gold);
            screenBasket=Group("Sluice / cedar classifier",root,new Vector3(3.58f,.08f,.42f));
            Shape("Classifier iron frame",screenBasket,PrimitiveType.Cube,new Vector3(0,.14f,0),new Vector3(1.02f,.16f,.74f),iron);
            Shape("Classifier enamel well",screenBasket,PrimitiveType.Cube,new Vector3(0,.24f,0),new Vector3(.88f,.12f,.60f),teal);
            for(int i=0;i<6;i++)Shape("Classifier brass screen",screenBasket,PrimitiveType.Cube,new Vector3(-.36f+i*.145f,.32f,0),new Vector3(.035f,.025f,.58f),brass);
            for(int i=0;i<2;i++)Shape("Classifier cedar grip",screenBasket,PrimitiveType.Cube,new Vector3(i==0?-.55f:.55f,.25f,0),new Vector3(.18f,.12f,.32f),timber);
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var soft=new MaterialPropertyBlock();soft.SetFloat("_ReceiveShadows",.30f);r.SetPropertyBlock(soft);
                if(r.name.Contains("flake")||r.name.Contains("seam")||r.name.Contains("crossbar")||r.name.Contains("spout"))r.shadowCastingMode=ShadowCastingMode.Off;
            }
        }
        static Transform Group(string name,Transform parent,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        static Transform Shape(string name,Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material mat,Vector3 rotation=default)
        { return Geometry.Shape(name,parent,type,position,scale,mat,rotation); }
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
            crankDrive=Mathf.MoveTowards(crankDrive,active&&intent.Work>0?1:0,dt*6);
            waterDrive=Mathf.MoveTowards(waterDrive,active&&intent.Wash>0?1:0,dt*8);
            if(workHelper.gameObject.activeSelf)crank.Rotate(Vector3.right,dt*90*crankDrive,Space.Self);
            if(workHelper.gameObject.activeSelf)
            {
                float pulse=Mathf.Sin(Time.time*5.6f)*crankDrive;
                pumpRod.localPosition=pumpRodRest+new Vector3(0,pulse*.018f,0);
                crankWeight.localScale=new Vector3(.09f,.16f+.012f*crankDrive,.09f);
            }
            spout.gameObject.SetActive(washHelper.gameObject.activeSelf && waterDrive>.01f);
            spout.localScale=spoutScale*Mathf.Lerp(.75f,1,waterDrive);
            if(washHelper.gameObject.activeSelf)washGauge.localPosition=new Vector3(.34f,1.20f+Mathf.Sin(Time.time*3.2f)*.015f*waterDrive,-.46f);
            if(catchTray.gameObject.activeSelf)catchLever.localRotation=catchLeverRest*Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.2f)*1.5f);
        }
        public void Dispose()
        {
            foreach(var m in new[]{teal,tealLight,brass,timber,timberEnd,canvas,water,gold,joinery,iron})if(m)Object.Destroy(m);
        }
    }
}
