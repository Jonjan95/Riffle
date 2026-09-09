using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek.Editor
{
    public static class BuildCreek
    {
        [MenuItem("Riffle/Create fresh creek scene")]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Creek/Scenes");Directory.CreateDirectory("Assets/Creek/Generated");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.70f,.76f,.70f);
            RenderSettings.fog=false;
            var key=new GameObject("Late afternoon sun").AddComponent<Light>();
            key.type=LightType.Directional;key.color=new Color(1,.93f,.79f);key.intensity=1.15f;key.shadows=LightShadows.Soft;key.shadowStrength=.58f;key.shadowBias=.045f;key.transform.rotation=Quaternion.Euler(48,-35,0);
            var camera=new GameObject("Fixed diorama camera").AddComponent<Camera>();camera.tag="MainCamera";
            camera.transform.position=new Vector3(0,12,-12);camera.transform.LookAt(new Vector3(0,.3f,.3f));
            camera.orthographic=true;camera.orthographicSize=5.65f;camera.nearClipPlane=.1f;camera.farClipPlane=70;
            camera.backgroundColor=new Color(.78f,.80f,.68f);camera.clearFlags=CameraClearFlags.SolidColor;camera.allowHDR=false;camera.allowMSAA=true;
            camera.gameObject.AddComponent<AudioListener>();
            var game=new GameObject("Riffle • vertical slice").AddComponent<CreekGame>();
            var matte=Shader.Find("Creek/Matte");var water=Shader.Find("Creek/Water");
            if(matte==null||water==null)throw new Exception("Custom shaders did not import.");
            game.sceneCamera=camera;game.diorama=Diorama.Build(game.transform,matte,water);game.pan=PanView.Build(game.transform,matte,water);
            game.pan.sedimentBed.gameObject.SetActive(false);game.pan.blackBed.gameObject.SetActive(false);
            // Persist generated meshes and materials so the entire diorama is visible and editable outside Play mode.
            const string artPath="Assets/Creek/Generated/CreekArt.asset";
            if(AssetDatabase.LoadAssetAtPath<ArtLibrary>(artPath)!=null)AssetDatabase.DeleteAsset(artPath);
            var library=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(library,artPath);
            var seen=new HashSet<UnityEngine.Object>();
            void Save(UnityEngine.Object obj)
            {
                if(obj==null||!seen.Add(obj)||AssetDatabase.Contains(obj))return;
                AssetDatabase.AddObjectToAsset(obj,library);
            }
            foreach(var filter in game.GetComponentsInChildren<MeshFilter>(true))Save(filter.sharedMesh);
            foreach(var renderer in game.GetComponentsInChildren<MeshRenderer>(true))foreach(var mat in renderer.sharedMaterials)Save(mat);
            Save(game.pan.pebble);foreach(var mat in game.pan.grainMaterials)Save(mat);
            AssetDatabase.SaveAssets();
            QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=35;
            QualitySettings.antiAliasing=4;QualitySettings.vSyncCount=1;
            PlayerSettings.companyName="Small River Studio";PlayerSettings.productName="Riffle - Alder Creek";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.colorSpace=ColorSpace.Gamma;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
            EditorSceneManager.SaveScene(scene,"Assets/Creek/Scenes/AlderCreek.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Creek/Scenes/AlderCreek.unity",true)};
            AssetDatabase.SaveAssets();
            Debug.Log("RIFFLE: Fresh diorama saved.");
        }

        public static void BuildAll()
        {
            CreateScene();
            BuildCurrent();
        }

        [MenuItem("Riffle/Reorient saved pan and build")]
        public static void BuildWorkingPan()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            var pan=UnityEngine.Object.FindAnyObjectByType<PanView>();
            // Edit the existing mesh group's transform. Preserve the saved diorama and camera.
            pan.riffleAccent.localRotation=Quaternion.Euler(0,180,0);
            pan.riffleAccent.name="Front working riffles • 108 degree sector";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            BuildCurrent();
        }

        public static void BuildCurrent()
        {
                        RiffleUrpMigration.VerifyConfiguration();
EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");
            SimulationChecks.Run();
            Directory.CreateDirectory("Builds/Windows");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/Creek/Scenes/AlderCreek.unity"},locationPathName="Builds/Windows/Riffle.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
            });
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+result.summary.result);
            Debug.Log("RIFFLE: Windows build succeeded, "+result.summary.totalSize+" bytes.");
        }
    }
}
