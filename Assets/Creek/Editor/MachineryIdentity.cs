using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek.Editor
{
    // Adds the authored machinery around the existing saved creek without regenerating approved scene art.
    public static class MachineryIdentity
    {
        const string ArtPath="Assets/Creek/Generated/MachineryArt.asset";
        const string GroupName="Authored sluice and machinery";

        static Transform Group(string name,Transform parent,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }

        static Transform Find(Transform root,string name)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;
            return null;
        }

        [MenuItem("Riffle/Author sluice and helper identity")]
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            if(!game||!game.diorama)throw new Exception("Saved diorama is missing");
            var previous=game.diorama.transform.Find(GroupName);
            var baseWheel=game.diorama.wheel;
            if(previous&&baseWheel&&baseWheel.IsChildOf(previous))baseWheel=null;
            if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);

            var art=AssetDatabase.LoadAssetAtPath<ArtLibrary>(ArtPath);
            if(art)
            {
                foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(ArtPath))if(obj!=art)UnityEngine.Object.DestroyImmediate(obj,true);
            }
            else {art=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(art,ArtPath);}

            // The legacy solid wheel is a separate object, so it can be hidden without touching the combined camp mesh.
            if(!baseWheel)baseWheel=game.diorama.transform.Find("Sluice water wheel")??game.diorama.transform.Find("Open cedar sluice wheel");
            if(baseWheel)baseWheel.gameObject.SetActive(false);

            var shader=Shader.Find("Creek/Matte");
            Material Mat(string name,string hex,float sheen=0)
            {
                var m=Geometry.Mat(name,hex,shader);m.SetFloat("_ReceiveShadows",.55f);m.SetFloat("_Sheen",sheen);return m;
            }
            var cedar=Mat("Machinery cedar","#A9784E");
            var cedarEnd=Mat("Machinery pale end grain","#C7A06F");
            var dark=Mat("Machinery dark timber","#604C3A");
            var iron=Mat("Machinery blue iron","#405D62",.025f);
            var brass=Mat("Machinery working brass","#C8A563",.08f);

            var root=Group(GroupName,game.diorama.transform,Vector3.zero);
            var sluice=Group("Authored tapered sluice",root,new Vector3(4.0f,.535f,2.1f));
            sluice.localEulerAngles=new Vector3(9,-18,0);
            var troughVerts=new[]{
                new Vector3(-.56f,.02f,-1.49f),new Vector3(.56f,.02f,-1.49f),new Vector3(-.73f,.02f,1.49f),new Vector3(.73f,.02f,1.49f),
                new Vector3(-.56f,-.18f,-1.49f),new Vector3(.56f,-.18f,-1.49f),new Vector3(-.73f,-.18f,1.49f),new Vector3(.73f,-.18f,1.49f)};
            var troughTriangles=new[]{0,2,1,1,2,3,4,5,6,5,7,6,0,4,2,2,4,6,1,3,5,3,7,5,0,1,4,1,5,4,2,6,3,3,6,7};
            Geometry.MeshObject("Tapered cedar trough",sluice,Geometry.Make("Authored tapered trough",troughVerts,troughTriangles),cedar,Vector3.zero);
            Geometry.Shape("Inset blue iron riffle mat",sluice,PrimitiveType.Cube,new Vector3(0,.16f,-.05f),new Vector3(1.02f,.035f,2.58f),iron);
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1:1;
                Geometry.Shape("Sculpted cedar side rail",sluice,PrimitiveType.Cube,new Vector3(side*.64f,.30f,0),new Vector3(.15f,.47f,3.02f),i==0?dark:cedarEnd,new Vector3(0,side*3.4f,0));
            }
            for(int i=0;i<8;i++)
            {
                float z=-1.12f+i*.31f,width=1.03f+i*.035f;
                Geometry.Shape("Removable pale riffle "+(i+1),sluice,PrimitiveType.Cube,new Vector3(0,.225f,z),new Vector3(width,.075f,.065f),cedarEnd);
            }
            Geometry.Shape("Classifier feed deck",sluice,PrimitiveType.Cube,new Vector3(0,.29f,1.48f),new Vector3(1.38f,.12f,.56f),dark);
            for(int i=0;i<6;i++)Geometry.Shape("Classifier iron screen "+(i+1),sluice,PrimitiveType.Cube,new Vector3(-.49f+i*.195f,.37f,1.48f),new Vector3(.045f,.035f,.51f),iron);
            for(int i=0;i<2;i++)Geometry.Shape("Classifier cedar cheek",sluice,PrimitiveType.Cube,new Vector3(i==0?-.72f:.72f,.46f,1.48f),new Vector3(.11f,.35f,.60f),cedar);
            Geometry.Shape("Pale sluice mouth",sluice,PrimitiveType.Cube,new Vector3(0,.21f,-1.51f),new Vector3(1.16f,.16f,.10f),cedarEnd);

            for(int i=0;i<2;i++)
            {
                float z=i==0?1.10f:3.20f;
                Geometry.Beam("Splayed cedar trestle",root,new Vector3(3.27f,.02f,z),new Vector3(3.53f,.67f,z),.08f,dark);
                Geometry.Beam("Splayed cedar trestle",root,new Vector3(4.73f,.02f,z),new Vector3(4.47f,.67f,z),.08f,dark);
                Geometry.Beam("Forged trestle tie",root,new Vector3(3.35f,.27f,z),new Vector3(4.65f,.27f,z),.055f,iron);
            }
            Geometry.Beam("Wheel axle support",root,new Vector3(4.50f,.05f,2.52f),new Vector3(4.84f,.83f,2.52f),.08f,dark);

            var wheel=Group("Open cedar sluice wheel",root,new Vector3(4.93f,.82f,2.52f));
            var rim=Geometry.MeshObject("Open dark timber wheel rim",wheel,Geometry.Lathe("Authored open wheel ring",new[]{new Vector2(.48f,-.055f),new Vector2(.60f,-.055f),new Vector2(.60f,.055f),new Vector2(.48f,.055f)},28),dark,Vector3.zero).transform;
            rim.localEulerAngles=new Vector3(0,0,90);
            Geometry.Shape("Forged wheel hub",wheel,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.20f,.13f,.20f),iron,new Vector3(0,0,90));
            for(int i=0;i<6;i++)
            {
                float a=i*60*Mathf.Deg2Rad;
                Geometry.Beam("Pale wheel spoke",wheel,Vector3.zero,new Vector3(0,Mathf.Sin(a)*.48f,Mathf.Cos(a)*.48f),.045f,cedarEnd);
                Geometry.Shape("Cupped cedar paddle",wheel,PrimitiveType.Cube,new Vector3(0,Mathf.Sin(a)*.60f,Mathf.Cos(a)*.60f),new Vector3(.34f,.12f,.25f),cedar,new Vector3(-i*60,0,0));
            }
            game.diorama.wheel=wheel;
            game.diorama.sluiceFlow.localPosition=new Vector3(4.0f,.70f,2.1f);
            game.diorama.sluiceFlow.localEulerAngles=new Vector3(9,-18,0);
            game.diorama.sluiceFlow.localScale=new Vector3(.98f,.035f,2.62f);

            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode=renderer.sharedMaterial==brass?ShadowCastingMode.Off:ShadowCastingMode.On;
                renderer.receiveShadows=true;
            }
            void Save(UnityEngine.Object obj) {if(obj&&!AssetDatabase.Contains(obj))AssetDatabase.AddObjectToAsset(obj,art);}
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))Save(filter.sharedMesh);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.sharedMaterials)Save(material);
            EditorUtility.SetDirty(game.diorama);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Verify();PanIdentity.Verify();PolishCreek.VerifyPresentation();EditorWorkflowChecks.VerifyStartupScene();BuildCreek.BuildCurrent();
        }

        public static void Verify()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            var root=game&&game.diorama?game.diorama.transform.Find(GroupName):null;
            if(!root||!Find(root,"Tapered cedar trough")||!Find(root,"Open dark timber wheel rim"))throw new Exception("Authored sluice geometry is missing");
            if(game.diorama.wheel.name!="Open cedar sluice wheel")throw new Exception("Diorama animation is not connected to the authored wheel");
            int renderers=0;
            foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(!r.sharedMaterial||!r.GetComponent<MeshFilter>()||!r.GetComponent<MeshFilter>().sharedMesh)throw new Exception("Incomplete machinery renderer: "+r.name);
                renderers++;
            }
            Directory.CreateDirectory("Playtest");
            File.WriteAllText("Playtest/machinery-identity-checks.txt","PASS: Authored tapered sluice and open wheel are saved\nPASS: Sluice wheel animation reference is connected\nPASS: "+renderers+" machinery renderers have meshes and materials\n");
        }
    }
}
