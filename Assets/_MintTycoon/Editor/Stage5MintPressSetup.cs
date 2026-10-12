using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage5MintPressSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Stations/CopperMintPress.prefab";
        private const string RecipePath = "Assets/_MintTycoon/Configs/Recipes/CopperMinting.asset";
        private const string IngotPath = "Assets/_MintTycoon/Configs/Resources/CopperIngot.asset";
        private const string CoinPath = "Assets/_MintTycoon/Configs/Resources/CopperCoin.asset";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 5/Set Up Copper Mint Press")]
        public static void SetUp()
        {
            ResourceDefinition ingot = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(IngotPath);
            ResourceDefinition coin = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(CoinPath);
            if (ingot == null || coin == null)
                throw new MissingReferenceException("Missing copper ingot or coin definition. Complete Stage 2 setup first.");

            ProductionRecipeDefinition recipe = LoadOrCreateRecipe(ingot, coin);
            Material wood = LoadOrCreateMaterial("Press_Wood", new Color(0.35f, 0.20f, 0.10f));
            Material bronze = LoadOrCreateMaterial("Press_Bronze", new Color(0.72f, 0.43f, 0.19f));
            Material metal = LoadOrCreateMaterial("Press_Metal", new Color(0.30f, 0.33f, 0.34f));
            Material inputPad = LoadOrCreateMaterial("Press_InputZone", new Color(0.96f, 0.52f, 0.17f));
            Material outputPad = LoadOrCreateMaterial("Press_OutputZone", new Color(0.20f, 0.68f, 0.69f));
            MakeUnlit(inputPad);
            MakeUnlit(outputPad);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Copper Mint Press");
            RemoveObject("PRESS BLOCKOUT");
            RemoveObject("PRESS BLOCKOUT Label");
            RemoveObject("COPPER MINT PRESS Label");

            GameObject press = new GameObject("Copper Mint Press");
            press.transform.position = new Vector3(2.5f, 0f, -1.5f);
            BuildPressVisual(press.transform, wood, bronze, metal);
            InteractionZone inputZone = BuildBufferZone(press.transform, "Input Zone", 1.78f, inputPad, "INGOT INPUT");
            InteractionZone outputZone = BuildBufferZone(press.transform, "Output Zone", -1.78f, outputPad, "COIN OUTPUT");

            BufferedProductionStation station = press.AddComponent<BufferedProductionStation>();
            SerializedObject serializedStation = new SerializedObject(station);
            serializedStation.FindProperty("recipe").objectReferenceValue = recipe;
            serializedStation.FindProperty("inputZone").objectReferenceValue = inputZone;
            serializedStation.FindProperty("outputZone").objectReferenceValue = outputZone;
            serializedStation.FindProperty("inputCapacity").intValue = 12;
            serializedStation.FindProperty("outputCapacity").intValue = 12;
            serializedStation.FindProperty("transferIntervalSeconds").floatValue = 0.12f;
            serializedStation.ApplyModifiedPropertiesWithoutUndo();

            ProductionBufferStackView view = press.AddComponent<ProductionBufferStackView>();
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("station").objectReferenceValue = station;
            serializedView.FindProperty("inputResource").objectReferenceValue = ingot;
            serializedView.FindProperty("outputResource").objectReferenceValue = coin;
            serializedView.FindProperty("inputAnchor").objectReferenceValue = inputZone.transform.Find("Buffer Stack Anchor");
            serializedView.FindProperty("outputAnchor").objectReferenceValue = outputZone.transform.Find("Buffer Stack Anchor");
            serializedView.FindProperty("inputCountLabel").objectReferenceValue = inputZone.transform.Find("Buffer Count").GetComponent<TextMesh>();
            serializedView.FindProperty("outputCountLabel").objectReferenceValue = outputZone.transform.Find("Buffer Count").GetComponent<TextMesh>();
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAssetAndConnect(press, PrefabPath, InteractionMode.AutomatedAction);
            CreateWorldLabel("COPPER MINT PRESS", press.transform.position + Vector3.up * 2.5f);
            EditorUtility.SetDirty(press);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = press;
            Debug.Log("Mint Tycoon Stage 5 mint press is ready: 1 ingot becomes 2 coins every 0.4 seconds, with bounded buffers and 0.12-second automatic transfers.");
        }

        private static ProductionRecipeDefinition LoadOrCreateRecipe(ResourceDefinition ingot, ResourceDefinition coin)
        {
            ProductionRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<ProductionRecipeDefinition>(RecipePath);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<ProductionRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, RecipePath);
            }

            SerializedObject serialized = new SerializedObject(recipe);
            serialized.FindProperty("input").objectReferenceValue = ingot;
            serialized.FindProperty("inputAmount").intValue = 1;
            serialized.FindProperty("output").objectReferenceValue = coin;
            serialized.FindProperty("outputAmount").intValue = 2;
            serialized.FindProperty("cycleDurationSeconds").floatValue = 0.4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void BuildPressVisual(Transform root, Material wood, Material bronze, Material metal)
        {
            CreateVisual("Press Base", PrimitiveType.Cube, root, new Vector3(0f, 0.30f, 0f), new Vector3(2.20f, 0.60f, 1.72f), wood);
            CreateVisual("Press Anvil", PrimitiveType.Cube, root, new Vector3(0f, 0.67f, 0f), new Vector3(1.78f, 0.20f, 1.22f), metal);
            CreateVisual("Press Upright Left", PrimitiveType.Cube, root, new Vector3(-0.66f, 1.39f, 0f), new Vector3(0.25f, 1.35f, 1.42f), wood);
            CreateVisual("Press Upright Right", PrimitiveType.Cube, root, new Vector3(0.66f, 1.39f, 0f), new Vector3(0.25f, 1.35f, 1.42f), wood);
            CreateVisual("Press Crossbeam", PrimitiveType.Cube, root, new Vector3(0f, 2.08f, 0f), new Vector3(1.72f, 0.32f, 1.48f), bronze);
            CreateVisual("Press Ram", PrimitiveType.Cube, root, new Vector3(0f, 1.45f, -0.08f), new Vector3(0.60f, 0.58f, 0.68f), metal);
            CreateVisual("Press Die", PrimitiveType.Cylinder, root, new Vector3(0f, 0.90f, -0.08f), new Vector3(0.48f, 0.12f, 0.48f), bronze);
            CreateVisual("Press Flywheel", PrimitiveType.Cylinder, root, new Vector3(0.93f, 1.50f, 0.10f), new Vector3(0.70f, 0.12f, 0.70f), bronze).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreateVisual("Flywheel Hub", PrimitiveType.Cylinder, root, new Vector3(0.93f, 1.50f, 0.10f), new Vector3(0.22f, 0.15f, 0.22f), metal).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreateVisual("Press Handle", PrimitiveType.Cube, root, new Vector3(1.27f, 1.93f, 0.1f), new Vector3(0.90f, 0.10f, 0.12f), wood).transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
        }

        private static InteractionZone BuildBufferZone(Transform root, string name, float localZ, Material marker, string labelText)
        {
            const float width = 2.30f;
            const float depth = 1.40f;
            const float groundTop = 0.15f;
            CreateVisual($"{name} Highlight", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + 0.025f, localZ), new Vector3(width - 0.14f, 0.04f, depth - 0.14f), marker);
            const float edge = 0.12f;
            const float edgeHeight = 0.07f;
            CreateVisual($"{name} Marker Front", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + edgeHeight * 0.5f + 0.03f, localZ - depth * 0.5f), new Vector3(width, edgeHeight, edge), marker);
            CreateVisual($"{name} Marker Back", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + edgeHeight * 0.5f + 0.03f, localZ + depth * 0.5f), new Vector3(width, edgeHeight, edge), marker);
            CreateVisual($"{name} Marker Left", PrimitiveType.Cube, root,
                new Vector3(-width * 0.5f, groundTop + edgeHeight * 0.5f + 0.03f, localZ), new Vector3(edge, edgeHeight, depth), marker);
            CreateVisual($"{name} Marker Right", PrimitiveType.Cube, root,
                new Vector3(width * 0.5f, groundTop + edgeHeight * 0.5f + 0.03f, localZ), new Vector3(edge, edgeHeight, depth), marker);

            GameObject zoneObject = new GameObject(name);
            zoneObject.transform.SetParent(root, false);
            zoneObject.transform.localPosition = new Vector3(0f, 0.95f, localZ);
            BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(width, 1.9f, depth);
            InteractionZone zone = zoneObject.AddComponent<InteractionZone>();
            Rigidbody body = zoneObject.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            GameObject anchorObject = new GameObject("Buffer Stack Anchor");
            anchorObject.transform.SetParent(zoneObject.transform, false);
            anchorObject.transform.localPosition = new Vector3(0f, -0.68f, 0f);

            GameObject countObject = new GameObject("Buffer Count");
            countObject.transform.SetParent(zoneObject.transform, false);
            countObject.transform.localPosition = new Vector3(0f, 0.08f, -depth * 0.5f + 0.12f);
            countObject.transform.localRotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = countObject.AddComponent<TextMesh>();
            label.text = $"{labelText}  0/12";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.105f;
            label.fontSize = 42;
            label.color = new Color(0.10f, 0.13f, 0.13f);
            label.fontStyle = FontStyle.Bold;
            return zone;
        }

        private static GameObject CreateVisual(string name, PrimitiveType shape, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(shape);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return visual;
        }

        private static void CreateWorldLabel(string text, Vector3 position)
        {
            GameObject labelObject = new GameObject(text + " Label");
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.18f;
            label.fontSize = 42;
            label.color = new Color(0.12f, 0.14f, 0.13f);
            label.fontStyle = FontStyle.Bold;
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

        private static void MakeUnlit(Material material)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                return;

            material.shader = shader;
            EditorUtility.SetDirty(material);
        }

        private static void RemoveObject(string name)
        {
            GameObject target = GameObject.Find(name);
            if (target != null)
                Object.DestroyImmediate(target);
        }
    }
}
