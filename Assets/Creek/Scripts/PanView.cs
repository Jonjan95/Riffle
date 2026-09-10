using UnityEngine;

namespace RiffleCreek
{
    public sealed class PanView : MonoBehaviour
    {
        public Transform sedimentBed, blackBed, waterFilm, riffleAccent;
        public Mesh pebble, goldFlake;
        public Material[] grainMaterials;
        readonly Transform[] grains=new Transform[PanSimulation.GrainCount];
        readonly Transform[] glints=new Transform[6];
        Material cleanGold,glintMaterial,siltMaterial,riffleLight,badgeMaterial;
        Mesh concentrateMesh, siltMesh;
        readonly Transform[] riffleCrests=new Transform[3];
        float revealClock; bool wasRevealed;
        Mesh glintMesh;
        float finishGlow;
        readonly Transform[] toolBadges=new Transform[3];
        Vector3 anchor;
        Transform outflow;
        readonly Transform[] flowMarks=new Transform[10];
        Mesh outflowMesh;
        readonly Vector3[] flowVertices=new Vector3[26];
        float flowClock;
        public float ForwardTilt {get;private set;}
        public float VisibleFlow {get;private set;}
        public Vector3 OutflowMouth {get;private set;}
        public Vector3 OutflowEnd {get;private set;}
        public Vector3 Anchor => anchor;

        public void Initialize()
        {
            anchor=transform.position;
            badgeMaterial=Geometry.Mat("Pan level brass","#DCC08A",Shader.Find("Creek/Matte"));
            // Thin layers and tiny grains should not project harsh self-shadow wedges into the bowl.
            sedimentBed.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            blackBed.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var softShadows=new MaterialPropertyBlock();softShadows.SetFloat("_ReceiveShadows",.18f);
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))renderer.SetPropertyBlock(softShadows);
            for(int i=0;i<3;i++)
            {
                toolBadges[i]=Geometry.Shape("Pan improvement brass inlay",transform,PrimitiveType.Cube,
                    new Vector3(2.94f,.88f,-.22f+i*.22f),new Vector3(.15f,.03f,.11f),badgeMaterial);
                toolBadges[i].gameObject.SetActive(false);
            }
            siltMaterial=new Material(sedimentBed.GetComponent<Renderer>().sharedMaterial);
            sedimentBed.GetComponent<Renderer>().sharedMaterial=siltMaterial;
            // Small irregularities break the perfect circular patch without altering material positions.
            Mesh OrganicLayer(Transform layer,string name,float amount)
            {
                var mesh=Instantiate(layer.GetComponent<MeshFilter>().sharedMesh);mesh.name=name;
                var vertices=mesh.vertices;
                for(int i=0;i<vertices.Length;i++)
                {
                    var v=vertices[i];float angle=Mathf.Atan2(v.x,v.z);
                    float shape=1+amount*(Mathf.Sin(angle*5+.7f)*.6f+Mathf.Cos(angle*9)*.4f);
                    v.x*=shape;v.z*=shape;vertices[i]=v;
                }
                mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                layer.GetComponent<MeshFilter>().sharedMesh=mesh;return mesh;
            }
            concentrateMesh=OrganicLayer(blackBed,"Irregular black sand pocket",.09f);
            siltMesh=OrganicLayer(sedimentBed,"Soft scoop edge",.035f);
            riffleLight=Geometry.Mat("Riffle crest brass","#BAA366",Shader.Find("Creek/Matte"));riffleLight.color=new Color(.73f,.64f,.40f);riffleLight.SetFloat("_Sheen",.08f);
            for(int i=0;i<3;i++)
            {
                float r=2.05f+i*.18f,y=.17f+(r-1.94f)*.68f+.079f;
                riffleCrests[i]=Geometry.MeshObject("Upgraded riffle satin crest",riffleAccent,
                    Geometry.Lathe("Riffle crown",new[]{new Vector2(r+.025f,y),new Vector2(r-.005f,y)},48,-47+i*2,94-i*4),riffleLight,Vector3.zero).transform;
                riffleCrests[i].GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            cleanGold=new Material(grainMaterials[3]);cleanGold.SetFloat("_Sheen",.18f);cleanGold.SetFloat("_ReceiveShadows",0);
            var root=new GameObject("Material particles • one simulation").transform;root.SetParent(transform,false);
            for(int i=0;i<grains.Length;i++)
            {
                int kind=i<88?0:i<98?1:i<140?2:3;
                grains[i]=Geometry.MeshObject(((MaterialKind)kind).ToString(),root,kind==3&&goldFlake?goldFlake:pebble,kind==3?cleanGold:grainMaterials[kind],Vector3.zero).transform;
                grains[i].GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                grains[i].GetComponent<Renderer>().SetPropertyBlock(softShadows);
                grains[i].gameObject.SetActive(false);
            }
            glintMaterial=Geometry.Mat("Soft gold glint","#FFF2BF",Shader.Find("Creek/Glow"));
            glintMesh=Geometry.Make("Four-point glint",new[]{Vector3.zero,new Vector3(0,0,1),new Vector3(.17f,0,.17f),new Vector3(.7f,0,0),new Vector3(.17f,0,-.17f),new Vector3(0,0,-1),new Vector3(-.17f,0,-.17f),new Vector3(-.7f,0,0),new Vector3(-.17f,0,.17f)},new[]{0,1,2,0,2,3,0,3,4,0,4,5,0,5,6,0,6,7,0,7,8,0,8,1});
            for(int i=0;i<glints.Length;i++) {glints[i]=Geometry.MeshObject("Gold ready glint",root,glintMesh,glintMaterial,Vector3.zero).transform;glints[i].localScale=Vector3.zero;}
            int[] flowTriangles=new int[12*6];
            for(int i=0;i<12;i++) {int v=i*2,t=i*6;flowTriangles[t]=v;flowTriangles[t+1]=v+1;flowTriangles[t+2]=v+2;flowTriangles[t+3]=v+1;flowTriangles[t+4]=v+3;flowTriangles[t+5]=v+2;}
            outflowMesh=Geometry.Make("Continuous front pour",flowVertices,flowTriangles);outflowMesh.MarkDynamic();
            outflow=Geometry.MeshObject("Water over front lip",root,outflowMesh,waterFilm.GetComponent<Renderer>().sharedMaterial,Vector3.zero).transform;
            outflow.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            for(int i=0;i<flowMarks.Length;i++) {flowMarks[i]=Geometry.MeshObject("Water flow streak",root,glintMesh,glintMaterial,Vector3.zero).transform;flowMarks[i].localScale=Vector3.zero;}
        }

        public void Render(PanSimulation sim, PanIntent intent, Progression progress, float dt)
        {
            for(int i=0;i<3;i++)toolBadges[i].gameObject.SetActive(i<progress.PanLevel);
            for(int i=0;i<3;i++)riffleCrests[i].gameObject.SetActive(i<progress.RiffleLevel);
            if(sim.Ready&&!wasRevealed)revealClock=0;
            revealClock+=dt;wasRevealed=sim.Ready;
            float working=sim.Ready?0:sim.Agitation,pouring=sim.Ready?0:sim.PourAmount;
            float forward=Mathf.Max(working*7,pouring*15);
            float rock=Mathf.Sin(sim.WorkPhase)*working;
            // Local -Z is the visible bottom/front. Negative X rotation lowers that lip in world Y.
            var desired=Quaternion.Euler(-forward+rock*.95f,rock*.30f,Mathf.Sin(sim.WorkPhase*.8f)*working*1.35f);
            transform.localRotation=Quaternion.Slerp(transform.localRotation,desired,1-Mathf.Exp(-dt*9));
            transform.position=Vector3.Lerp(transform.position,anchor+new Vector3(rock*.032f,0,-working*.055f-pouring*.035f),1-Mathf.Exp(-dt*10));
            ForwardTilt=Vector3.Dot(transform.TransformPoint(new Vector3(0,.8f,-2.88f))-transform.TransformPoint(new Vector3(0,.8f,2.88f)),Vector3.up);
            sedimentBed.gameObject.SetActive(sim.Loaded&&!sim.Ready&&sim.Sediment>.01f);
            float bed=Mathf.Sqrt(sim.Sediment);
            siltMaterial.color=Color.Lerp(new Color(.48f,.35f,.24f),new Color(.61f,.47f,.32f),sim.Looseness*.7f+(1-sim.Sediment)*.3f);
            sedimentBed.localScale=new Vector3(bed,.055f+sim.Sediment*.21f,bed*(1-sim.FrontLoad*.22f));
            sedimentBed.localPosition=new Vector3(rock*.018f,.25f+Mathf.Sin(sim.WorkPhase*2)*working*.012f,-sim.FrontLoad*.55f);
            blackBed.gameObject.SetActive(sim.Loaded&&(sim.BlackSand>.025f||sim.Ready));
            blackBed.localScale=new Vector3(Mathf.Sqrt(sim.BlackSand)*Mathf.Lerp(.82f,.54f,sim.Concentration),.035f,Mathf.Sqrt(sim.BlackSand)*Mathf.Lerp(.82f,.25f,sim.Concentration));
            blackBed.localPosition=new Vector3(0,.225f,-sim.Concentration*1.50f);
            // Keep a small charcoal pocket under the revealed gold, even if the player keeps washing.
            if(sim.Ready) {blackBed.localScale=new Vector3(.47f,.035f,.19f);blackBed.localPosition=new Vector3(0,.235f,-1.58f);}
            finishGlow=Mathf.MoveTowards(finishGlow,sim.Ready?1:0,dt*3);
            cleanGold.color=Color.Lerp(new Color(.81f,.60f,.20f),new Color(1,.84f,.36f),Mathf.Max(sim.GoldExposure,finishGlow));
            waterFilm.gameObject.SetActive(sim.Loaded&&!sim.Ready&&pouring>.03f);
            waterFilm.localPosition=new Vector3(0,Mathf.Sin(Time.time*3)*.008f,0);
            DrawPour(sim,dt);
            riffleAccent.localScale=new Vector3(1,1+progress.RiffleLevel*.03f,1);
            for(int i=0;i<grains.Length;i++)
            {
                var g=sim.Grains[i];var t=grains[i];
                bool visible=sim.Loaded && g.Active && (!sim.Ready || g.Kind!=MaterialKind.Sediment);
                if(t.gameObject.activeSelf!=visible)t.gameObject.SetActive(visible);
                if(!visible)continue;
                bool gold=g.Kind==MaterialKind.Gold, heavy=g.Kind==MaterialKind.BlackSand;
                bool stone=g.Kind==MaterialKind.Stone;
                float layer=gold?.275f:heavy?.26f:stone?.22f+g.Size*.4f+sim.Sediment*.10f:.31f+sim.Sediment*.14f;
                float ramp=Mathf.Clamp(g.Position.magnitude-1.94f,0,.94f)*.68f;
                float falling=stone?g.Exit*g.Exit*6:g.Exit>.22f?(g.Exit-.22f)*(g.Exit-.22f)*7:0;
                t.localPosition=new Vector3(g.Position.x,layer+ramp-falling,g.Position.y);
                float reveal=gold?Mathf.Clamp01((.55f-sim.Sediment)*3.5f)*Mathf.Lerp(.65f,1,sim.GoldExposure):1;
                float size=g.Size*reveal*(g.Exit>0?Mathf.Clamp01(1-g.Exit/(stone?1.1f:.65f)):1);
                // Broad little flakes, fine charcoal grains, round stones, and shallow crumbly clods.
                float width=heavy?.62f:gold?1.12f:stone?1:.82f;
                float thickness=stone?.9f:gold?(g.Size>.25f?.45f:.20f):heavy?.20f:.30f;
                t.localScale=new Vector3(size*width,size*thickness,size*width*.85f);
                t.localRotation=Quaternion.Euler(g.Kind==MaterialKind.Stone?g.Spin:0,g.Spin,0);
                if(gold)t.localScale*=1+finishGlow*.10f;
            }
            for(int i=0;i<glints.Length;i++)
            {
                float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(Time.time*2.2f+i*1.7f)),12);
                glints[i].localPosition=grains[140+i*2].localPosition+new Vector3(.035f,.085f,.025f);
                float firstGlint=i<2?Mathf.Sin(Mathf.Clamp01(revealClock/.48f)*Mathf.PI):0;
                glints[i].localScale=Vector3.one*(sim.Loaded?finishGlow*Mathf.Max(pulse,firstGlint)*.15f:0);
            }
        }

        Vector3 FlowPoint(float t)
        {
            if(t<.55f) {float z=Mathf.Lerp(-2.05f,-2.94f,t/.55f);return new Vector3(0,.20f+(-z-1.94f)*.70f,z);}
            float fall=(t-.55f)/.45f;
            // Beyond the rim the sheet falls with world gravity, not along a rotated pan normal.
            return new Vector3(0,.90f,-2.94f)+transform.InverseTransformVector(new Vector3(0,-fall*fall*1.05f,-fall*.65f));
        }

        void DrawPour(PanSimulation sim,float dt)
        {
            VisibleFlow=sim.Loaded&&!sim.Ready?sim.PourAmount:0;flowClock+=dt;
            OutflowMouth=transform.TransformPoint(FlowPoint(.55f));OutflowEnd=transform.TransformPoint(FlowPoint(1));
            outflow.gameObject.SetActive(VisibleFlow>.015f);
            if(VisibleFlow>.015f)
            {
                for(int i=0;i<=12;i++)
                {
                    float t=i/12f;Vector3 center=FlowPoint(t);
                    float width=(.5f+.10f*Mathf.Sin(flowClock*5+t*14))*(1-t*.35f)*VisibleFlow;
                    flowVertices[i*2]=center+Vector3.left*width;flowVertices[i*2+1]=center+Vector3.right*width;
                }
                outflowMesh.vertices=flowVertices;outflowMesh.RecalculateNormals();outflowMesh.RecalculateBounds();
            }
            for(int i=0;i<flowMarks.Length;i++)
            {
                float t=Mathf.Repeat(flowClock*.9f+i*.1f,1);
                flowMarks[i].localPosition=FlowPoint(t)+new Vector3(Mathf.Sin(i*2.4f)*.3f,.014f,0);
                flowMarks[i].localScale=new Vector3(.023f,1,.11f)*VisibleFlow*Mathf.Sin(t*Mathf.PI);
            }
        }

        void OnDestroy() {if(cleanGold)Destroy(cleanGold);if(glintMaterial)Destroy(glintMaterial);if(glintMesh)Destroy(glintMesh);if(outflowMesh)Destroy(outflowMesh);if(siltMaterial)Destroy(siltMaterial);if(badgeMaterial)Destroy(badgeMaterial);if(riffleLight)Destroy(riffleLight);if(concentrateMesh)Destroy(concentrateMesh);if(siltMesh)Destroy(siltMesh);
            foreach(var crest in riffleCrests)if(crest)Destroy(crest.GetComponent<MeshFilter>().sharedMesh);}

        public static PanView Build(Transform parent, Shader shader, Shader water)
        {
            var root=new GameObject("The pan • anchored working surface").transform;root.SetParent(parent,false);root.localPosition=new Vector3(-1.6f,.77f,-1.65f);
            var v=root.gameObject.AddComponent<PanView>();
            Material teal=Geometry.Mat("Deep enamel", "#245D60",shader), inside=Geometry.Mat("Sea glass enamel","#4C9390",shader), rim=Geometry.Mat("Warm brass rim","#C7A865",shader), dark=Geometry.Mat("Riffle shadows","#245253",shader);
            var shell=new GameObject("Bowl geometry").transform;shell.SetParent(root,false);
            Geometry.MeshObject("Spun outer bowl",shell,Geometry.Lathe("Outer wall",new[]{new Vector2(0,-.12f),new Vector2(2.05f,-.12f),new Vector2(2.88f,.69f),new Vector2(2.88f,.77f)}),teal,Vector3.zero);
            Geometry.MeshObject("Sloping inner bowl",shell,Geometry.Lathe("Inner concave bowl",new[]{new Vector2(2.86f,.76f),new Vector2(2.72f,.69f),new Vector2(1.94f,.17f),new Vector2(0,.17f)}),inside,Vector3.zero);
            Geometry.MeshObject("Rolled brass lip",shell,Geometry.Lathe("Rim cross section",new[]{new Vector2(2.79f,.70f),new Vector2(2.79f,.79f),new Vector2(2.85f,.835f),new Vector2(2.94f,.79f),new Vector2(2.94f,.70f)}),rim,Vector3.zero);
            Geometry.Shape("Thumb rest",shell,PrimitiveType.Cube,new Vector3(-2.83f,.58f,0),new Vector3(.38f,.15f,.7f),teal,new Vector3(0,0,-10));
            Geometry.Shape("Thumb rest",shell,PrimitiveType.Cube,new Vector3(2.83f,.58f,0),new Vector3(.38f,.15f,.7f),teal,new Vector3(0,0,10));
            Geometry.Combine(shell);
            v.riffleAccent=new GameObject("Front working riffles • 108 degree sector").transform;v.riffleAccent.SetParent(root,false);
            v.riffleAccent.localRotation=Quaternion.Euler(0,180,0);
            for(int i=0;i<4;i++)
            {
                float r=2.0f+i*.20f, y=.23f+i*.135f;
                Geometry.MeshObject("Riffle "+(i+1),v.riffleAccent,Geometry.Lathe("Raised arc",new[]{new Vector2(r-.035f,y),new Vector2(r-.035f,y+.055f),new Vector2(r+.065f,y+.07f),new Vector2(r+.07f,y)},56,-54,108),i%2==0?dark:teal,Vector3.zero);
            }
            Geometry.Combine(v.riffleAccent);
            v.pebble=Geometry.Pebble();
            v.grainMaterials=new[]{Geometry.Mat("Ochre gravel","#B39159",shader),Geometry.Mat("River stone","#8F9E98",shader),Geometry.Mat("Magnetite","#303B43",shader),Geometry.Mat("Butter gold","#FFD05A",shader)};
            v.sedimentBed=Geometry.Shape("Layer of loose silt",root,PrimitiveType.Sphere,new Vector3(0,.25f,0),Vector3.one,v.grainMaterials[0]);
            v.sedimentBed.GetComponent<MeshFilter>().sharedMesh=Geometry.Lathe("Silt mound",new[]{new Vector2(2.08f,-.1f),new Vector2(2.06f,.07f),new Vector2(1.25f,.62f),new Vector2(0,.7f)},64);
            v.blackBed=Geometry.Shape("Concentrate layer",root,PrimitiveType.Sphere,new Vector3(0,.22f,0),Vector3.one,v.grainMaterials[2]);
            v.blackBed.GetComponent<MeshFilter>().sharedMesh=v.sedimentBed.GetComponent<MeshFilter>().sharedMesh;
            v.waterFilm=Geometry.MeshObject("Water at smooth lip",root,Geometry.Lathe("Shallow water band",new[]{new Vector2(2.13f,.30f),new Vector2(2.22f,.37f)},56,100,160),Geometry.Mat("Pan water","#5BA6A2",water),Vector3.zero).transform;
            return v;
        }
    }
}
