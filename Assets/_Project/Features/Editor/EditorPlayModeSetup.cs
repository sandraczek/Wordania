using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EditorPlayModeSetup
{
    private const string BootstrapScenePath = "Assets/_Project/Features/Scene/Scenes/Boot.unity";

    static EditorPlayModeSetup()
    {
        SceneAsset bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

        if (bootScene != null)
        {
            EditorSceneManager.playModeStartScene = bootScene;
        }
        else
        {
            Debug.LogError($"Scene not found: {BootstrapScenePath}. ");
        }
    }
}