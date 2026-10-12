using MintTycoon.Configs;
using MintTycoon.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage2InventorySetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PlayerPrefabPath = "Assets/_MintTycoon/Art/Prefabs/Player_Greybox.prefab";
        private const string PrefabFolder = "Assets/_MintTycoon/Art/Prefabs/Resources";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials/Resources";
        private const string DefinitionFolder = "Assets/_MintTycoon/Configs/Resources";

        [MenuItem("Mint Tycoon/Stage 2/Set Up Inventory and Resources")]
        public static void SetUp()
        {
            EnsureFolder("Assets/_MintTycoon/Art/Prefabs", "Resources");
            EnsureFolder("Assets/_MintTycoon/Art/Materials", "Resources");
            EnsureFolder("Assets/_MintTycoon/Configs", "Resources");

            CreateStackPrefab("CopperOre_Stack", PrimitiveType.Sphere, new Vector3(0.24f, 0.18f, 0.20f), new Color(0.61f, 0.28f, 0.13f));
            CreateStackPrefab("CopperIngot_Stack", PrimitiveType.Cube, new Vector3(0.28f, 0.13f, 0.18f), new Color(0.79f, 0.39f, 0.19f));
            CreateStackPrefab("CopperCoin_Stack", PrimitiveType.Cylinder, new Vector3(0.18f, 0.035f, 0.18f), new Color(0.91f, 0.61f, 0.24f));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ResourceDefinition[] definitions =
            {
                CreateDefinition(ResourceType.CopperOre, "Copper Ore", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/CopperOre_Stack.prefab")),
                CreateDefinition(ResourceType.CopperIngot, "Copper Ingot", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/CopperIngot_Stack.prefab")),
                CreateDefinition(ResourceType.CopperCoin, "Copper Coin", AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/CopperCoin_Stack.prefab"))
            };

            AssetDatabase.SaveAssets();
            ConfigurePlayerPrefab(definitions);
            ConfigureScenePlayer(definitions);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Mint Tycoon Stage 2 inventory and resource assets are ready.");
        }

        private static ResourceDefinition CreateDefinition(ResourceType type, string displayName, GameObject prefab)
        {
            string path = $"{DefinitionFolder}/{type}.asset";
            ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ResourceDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("type").enumValueIndex = (int)type;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("stackVisualPrefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject CreateStackPrefab(string name, PrimitiveType shape, Vector3 scale, Color color)
        {
            string prefabPath = $"{PrefabFolder}/{name}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Material material = LoadOrCreateMaterial(name + "_Material", color);
            if (existing != null)
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
                contents.transform.localScale = scale;
                Renderer existingRenderer = contents.GetComponentInChildren<Renderer>();
                if (existingRenderer != null)
                    existingRenderer.sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                Object.DestroyImmediate(contents);
                return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            GameObject visual = GameObject.CreatePrimitive(shape);
            visual.name = name;
            visual.transform.localScale = scale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(visual, prefabPath);
            Object.DestroyImmediate(visual);
            return prefab;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigurePlayerPrefab(ResourceDefinition[] definitions)
        {
            GameObject player = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            ConfigurePlayer(player, definitions);
            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
        }

        private static void ConfigureScenePlayer(ResourceDefinition[] definitions)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player");
            if (player == null)
                throw new MissingReferenceException("The greybox scene does not contain a GameObject named Player.");

            ConfigurePlayer(player, definitions);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = player;
        }

        private static void ConfigurePlayer(GameObject player, ResourceDefinition[] definitions)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null)
                inventory = Undo.AddComponent<PlayerInventory>(player);

            inventory.enabled = true;
            Transform anchor = player.transform.Find("Inventory Stack Anchor");
            if (anchor == null)
            {
                GameObject anchorObject = new GameObject("Inventory Stack Anchor");
                Undo.RegisterCreatedObjectUndo(anchorObject, "Create inventory stack anchor");
                anchor = anchorObject.transform;
                anchor.SetParent(player.transform, false);
            }
            anchor.localPosition = new Vector3(0f, 1.62f, -0.12f);
            anchor.localRotation = Quaternion.identity;
            anchor.localScale = Vector3.one;

            InventoryStackView view = player.GetComponent<InventoryStackView>();
            if (view == null)
                view = Undo.AddComponent<InventoryStackView>(player);

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("inventory").objectReferenceValue = inventory;
            serializedView.FindProperty("stackAnchor").objectReferenceValue = anchor;
            SerializedProperty definitionsProperty = serializedView.FindProperty("resourceDefinitions");
            definitionsProperty.arraySize = definitions.Length;
            for (int i = 0; i < definitions.Length; i++)
                definitionsProperty.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
