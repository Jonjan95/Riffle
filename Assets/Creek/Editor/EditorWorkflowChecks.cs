using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RiffleCreek.Editor
{
    /// <summary>Editor-only verification for the normal Hub → Play workflow.</summary>
    public static class EditorWorkflowChecks
    {
        [MenuItem("Riffle/Verify editor startup scene")]
        public static void VerifyStartupScene()
        {
            RiffleEditorBootstrap.ConfigurePlayModeStartScene();
            var expected = AssetDatabase.LoadAssetAtPath<SceneAsset>(RiffleEditorBootstrap.StartupScenePath);
            if (expected == null || EditorSceneManager.playModeStartScene != expected)
                throw new Exception("Riffle Play Mode startup scene is not configured.");

            var scene = EditorSceneManager.OpenScene(RiffleEditorBootstrap.StartupScenePath, OpenSceneMode.Single);
            var game = UnityEngine.Object.FindAnyObjectByType<CreekGame>();
            if (!scene.isLoaded || game == null || game.sceneCamera == null || game.pan == null || game.diorama == null)
                throw new Exception("Saved Riffle startup scene is incomplete.");

            Directory.CreateDirectory("Playtest");
            File.WriteAllText("Playtest/editor-workflow-check.txt",
                "PASS: Play Mode Start Scene is " + RiffleEditorBootstrap.StartupScenePath + Environment.NewLine +
                "PASS: Saved scene contains Riffle game, camera, pan, and diorama references." + Environment.NewLine);
            Debug.Log("RIFFLE: Editor startup scene verification passed.");
        }
    }
}
