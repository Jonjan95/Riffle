using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek.Editor
{
    // Authors a cohesive landscape layer into the existing scene without rebuilding approved pan or camp art.
    public static class EnvironmentIdentity
    {
        const string ArtPath="Assets/Creek/Generated/EnvironmentArt.asset";
        const string GroupName="Authored creek environment";

        static Transform Group(string name,Transform parent,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }

        static Transform Find(Transform root,string name)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;
            return null;
        }

        [MenuItem("Riffle/Author Alder Creek environment and light build")]
        public static void ApplyAndBuildLight()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            if(!game||!game.diorama||!game.sceneCamera)throw new Exception("Saved Alder Creek scene is incomplete");
            var old=game.diorama.transform.Find(GroupName);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);

            var art=AssetDatabase.LoadAssetAtPath<ArtLibrary>(ArtPath);
            if(art)
            {
                foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(ArtPath))if(obj!=art)UnityEngine.Object.DestroyImmediate(obj,true);
            }
            else {art=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(art,ArtPath);}

            var shader=Shader.Find("Creek/Matte");if(!shader)throw new Exception("Creek matte shader is missing");
            Material Mat(string name,string hex,float receive=.65f)
            {
                var m=Geometry.Mat(name,hex,shader);m.SetFloat("_ReceiveShadows",receive);return m;
            }
            var bankMoss=Mat("Alder bank moss","#929F78");
            var dampBank=Mat("Alder damp bank","#71806B");
            var gravel=Mat("Alder creek gravel","#B7AA84",.55f);
            var path=Mat("Alder worn earth","#A49370",.6f);
            var farGround=Mat("Alder distant sage","#9DA989",.75f);
            var trunk=Mat("Alder charcoal bark","#5B4B3D",.7f);
            var trunkLight=Mat("Alder warm bark","#887058",.65f);
            var leafDark=Mat("Alder deep leaves","#496B58",.55f);
            var leaf=Mat("Alder leaves","#648164",.48f);
            var leafLight=Mat("Alder sun leaves","#819777",.42f);
            var grass=Mat("Alder foreground grass","#627F59",.55f);
            var grassTip=Mat("Alder dry grass tips","#A9A267",.5f);
            var ripple=Mat("Alder shoreline glint","#A9D3C7",.1f);

            var root=Group(GroupName,game.diorama.transform,Vector3.zero);
            var bankRoot=Group("Layered creek banks",root,Vector3.zero);
            var campRoot=Group("Camp ground connections",root,Vector3.zero);
            var treeRoot=Group("Alder tree groups",root,Vector3.zero);
            var depthRoot=Group("Quiet background depth",root,Vector3.zero);
            var foregroundRoot=Group("Foreground framing",root,Vector3.zero);
            var crowns=new List<Transform>();var grasses=new List<Transform>();var ripples=new List<Transform>();
            Mesh bankShape=Geometry.Pebble();
            Mesh crownShape=Geometry.Lathe("Faceted alder crown",new[]{new Vector2(0,-.62f),new Vector2(.62f,-.55f),new Vector2(.96f,-.15f),new Vector2(.92f,.28f),new Vector2(.58f,.66f),new Vector2(.18f,.78f),new Vector2(0,.74f)},11);

            Transform Patch(string name,Transform parent,Vector3 position,Vector3 scale,Material material,float angle=0)
            {
                var t=Geometry.MeshObject(name,parent,bankShape,material,position).transform;t.localScale=scale;t.localEulerAngles=new Vector3(0,angle,0);return t;
            }

            // Overlapping low shelves replace the single hard shoreline with a readable damp-to-dry transition.
            for(int i=0;i<12;i++)
            {
                float x=-9.8f+i*1.78f,mid=2.1f+Mathf.Sin(x*.28f)*.9f,tangent=-Mathf.Cos(x*.28f)*14;
                float varied=(i%3-1)*.07f;
                Patch("Mossy near bank shelf",bankRoot,new Vector3(x,.035f,mid-1.22f+varied),new Vector3(1.35f,.045f,.54f),i%3==0?dampBank:bankMoss,tangent);
                Patch("Damp far bank shelf",bankRoot,new Vector3(x+.22f,.033f,mid+1.31f-varied),new Vector3(1.30f,.04f,.52f),i%4==0?gravel:bankMoss,tangent);
                if(i%2==0)
                {
                    float side=i%4==0?-1:1;
                    Patch("Inside bend gravel bar",bankRoot,new Vector3(x+.35f,.054f,mid+side*1.06f),new Vector3(.83f,.024f,.28f),gravel,tangent+side*5);
                }
            }
            for(int i=0;i<8;i++)
            {
                float x=-8.7f+i*2.45f,mid=2.1f+Mathf.Sin(x*.28f)*.9f,side=i%2==0?-1:1;
                var r=Geometry.Shape("Broken shoreline ripple",bankRoot,PrimitiveType.Sphere,new Vector3(x,.066f,mid+side*1.00f),new Vector3(.70f,.010f,.065f),ripple);
                r.localEulerAngles=new Vector3(0,-Mathf.Cos(x*.28f)*13,0);ripples.Add(r);
            }

            // Broad, low contact shapes make the existing tent, stepping stones and machinery belong to the bank.
            Patch("Tent moss footprint",campRoot,new Vector3(-6.65f,.043f,3.70f),new Vector3(2.20f,.035f,1.36f),dampBank,-12);
            Patch("Tent entrance worn earth",campRoot,new Vector3(-6.15f,.061f,2.55f),new Vector3(1.05f,.020f,.58f),path,-8);
            for(int i=0;i<5;i++)Patch("Soft camp footfall "+(i+1),campRoot,new Vector3(-5.85f+i*.18f,.064f,2.05f-i*.24f),new Vector3(.36f,.016f,.27f),i%2==0?path:gravel,-15+i*7);
            Patch("Sluice lower footing",campRoot,new Vector3(4.18f,.042f,.92f),new Vector3(1.55f,.035f,.70f),dampBank,-12);
            Patch("Sluice upper footing",campRoot,new Vector3(4.15f,.041f,3.28f),new Vector3(1.45f,.032f,.62f),gravel,-17);
            Patch("Tool rack worn patch",campRoot,new Vector3(-3.25f,.045f,3.62f),new Vector3(1.58f,.030f,.73f),path,-4);

            // Low distant forms separate the tree line from the flat backdrop without adding a new location.
            foreach(var data in new[]{new Vector4(-7.0f,.08f,7.4f,6.1f),new Vector4(.2f,-.02f,7.9f,7.2f),new Vector4(7.2f,.05f,7.45f,5.6f)})
            {
                var hill=Geometry.Shape("Soft distant bank rise",depthRoot,PrimitiveType.Sphere,new Vector3(data.x,data.y,data.z),new Vector3(data.w,.78f,2.25f),farGround);
                hill.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            }

            Vector3[] treePositions={new Vector3(-9.0f,0,5.25f),new Vector3(-3.95f,0,6.28f),new Vector3(7.65f,0,5.65f)};
            float[] treeHeights={3.15f,3.65f,3.30f};
            for(int i=0;i<treePositions.Length;i++)
            {
                var tree=Group("Alder grove tree "+(i+1),treeRoot,treePositions[i]);float h=treeHeights[i];
                Geometry.Beam("Forked alder trunk",tree,new Vector3(0,0,0),new Vector3(0,h*.68f,0),.16f,trunk);
                Geometry.Beam("Warm trunk face",tree,new Vector3(-.035f,.22f,-.10f),new Vector3(.035f,h*.66f,-.08f),.055f,trunkLight);
                Geometry.Beam("Alder branch",tree,new Vector3(0,h*.40f,0),new Vector3(-.62f,h*.70f,.08f),.075f,trunk);
                Geometry.Beam("Alder branch",tree,new Vector3(0,h*.48f,0),new Vector3(.66f,h*.76f,-.04f),.07f,trunk);
                var crown=Group("Swaying alder crown",tree,new Vector3(0,h*.73f,0));crowns.Add(crown);
                Vector3[] lobes={new Vector3(0,.28f,0),new Vector3(-.70f,.02f,.08f),new Vector3(.68f,.10f,-.05f),new Vector3(-.30f,.58f,-.08f),new Vector3(.34f,.55f,.10f)};
                for(int k=0;k<lobes.Length;k++)
                {
                    var canopy=Geometry.MeshObject("Rounded alder leaf mass",crown,crownShape,k==3?leafLight:k%2==0?leaf:leafDark,lobes[k]).transform;
                    float s=(i==1?1.08f:1)*(k==0?1.08f:.82f);canopy.localScale=new Vector3(1.05f*s,.72f*s,.88f*s);canopy.localEulerAngles=new Vector3(0,k*31+i*17,0);
                }
                for(int k=0;k<3;k++)Patch("Alder understory",tree,new Vector3((k-1)*.47f,.10f,(k%2-.5f)*.32f),new Vector3(.62f,.28f,.52f),k==1?leaf:leafDark,k*24);
            }

            // Sparse foreground shapes frame the landing and keep the pan as the clean central silhouette.
            for(int side=-1;side<=1;side+=2)
            {
                Patch("Foreground river stone",foregroundRoot,new Vector3(side*8.7f,.14f,-4.65f),new Vector3(1.15f,.38f,.92f),side<0?dampBank:gravel,side*12);
                for(int i=0;i<6;i++)
                {
                    var tuft=Group("Foreground grass tuft",foregroundRoot,new Vector3(side*(7.0f+i*.34f),.04f,-4.15f-(i%3)*.35f));grasses.Add(tuft);
                    for(int k=0;k<4;k++)
                    {
                        float spread=(k-1.5f)*.10f,height=.58f+(i+k)%3*.18f;
                        Geometry.Beam("Long grass blade",tuft,new Vector3(spread,0,0),new Vector3(spread+side*(k-1.5f)*.055f,height,(k%2-.5f)*.11f),.035f,k==3?grassTip:grass);
                    }
                }
            }
            Geometry.Beam("Bank-fallen alder branch",foregroundRoot,new Vector3(-1.9f,.06f,4.18f),new Vector3(.10f,.16f,4.52f),.09f,trunk);
            Geometry.Beam("Small branch fork",foregroundRoot,new Vector3(-.65f,.13f,4.38f),new Vector3(-.20f,.48f,4.48f),.045f,trunkLight);

            var motion=root.gameObject.AddComponent<CreekEnvironmentMotion>();
            motion.treeCrowns=crowns.ToArray();motion.foregroundGrass=grasses.ToArray();motion.bankRipples=ripples.ToArray();
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string n=renderer.sharedMaterial?renderer.sharedMaterial.name:"";
                if(n.Contains("glint")||n.Contains("grass"))renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            void Save(UnityEngine.Object obj) {if(obj&&!AssetDatabase.Contains(obj))AssetDatabase.AddObjectToAsset(obj,art);}
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))Save(filter.sharedMesh);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.sharedMaterials)Save(material);
            EditorUtility.SetDirty(motion);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Verify();EditorWorkflowChecks.VerifyStartupScene();RiffleUrpMigration.VerifyConfiguration();BuildLight();
        }

        static void BuildLight()
        {
            Directory.CreateDirectory("Builds/Windows");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Creek/Scenes/AlderCreek.unity"},locationPathName="Builds/Windows/Riffle.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
            });
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Light environment Windows build failed: "+result.summary.result);
            Debug.Log("RIFFLE: Light environment Windows build succeeded, "+result.summary.totalSize+" bytes.");
        }

        public static void Verify()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();var root=game&&game.diorama?game.diorama.transform.Find(GroupName):null;
            if(!root||!root.GetComponent<CreekEnvironmentMotion>())throw new Exception("Authored environment or motion component is missing");
            foreach(var required in new[]{"Layered creek banks","Camp ground connections","Alder tree groups","Quiet background depth","Foreground framing"})
                if(!root.Find(required))throw new Exception("Missing environment group: "+required);
            if(!Find(root,"Mossy near bank shelf")||!Find(root,"Swaying alder crown")||!Find(root,"Foreground grass tuft"))throw new Exception("Key authored environment silhouettes are missing");
            int renderers=0;
            foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(!r.sharedMaterial||!r.GetComponent<MeshFilter>()||!r.GetComponent<MeshFilter>().sharedMesh)throw new Exception("Incomplete environment renderer: "+r.name);
                renderers++;
            }
            Directory.CreateDirectory("Playtest");
            File.WriteAllText("Playtest/environment-identity-checks.txt","PASS: Layered creek banks and camp contact shapes are saved\nPASS: Authored alder and foreground silhouettes are saved\nPASS: Visual-only environment motion is connected\nPASS: "+renderers+" environment renderers have meshes and materials\n");
        }
    }
}
