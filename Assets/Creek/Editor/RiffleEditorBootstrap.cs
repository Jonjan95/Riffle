using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RiffleCreek.Editor
{
    /// <summary>
    /// Keeps the Hub-to-Play workflow deterministic. Unity otherwise plays the
    /// scene that happened to be open when the project was last closed.
    /// </summary>
    [InitializeOnLoad]
    public static class RiffleEditorBootstrap
    {
        public const string StartupScenePath = "Assets/Creek/Scenes/AlderCreek.unity";

        static RiffleEditorBootstrap()
        {
            EditorApplication.delayCall += ConfigurePlayModeStartScene;
        }

        [MenuItem("Riffle/Use Alder Creek for Play Mode")]
        public static void ConfigurePlayModeStartScene()
        {
            var startupScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupScenePath);
            if (startupScene == null)
            {
                Debug.LogError("RIFFLE: The saved startup scene is missing at " + StartupScenePath);
                return;
            }

            EditorSceneManager.playModeStartScene = startupScene;
        }
    }
}
