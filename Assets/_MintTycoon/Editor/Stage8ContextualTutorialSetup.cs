using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage8ContextualTutorialSetup
    {
        [MenuItem("Mint Tycoon/Stage 8/Set Up Contextual Tutorial")]
        public static void SetUp()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity", OpenSceneMode.Single);
            MinimalHudSetup.Build();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Mint Tycoon: compact counters and brief contextual hints are ready.");
        }
    }
}
