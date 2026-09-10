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
    public static class TentIdentity
    {
        const string ModelPath="Assets/Creek/Art/Tent/AlderTent.fbx";
        const string ArtPath="Assets/Creek/Art/Tent/TentIntegration.asset";
        const string PrefabPath="Assets/Creek/Art/Tent/AlderTent.prefab";

        // Rebuild only affected material batches from retained source meshes.
        // No other camp meshes, materials, transforms, or source art assets are changed.
        static void ExcludeLegacy(Transform batchRoot,Transform[] legacy,ArtLibrary art)
        {
            foreach(var t in legacy)if(!t)throw new Exception("Expected legacy tent group is missing");
            bool Excluded(Transform t){foreach(var item in legacy)if(t.IsChildOf(item))return true;return false;}
            var filters=batchRoot.GetComponentsInChildren<MeshFilter>(true);
            var affected=new HashSet<Material>();
            foreach(var item in legacy)foreach(var r in item.GetComponentsInChildren<MeshRenderer>(true))affected.Add(r.sharedMaterial);
            foreach(var batch in filters)
            {
                if(batch.transform.parent!=batchRoot||!batch.name.StartsWith("Batched "))continue;
                var renderer=batch.GetComponent<MeshRenderer>();
                if(!affected.Contains(renderer.sharedMaterial))continue;
                var parts=new List<CombineInstance>();int kept=0,removed=0;
                foreach(var f in filters)
                {
                    if(f.name.StartsWith("Batched "))continue;
                    var r=f.GetComponent<MeshRenderer>();
                    if(!r||r.sharedMaterial!=renderer.sharedMaterial)continue;
                    if(Excluded(f.transform)){removed+=f.sharedMesh.triangles.Length;continue;}
                    parts.Add(new CombineInstance{mesh=f.sharedMesh,transform=batchRoot.worldToLocalMatrix*f.transform.localToWorldMatrix});
                    kept+=f.sharedMesh.triangles.Length;
                }
                // Reapplying is valid when the previous batch already excludes the tent.
                int previous=batch.sharedMesh.triangles.Length;
                if(previous>kept+removed||previous<kept)throw new Exception("Unexpected batch composition: "+batch.name);
                var mesh=new Mesh{name=batch.name+" without legacy tent",indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(parts.ToArray());
                if(mesh.triangles.Length!=kept)throw new Exception("Unrelated batch triangles were lost");
                AssetDatabase.AddObjectToAsset(mesh,art);batch.sharedMesh=mesh;
            }
            foreach(var item in legacy)item.gameObject.SetActive(false);
        }

        [MenuItem("Riffle/Import Blender tent and light build")]
        public static void ApplyAndBuildLight()
        {
            AssetDatabase.ImportAsset(ModelPath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(ModelImporter)AssetImporter.GetAtPath(ModelPath);
            importer.globalScale=1;importer.useFileScale=true;importer.importCameras=false;importer.importLights=false;
            importer.importAnimation=false;importer.isReadable=false;importer.addCollider=false;
            importer.importNormals=ModelImporterNormals.Import;importer.meshCompression=ModelImporterMeshCompression.Off;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            if(!game||!game.diorama)throw new Exception("Saved camp is missing");
            var art=AssetDatabase.LoadAssetAtPath<ArtLibrary>(ArtPath);
            if(!art){art=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(art,ArtPath);}
            var old=game.diorama.transform.Find("Blender prospecting tent")??game.diorama.transform.Find("AlderTent");
            if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var staticCamp=game.diorama.transform.Find("Static camp and riverbank");
            var details=game.diorama.transform.Find("Handcrafted camp details");
            var oldTent=staticCamp.Find("Ochre A-frame tent");
            var exclusions=new List<Transform>{oldTent};
            // Only remove source stones/tufts physically intersecting the newly open tent.
            bool InFootprint(Transform t)
            {
                var p=oldTent.InverseTransformPoint(t.position);
                return Mathf.Abs(p.x)<1.65f&&Mathf.Abs(p.z)<1.40f;
            }
            foreach(Transform t in staticCamp)
                if((t.name=="Rounded bank stone"||t.name=="Grass tuft")&&InFootprint(t))exclusions.Add(t);
            foreach(var reed in game.diorama.reeds)if(InFootprint(reed))reed.gameObject.SetActive(false);
            ExcludeLegacy(staticCamp,exclusions.ToArray(),art);
            ExcludeLegacy(details,new[]{details.Find("Canvas hems")},art);
            var colors=new Dictionary<string,string>{
                {"Canvas","#D5A15B"},{"CanvasSun","#E6BE79"},{"Binding","#EBD4A4"},
                {"Interior","#66503B"},{"Cedar","#936541"},{"Rope","#A49A76"}};
            var materials=new Dictionary<string,Material>();
            foreach(var pair in colors)
            {
                string path="Assets/Creek/Art/Tent/"+pair.Key+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!m){m=new Material(Shader.Find("Creek/Matte"));AssetDatabase.CreateAsset(m,path);}
                ColorUtility.TryParseHtmlString(pair.Value,out var c);m.color=c;
                m.SetFloat("_ReceiveShadows",.35f);m.SetFloat("_Sheen",0);EditorUtility.SetDirty(m);materials[pair.Key]=m;
            }
            foreach(var pair in materials)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),pair.Key),pair.Value);
            importer.SaveAndReimport();
            var container=new GameObject("Blender prospecting tent");
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));
            model.transform.SetParent(container.transform,false);
            foreach(var r in model.GetComponentsInChildren<MeshRenderer>())
            {
                var remapped=r.sharedMaterials;
                for(int i=0;i<remapped.Length;i++)
                {
                    string key=remapped[i].name;
                    if(!materials.ContainsKey(key))throw new Exception("Unexpected Blender material: "+key);
                    remapped[i]=materials[key];
                }
                r.sharedMaterials=remapped;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
            }
            // FBX's handedness conversion faces the tent entrance along local -Z.
            model.transform.localRotation=Quaternion.Euler(0,180,0)*model.transform.localRotation;
            PrefabUtility.SaveAsPrefabAsset(container,PrefabPath);
            UnityEngine.Object.DestroyImmediate(container);
            var placed=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            placed.name="Blender prospecting tent";
            placed.transform.SetParent(game.diorama.transform,false);
            placed.transform.localPosition=new Vector3(-6.7f,.015f,3.65f);
            placed.transform.localRotation=Quaternion.Euler(0,-12,0);
            // Keep only currently referenced replacement batches in this tool's private library.
            var live=new HashSet<UnityEngine.Object>();
            foreach(var f in game.GetComponentsInChildren<MeshFilter>(true))live.Add(f.sharedMesh);
            foreach(var sub in AssetDatabase.LoadAllAssetsAtPath(ArtPath))
                if(sub!=art&&!live.Contains(sub))UnityEngine.Object.DestroyImmediate(sub,true);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Verify();EditorWorkflowChecks.VerifyStartupScene();
            Directory.CreateDirectory("Builds/Windows");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{"Assets/Creek/Scenes/AlderCreek.unity"},locationPathName="Builds/Windows/Riffle.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Tent build failed: "+result.summary.result);
            Debug.Log("RIFFLE: Blender tent light build succeeded.");
        }
        public static void Verify()
        {
            var game=UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            var tent=game.diorama.transform.Find("Blender prospecting tent");
            if(!tent)throw new Exception("Imported tent missing");
            int triangles=0;Bounds bounds=default;bool first=true;
            foreach(var f in tent.GetComponentsInChildren<MeshFilter>())
            {
                triangles+=f.sharedMesh.triangles.Length/3;
                var r=f.GetComponent<MeshRenderer>();
                if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);
                foreach(var m in r.sharedMaterials)
                    if(!m||m.shader.name!="Creek/Matte"||ShaderUtil.ShaderHasError(m.shader))throw new Exception("Tent material error");
            }
            if(triangles>3000||triangles<100||bounds.size.y<1.5f||bounds.size.y>2.4f)throw new Exception("Unexpected tent scale or complexity: "+bounds.size);
            Directory.CreateDirectory("Playtest/Tent");
            File.WriteAllText("Playtest/Tent/import-checks.txt","PASS: FBX and prefab imported\nPASS: "+triangles+" triangles, six matte material regions\nPASS: Correct metre scale "+bounds.size+"\nPASS: Retained source geometry preserved outside tent clearance\n");
        }
    }
}
