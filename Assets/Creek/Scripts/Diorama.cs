using System.Collections.Generic;
using UnityEngine;

namespace RiffleCreek
{
    public sealed class Diorama : MonoBehaviour
    {
        public Transform[] reeds, foam, fireflies;
        public Transform sluiceFlow, wheel, lanternGlow;
        Vector3[] foamAnchors;
        public CampProgressView Camp {get;private set;}
        public void Initialize()
        {
            Camp=new CampProgressView(transform);
            foamAnchors=new Vector3[foam.Length];
            for(int i=0;i<foam.Length;i++)foamAnchors[i]=foam[i].localPosition;
        }
        public void ApplyProgress(Progression p) { Camp.Apply(p); sluiceFlow.gameObject.SetActive(p.SluiceActive); }
        void OnDestroy() { if(Camp!=null)Camp.Dispose(); }
        public void Animate(Progression progress, PanIntent intent, bool processing, float dt)
        {
            bool active=progress.SluiceActive;
            Camp.Animate(intent,processing,dt);
            float t=Time.time;
            for(int i=0;i<reeds.Length;i++)reeds[i].localRotation=Quaternion.Euler(Mathf.Sin(t*1.2f+i)*3,0,Mathf.Sin(t*.8f+i*1.4f)*4);
            for(int i=0;i<foam.Length;i++)
            {
                float flow=Mathf.Repeat(t*.33f+i*.79f,3);
                foam[i].localPosition=foamAnchors[i]+new Vector3(flow-1.5f,Mathf.Sin(t+i)*.007f,Mathf.Sin(t*.5f+i)*.035f);
                float s=Mathf.Sin(flow/3*Mathf.PI);foam[i].localScale=new Vector3(.3f+s*.6f,.012f,.035f+s*.04f);
            }
            for(int i=0;i<fireflies.Length;i++)
            {
                float s=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*1.6f+i*2.2f)),9)*.07f;
                fireflies[i].localScale=Vector3.one*s;
            }
            sluiceFlow.gameObject.SetActive(active);
            if(active)wheel.Rotate(Vector3.right,dt*35,Space.Self);
            lanternGlow.localScale=Vector3.one*(.17f+Mathf.Sin(t*3)*.008f);
        }

        public static Diorama Build(Transform parent, Shader matte, Shader waterShader)
        {
            var root=new GameObject("Alder creek • one living diorama").transform;root.SetParent(parent,false);
            var scene=root.gameObject.AddComponent<Diorama>();
            var land=new GameObject("Static camp and riverbank").transform;land.SetParent(root,false);
            Material ground=Geometry.Mat("Sage riverbank","#BFC796",matte), edge=Geometry.Mat("Cut earth","#77745A",matte), wood=Geometry.Mat("Cedar","#AA7548",matte), end=Geometry.Mat("Pale end grain","#CEA573",matte), darkWood=Geometry.Mat("Dark timber","#634D38",matte), grass=Geometry.Mat("Sage","#759866",matte), grassLight=Geometry.Mat("Golden grass","#ADB56C",matte), pine=Geometry.Mat("Pine","#416E5B",matte), pineLight=Geometry.Mat("Pine tips","#659275",matte), rock=Geometry.Mat("Warm stone","#A5AA98",matte), rockShade=Geometry.Mat("Slate","#768C87",matte), canvas=Geometry.Mat("Marigold canvas","#E8B65E",matte), canvasLight=Geometry.Mat("Canvas sun side","#F1D08A",matte), iron=Geometry.Mat("Blue iron","#405D62",matte), pale=Geometry.Mat("River foam","#C0E0C5",matte), glow=Geometry.Mat("Lantern butter","#FFE0A0",Shader.Find("Creek/Glow"));
            var river=Geometry.Mat("Jade creek","#4B9D9D",waterShader);
            var island=Geometry.MeshObject("Soft edged riverbank",land,Geometry.Lathe("Island",new[]{new Vector2(0,-.8f),new Vector2(.91f,-.8f),new Vector2(1,-.35f),new Vector2(.99f,0),new Vector2(.90f,.06f),new Vector2(0,.06f)},80),ground,new Vector3(0,-.2f,0)).transform;
            island.localScale=new Vector3(12,1,8.4f);
            var lower=Geometry.Shape("Earth plinth",land,PrimitiveType.Cylinder,new Vector3(0,-.65f,0),new Vector3(23,.35f,16),edge);
            // A graphic river ribbon, with an opaque shader and no refraction or expensive water simulation.
            var vertices=new List<Vector3>();var tri=new List<int>();
            for(int i=0;i<=40;i++)
            {
                float x=-12+i*.6f, mid=2.1f+Mathf.Sin(x*.28f)*.9f;
                vertices.Add(new Vector3(x,.02f,mid-1.25f));vertices.Add(new Vector3(x,.02f,mid+1.35f));
                if(i<40) {int n=i*2;tri.Add(n);tri.Add(n+1);tri.Add(n+2);tri.Add(n+1);tri.Add(n+3);tri.Add(n+2);}
            }
            Geometry.MeshObject("Slow jade water",land,Geometry.Make("Creek ribbon",vertices.ToArray(),tri.ToArray()),river,Vector3.zero);
            var rng=new System.Random(941);
            float R(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
            Mesh pebble=Geometry.Pebble();
            for(int i=0;i<62;i++)
            {
                float x=R(-10.6f,10.6f), bank=(i%2==0?-1.5f:1.6f), z=2.1f+Mathf.Sin(x*.28f)*.9f+bank+R(-.24f,.24f), size=R(.18f,.55f);
                var s=Geometry.MeshObject("Rounded bank stone",land,pebble,i%3==0?rockShade:rock,new Vector3(x,.1f,z)).transform;
                s.localScale=new Vector3(size*1.7f,size,size*1.3f);s.localEulerAngles=new Vector3(R(-8,8),R(0,360),R(-8,8));
            }
            for(int i=0;i<9;i++)
            {
                float x=-9.3f+i*2.18f, z=R(5.0f,6.5f), h=R(2.5f,4.1f);
                Geometry.Beam("Pine trunk",land,new Vector3(x,0,z),new Vector3(x,h*.85f,z),.24f,darkWood);
                for(int k=0;k<3;k++)
                {
                    float radius=(h*.37f)*(1-k*.20f), y=h*.38f+k*h*.20f;
                    var mesh=Geometry.Lathe("Soft pine tier",new[]{new Vector2(0,-.12f),new Vector2(radius*.87f,-.12f),new Vector2(radius,0),new Vector2(radius*.25f,h*.3f),new Vector2(0,h*.48f)},10);
                    Geometry.MeshObject("Pine canopy",land,mesh,k==2?pineLight:pine,new Vector3(x,y,z));
                }
            }
            // A compact wood landing provides a visual frame beneath the pan.
            for(int i=0;i<10;i++)Geometry.Shape("Landing plank",land,PrimitiveType.Cube,new Vector3(-1.65f,.17f,-3.1f+i*.53f),new Vector3(6.9f,.22f,.48f),i%3==0?end:wood);
            for(int i=0;i<4;i++)
            {
                float x=i<2?-5.05f:1.75f,z=i%2==0?-3.3f:1.7f;
                Geometry.Shape("Dock post",land,PrimitiveType.Cylinder,new Vector3(x,.19f,z),new Vector3(.28f,.42f,.28f),darkWood);
                Geometry.Shape("Post cap",land,PrimitiveType.Cylinder,new Vector3(x,.63f,z),new Vector3(.34f,.05f,.34f),end);
            }
            // Tent faces the pan; graphic panels have deliberately broad, uninterrupted colors.
            var tent=new GameObject("Ochre A-frame tent").transform;tent.SetParent(land,false);tent.localPosition=new Vector3(-6.7f,0,3.65f);tent.localEulerAngles=new Vector3(0,-12,0);
            var tv=new[]{new Vector3(-1.4f,0,-1),new Vector3(0,1.95f,-1),new Vector3(1.4f,0,-1),new Vector3(-1.4f,0,1.15f),new Vector3(0,1.95f,1.15f),new Vector3(1.4f,0,1.15f)};
            Geometry.MeshObject("Left canvas",tent,Geometry.Make("Canvas",tv,new[]{0,3,1,1,3,4}),canvasLight,Vector3.zero);
            Geometry.MeshObject("Right canvas",tent,Geometry.Make("Canvas",tv,new[]{1,4,2,2,4,5}),canvas,Vector3.zero);
            Geometry.MeshObject("Dark tent opening",tent,Geometry.Make("Door",new[]{new Vector3(-1.32f,.04f,-.96f),new Vector3(0,1.85f,-.96f),new Vector3(1.32f,.04f,-.96f)},new[]{0,1,2}),darkWood,Vector3.zero);
            Geometry.MeshObject("Open canvas flap",tent,Geometry.Make("Flap",new[]{new Vector3(.10f,1.8f,-1.01f),new Vector3(1.25f,.03f,-1.01f),new Vector3(.90f,.07f,-1.1f)},new[]{0,1,2}),canvasLight,Vector3.zero);
            Geometry.Beam("Tent ridge",tent,new Vector3(0,1.99f,-1.13f),new Vector3(0,1.99f,1.3f),.08f,end);
            Geometry.Beam("Guy rope",tent,new Vector3(0,1.95f,-1.1f),new Vector3(-2.1f,.05f,-.4f),.025f,pale);
            Geometry.Shape("Tent peg",tent,PrimitiveType.Cylinder,new Vector3(-2.1f,.07f,-.4f),new Vector3(.09f,.18f,.09f),wood,new Vector3(15,0,0));
            // Bucket, shovel, lantern, and prospecting stool.
            var bucket=new GameObject("Bucket").transform;bucket.SetParent(land,false);bucket.localPosition=new Vector3(-5.7f,.12f,-1.2f);
            Geometry.MeshObject("Bucket shell",bucket,Geometry.Lathe("Pail",new[]{new Vector2(.27f,0),new Vector2(.39f,.63f),new Vector2(.34f,.66f),new Vector2(.24f,.08f),new Vector2(0,.08f)},40),iron,Vector3.zero);
            Geometry.MeshObject("Bucket water",bucket,Geometry.Lathe("Pail surface",new[]{new Vector2(.32f,.48f),new Vector2(0,.48f)},40),river,Vector3.zero);
            Geometry.Beam("Pail handle",bucket,new Vector3(-.35f,.63f,0),new Vector3(-.35f,1,0),.04f,end);
            Geometry.Beam("Pail handle",bucket,new Vector3(.35f,.63f,0),new Vector3(.35f,1,0),.04f,end);
            Geometry.Beam("Pail grip",bucket,new Vector3(-.35f,1,0),new Vector3(.35f,1,0),.06f,end);
            Geometry.Beam("Shovel shaft",land,new Vector3(-5.4f,.22f,-2.1f),new Vector3(-6.2f,1.95f,-1.8f),.085f,end);
            Geometry.Shape("Shovel blade",land,PrimitiveType.Sphere,new Vector3(-5.34f,.25f,-2.12f),new Vector3(.47f,.65f,.11f),iron,new Vector3(0,0,25));
            Geometry.Beam("Shovel grip",land,new Vector3(-6.42f,1.91f,-1.8f),new Vector3(-6.02f,2.09f,-1.8f),.09f,darkWood);
            Geometry.Shape("Low stool",land,PrimitiveType.Cylinder,new Vector3(-7.1f,.43f,-.7f),new Vector3(1.05f,.13f,.85f),wood);
            for(int i=0;i<3;i++) {float a=i*2.094f;Geometry.Beam("Stool leg",land,new Vector3(-7.1f+Mathf.Sin(a)*.32f,0,-.7f+Mathf.Cos(a)*.25f),new Vector3(-7.1f+Mathf.Sin(a)*.26f,.42f,-.7f+Mathf.Cos(a)*.2f),.12f,darkWood);}
            Vector3 lp=new Vector3(-7.1f,.73f,-.7f);
            Geometry.Shape("Lantern base",land,PrimitiveType.Cylinder,lp,new Vector3(.40f,.07f,.40f),iron);
            Geometry.Shape("Lantern cap",land,PrimitiveType.Cylinder,lp+Vector3.up*.5f,new Vector3(.40f,.07f,.40f),iron);
            for(int i=0;i<4;i++) {float a=i*Mathf.PI*.5f;Geometry.Beam("Lantern rail",land,lp+new Vector3(Mathf.Sin(a)*.16f,0,Mathf.Cos(a)*.16f),lp+new Vector3(Mathf.Sin(a)*.16f,.5f,Mathf.Cos(a)*.16f),.035f,iron);}
            scene.lanternGlow=Geometry.Shape("Warm lantern",root,PrimitiveType.Sphere,lp+Vector3.up*.25f,Vector3.one*.17f,glow);
            // A future equipment spot remains open beside a hand-fed sluice.
            var sluice=new GameObject("Little sluice").transform;sluice.SetParent(land,false);sluice.localPosition=new Vector3(4.0f,.48f,2.1f);sluice.localEulerAngles=new Vector3(9,-18,0);
            Geometry.Shape("Sluice bed",sluice,PrimitiveType.Cube,Vector3.zero,new Vector3(1.25f,.16f,2.8f),darkWood);
            for(int i=0;i<2;i++)Geometry.Shape("Sluice wall",sluice,PrimitiveType.Cube,new Vector3(i==0?-.65f:.65f,.16f,0),new Vector3(.13f,.4f,2.9f),end);
            for(int i=0;i<8;i++)Geometry.Shape("Sluice crossbar",sluice,PrimitiveType.Cube,new Vector3(0,.16f,-1.15f+i*.33f),new Vector3(1.2f,.10f,.075f),wood);
            for(int i=0;i<4;i++)Geometry.Shape("Sluice leg",land,PrimitiveType.Cube,new Vector3(3.4f+i%2*1.2f,.22f,1.1f+i/2*2.2f),new Vector3(.13f,.65f,.13f),darkWood);
            scene.sluiceFlow=Geometry.Shape("Active sluice flow",root,PrimitiveType.Cube,new Vector3(4.0f,.65f,2.1f),new Vector3(1.1f,.035f,2.75f),river,new Vector3(9,-18,0));
            scene.sluiceFlow.gameObject.SetActive(false);
            scene.wheel=new GameObject("Sluice water wheel").transform;scene.wheel.SetParent(root,false);scene.wheel.localPosition=new Vector3(4.95f,.8f,2.5f);
            Geometry.Shape("Wheel hub",scene.wheel,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.8f,.1f,.8f),darkWood,new Vector3(0,0,90));
            for(int i=0;i<8;i++) {float a=i*45*Mathf.Deg2Rad;Geometry.Shape("Paddle",scene.wheel,PrimitiveType.Cube,new Vector3(0,Mathf.Sin(a)*.43f,Mathf.Cos(a)*.43f),new Vector3(.42f,.16f,.27f),wood,new Vector3(-i*45,0,0));}
            Geometry.Combine(scene.wheel);
            // Static grass tufts plus a modest set of centrally animated reeds.
            var swaying=new List<Transform>();
            for(int i=0;i<64;i++)
            {
                float x=R(-10,10), z=2.1f+Mathf.Sin(x*.28f)*.9f+(i%2==0?-1.85f:1.95f)+R(-.3f,.3f);
                if(x>-5.3f&&x<2&&z<1.8f)continue;
                bool animated=i%3==0;
                var tuft=new GameObject(animated?"Swaying reeds":"Grass tuft").transform;tuft.SetParent(animated?root:land,false);tuft.localPosition=new Vector3(x,.05f,z);
                for(int k=0;k<4;k++)
                {
                    float h=R(.25f,.68f), dx=R(-.16f,.16f);
                    Geometry.Beam("Grass blade",tuft,new Vector3(dx,0,R(-.1f,.1f)),new Vector3(dx+R(-.15f,.15f),h,R(-.1f,.1f)),.045f,k%2==0?grass:grassLight);
                }
                if(animated) {Geometry.Combine(tuft);swaying.Add(tuft);}
            }
            scene.reeds=swaying.ToArray();
            scene.foam=new Transform[30];scene.fireflies=new Transform[15];
            for(int i=0;i<scene.foam.Length;i++)
            {
                float x=R(-10.5f,10), z=2.1f+Mathf.Sin(x*.28f)*.9f+R(-.95f,.95f);
                scene.foam[i]=Geometry.Shape("Drifting foam",root,PrimitiveType.Sphere,new Vector3(x,.045f,z),new Vector3(.6f,.012f,.06f),pale);
            }
            for(int i=0;i<scene.fireflies.Length;i++)scene.fireflies[i]=Geometry.Shape("Water glint",root,PrimitiveType.Sphere,new Vector3(R(-8,7),R(.1f,.25f),R(1.5f,3.5f)),Vector3.one*.035f,glow);
            Geometry.Combine(land);
            return scene;
        }
    }
}
