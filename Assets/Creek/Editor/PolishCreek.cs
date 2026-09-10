using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek.Editor
{
    // Authors presentation into the existing scene. Never regenerates the camp or its gameplay objects.
    public static class PolishCreek
    {
        const string ArtPath="Assets/Creek/Generated/PolishArt.asset";
        static Transform Group(string name,Transform parent,Vector3 p)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
        static void Shape(string n,Transform p,Vector3 at,Vector3 size,Material m)
        {Geometry.Shape(n,p,PrimitiveType.Cube,at,size,m);}
        [MenuItem("Riffle/Apply Alder Creek presentation")]
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            var art=AssetDatabase.LoadAssetAtPath<ArtLibrary>(ArtPath);
            if(art!=null)
            {
                // Reapplying only replaces this tool's two finishing groups and their private subassets.
                var oldPan=game.pan.transform.Find("Enamel and brass finishing");
                var oldCamp=game.diorama.transform.Find("Handcrafted camp details");
                if(oldPan)UnityEngine.Object.DestroyImmediate(oldPan.gameObject);
                if(oldCamp)UnityEngine.Object.DestroyImmediate(oldCamp.gameObject);
                foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(ArtPath))if(obj!=art)UnityEngine.Object.DestroyImmediate(obj,true);
            }
            else {art=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(art,ArtPath);}
            var shader=Shader.Find("Creek/Matte");
            var colors=new Dictionary<string,string> {
                {"Sage riverbank","#B4BE98"},{"Sea glass enamel","#4A8986"},{"Deep enamel","#255759"},
                {"Warm brass rim","#D4B87C"},{"Riffle shadows","#2C575A"},{"Ochre gravel","#9D7958"},
                {"River stone","#84989B"},{"Magnetite","#293A40"},{"Pine","#486C5C"},{"Pine tips","#739279"},
                {"Marigold canvas","#D9A45C"},{"Canvas sun side","#ECD098"},{"River foam","#A9D4CB"}
            };
            var seen=new HashSet<Material>();
            foreach(var r in game.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)
            {
                if(!m||!seen.Add(m))continue;
                if(colors.TryGetValue(m.name,out var hex)) {ColorUtility.TryParseHtmlString(hex,out var c);m.color=c;}
                if(m.shader==shader)
                {
                    m.SetFloat("_ReceiveShadows",m.name.Contains("enamel")?.12f:.70f);
                    m.SetFloat("_Sheen",m.name.Contains("enamel")?.07f:m.name.Contains("brass")?.12f:0);
                }
                EditorUtility.SetDirty(m);
            }
            // Decorative grass, foam and water do not need small, noisy projected shadows.
            foreach(var r in game.diorama.GetComponentsInChildren<Renderer>(true))
            {
                string n=r.sharedMaterial?r.sharedMaterial.name:"";
                if(n=="Sage"||n=="Golden grass"||n=="River foam"||n=="Jade creek"||n=="Lantern butter")
                    r.shadowCastingMode=ShadowCastingMode.Off;
            }
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>())
                if(light.type==LightType.Directional) {light.shadows=LightShadows.Soft;light.shadowStrength=.42f;light.shadowNormalBias=.25f;}

            Material Mat(string n,string h)=>Geometry.Mat(n,h,shader);
            Material enamel=Mat("Polish enamel edge","#366E70"), pale=Mat("Polish enamel light","#75A7A0"), brass=Mat("Polish satin brass","#DCC48C");
            Material wood=Mat("Polish cedar trim","#B88959"), dark=Mat("Polish joints","#695744"), canvas=Mat("Polish canvas seam","#F1D8A6");
            Material foam=Mat("Polish bank water","#81B9B0"), stone=Mat("Polish warm footpath","#9DAD9B");
            var pan=Group("Enamel and brass finishing",game.pan.transform,Vector3.zero);
            Geometry.MeshObject("Inner bowl seam",pan,Geometry.Lathe("Enamel well boundary",new[]{new Vector2(1.937f,.179f),new Vector2(1.905f,.178f)},80),enamel,Vector3.zero);
            Geometry.MeshObject("Rear enamel shoulder",pan,Geometry.Lathe("Soft shoulder accent",new[]{new Vector2(2.715f,.689f),new Vector2(2.685f,.670f)},64,-80,160),pale,Vector3.zero);
            Geometry.MeshObject("Satin rolled lip",pan,Geometry.Lathe("Lip highlight",new[]{new Vector2(2.872f,.833f),new Vector2(2.838f,.837f)},80,-95,190),brass,Vector3.zero);
            for(int i=0;i<3;i++)Shape("Front pouring mark",pan,new Vector3((i-1)*.12f,.806f,-2.875f),new Vector3(.035f,.012f,i==1?.115f:.065f),brass);
            foreach(float x in new[]{-2.88f,2.88f})for(int i=0;i<2;i++)
                Geometry.Shape("Thumb rest rivet",pan,PrimitiveType.Sphere,new Vector3(x,.668f,i==0?-.21f:.21f),new Vector3(.055f,.025f,.055f),brass);
            var camp=Group("Handcrafted camp details",game.diorama.transform,Vector3.zero);
            // Thin dock joinery, with broad surfaces left quiet.
            for(int i=0;i<10;i++)foreach(float x in new[]{-4.75f,1.45f})
                Geometry.Shape("Landing peg",camp,PrimitiveType.Cylinder,new Vector3(x,.286f,-3.1f+i*.53f),new Vector3(.06f,.003f,.06f),dark);
            for(int i=0;i<4;i++)
            {
                float x=i<2?-5.05f:1.75f,z=i%2==0?-3.3f:1.7f;
                Geometry.MeshObject("Post collar",camp,Geometry.Lathe("Rope collar",new[]{new Vector2(.144f,.48f),new Vector2(.153f,.50f),new Vector2(.144f,.52f)},24),canvas,new Vector3(x,0,z));
            }
            var tent=Group("Canvas hems",camp,new Vector3(-6.7f,0,3.65f));tent.localRotation=Quaternion.Euler(0,-12,0);
            Geometry.Beam("Left tent hem",tent,new Vector3(-1.4f,.025f,-1.02f),new Vector3(0,1.96f,-1.02f),.035f,canvas);
            Geometry.Beam("Right tent hem",tent,new Vector3(0,1.96f,-1.02f),new Vector3(1.4f,.025f,-1.02f),.035f,canvas);
            Shape("Rolled entrance mat",tent,new Vector3(0,.055f,-.87f),new Vector3(1.25f,.055f,.50f),wood);
            for(int i=0;i<4;i++)Shape("Mat weave",tent,new Vector3(-.48f+i*.32f,.085f,-.87f),new Vector3(.035f,.007f,.47f),canvas);
            var handle=Geometry.MeshObject("Lantern carrying loop",camp,Geometry.Lathe("Lantern loop",new[]{new Vector2(.15f,0),new Vector2(.18f,0)},24,-105,210),dark,new Vector3(-7.1f,1.39f,-.7f));
            handle.transform.localRotation=Quaternion.Euler(90,0,0);
            // A broken waterline follows the authored creek's banks instead of a hard continuous outline.
            for(int side=-1;side<=1;side+=2)for(int i=0;i<14;i++)
            {
                float x=-9.8f+i*1.43f,z=2.1f+Mathf.Sin(x*.28f)*.9f+side*1.20f;
                var t=Geometry.Shape("Quiet shoreline ripple",camp,PrimitiveType.Sphere,new Vector3(x,.034f,z),new Vector3(.52f,.012f,.075f),foam);
                t.localRotation=Quaternion.Euler(0,-Mathf.Cos(x*.28f)*14,0);
            }
            for(int i=0;i<5;i++)
                Geometry.Shape("Camp stepping stone",camp,PrimitiveType.Sphere,new Vector3(-5.7f-i*.22f,.06f,1.0f+i*.3f),new Vector3(.38f,.08f,.30f),stone);
            foreach(var root in new[]{pan,camp})
            {
                Geometry.Combine(root);
                foreach(var r in root.GetComponentsInChildren<Renderer>(true)) {r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
                foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))if(!AssetDatabase.Contains(f.sharedMesh))AssetDatabase.AddObjectToAsset(f.sharedMesh,art);
                foreach(var r in root.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)
                    if(!AssetDatabase.Contains(m))AssetDatabase.AddObjectToAsset(m,art);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            VerifyPresentation();EditorWorkflowChecks.VerifyStartupScene();BuildCreek.BuildCurrent();
        }
        public static void VerifyPresentation()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            if(!game||!game.pan||!game.diorama||!game.sceneCamera)throw new Exception("Missing saved scene references");
            if(!game.pan.transform.Find("Enamel and brass finishing")||!game.diorama.transform.Find("Handcrafted camp details"))throw new Exception("Missing saved presentation groups");
            int count=0;
            foreach(var r in game.GetComponentsInChildren<MeshRenderer>(true))
            {
                var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)throw new Exception("Missing mesh: "+r.name);
                foreach(var m in r.sharedMaterials)if(!m||!m.shader||ShaderUtil.ShaderHasError(m.shader))throw new Exception("Invalid material: "+r.name);
                count++;
            }
            foreach(var t in game.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);
            Directory.CreateDirectory("Playtest");
            File.WriteAllText("Playtest/presentation-checks.txt","PASS: Saved pan/camp/camera references\nPASS: Authored presentation groups\nPASS: "+count+" mesh renderers have valid meshes and error-free materials\nPASS: No missing scripts in the saved game hierarchy\n");
        }
    }
}
