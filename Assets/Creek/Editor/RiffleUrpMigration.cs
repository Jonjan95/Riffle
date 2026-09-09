using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RiffleCreek.Editor
{
    public static class RiffleUrpMigration
    {
        public const string PipelineAssetPath = "Assets/Creek/Rendering/RiffleURP.asset";
        public const string RendererAssetPath = "Assets/Creek/Rendering/RiffleURPRenderer.asset";
        const string ArtAssetPath = "Assets/Creek/Generated/CreekArt.asset";

        [MenuItem("Riffle/Rendering/Configure URP")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Creek/Rendering");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererAssetPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            }

            pipeline.msaaSampleCount = 4;
            pipeline.shadowDistance = 35;
            pipeline.shadowCascadeCount = 4;
            GraphicsSettings.defaultRenderPipeline = pipeline;

            ConvertMaterials();
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            VerifyConfiguration();
        }

        static void ConvertMaterials()
        {
            var matte = RequiredShader("Creek/Matte");
            var water = RequiredShader("Creek/Water");
            var glow = RequiredShader("Creek/Glow");

            foreach (var material in AssetDatabase.LoadAllAssetsAtPath(ArtAssetPath).OfType<Material>())
                ConvertMaterial(material, matte, water, glow);
        }

        static void ConvertMaterial(Material material, Shader matte, Shader water, Shader glow)
        {
            if (material == null) return;
            Shader replacement = null;
            if (material.name == "Lantern butter" || material.shader == null || material.shader.name == "Unlit/Color")
                replacement = glow;
            else if (material.shader.name == "Creek/Matte")
                replacement = matte;
            else if (material.shader.name == "Creek/Water")
                replacement = water;

            if (replacement == null || material.shader == replacement) return;
            material.shader = replacement;
            EditorUtility.SetDirty(material);
        }

        static Shader RequiredShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null || !shader.isSupported)
                throw new Exception("URP shader is unavailable: " + name);
            return shader;
        }

        [MenuItem("Riffle/Rendering/Verify URP")]
        public static void VerifyConfiguration()
        {
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
                throw new Exception("Riffle is not assigned a Universal Render Pipeline Asset.");

            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != pipeline)
                throw new Exception("The active quality level overrides Riffle's URP asset.");

            RequiredShader("Creek/Matte");
            RequiredShader("Creek/Water");
            RequiredShader("Creek/Glow");
            var lantern = AssetDatabase.LoadAllAssetsAtPath(ArtAssetPath).OfType<Material>()
                .FirstOrDefault(material => material.name == "Lantern butter");
            if (lantern == null || lantern.shader == null || lantern.shader.name != "Creek/Glow")
                throw new Exception("The lantern material did not migrate to the URP glow shader.");

            Debug.Log("RIFFLE: URP configuration and material verification passed.");
        }
    }
}
