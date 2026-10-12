using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage4FurnaceSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Stations/CopperFurnace.prefab";
        private const string RecipePath = "Assets/_MintTycoon/Configs/Recipes/CopperSmelting.asset";
        private const string OrePath = "Assets/_MintTycoon/Configs/Resources/CopperOre.asset";
        private const string IngotPath = "Assets/_MintTycoon/Configs/Resources/CopperIngot.asset";
        private const string PrefabFolder = "Assets/_MintTycoon/Art/Prefabs/Stations";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 4/Set Up Copper Furnace")]
        public static void SetUp()
        {
            ResourceDefinition ore = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(OrePath);
            ResourceDefinition ingot = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(IngotPath);
            if (ore == null || ingot == null)
                throw new MissingReferenceException("Missing copper ore or ingot definition. Complete Stage 2 setup first.");

            ProductionRecipeDefinition recipe = LoadOrCreateRecipe(ore, ingot);
            Material stone = LoadOrCreateMaterial("Furnace_Stone", new Color(0.30f, 0.32f, 0.31f));
            Material chamber = LoadOrCreateMaterial("Furnace_Chamber", new Color(0.10f, 0.12f, 0.12f));
            Material copper = LoadOrCreateMaterial("Furnace_Copper", new Color(0.78f, 0.35f, 0.16f));
            Material inputPad = LoadOrCreateMaterial("Furnace_InputZone", new Color(0.96f, 0.52f, 0.17f));
            Material outputPad = LoadOrCreateMaterial("Furnace_OutputZone", new Color(0.20f, 0.68f, 0.69f));
            MakeUnlit(inputPad);
            MakeUnlit(outputPad);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Copper Furnace");
            RemoveObject("FURNACE BLOCKOUT");
            RemoveObject("FURNACE BLOCKOUT Label");
            RemoveObject("COPPER FURNACE Label");

            GameObject furnace = new GameObject("Copper Furnace");
            furnace.transform.position = new Vector3(2.5f, 0f, 4.5f);
            BuildFurnaceVisual(furnace.transform, stone, chamber, copper);
            InteractionZone inputZone = BuildBufferZone(furnace.transform, "Input Zone", -1.95f, inputPad, "INPUT  ORE");
            InteractionZone outputZone = BuildBufferZone(furnace.transform, "Output Zone", 1.95f, outputPad, "OUTPUT  INGOTS");

            FurnaceStation station = furnace.AddComponent<FurnaceStation>();
            SerializedObject serializedStation = new SerializedObject(station);
            serializedStation.FindProperty("recipe").objectReferenceValue = recipe;
            serializedStation.FindProperty("inputZone").objectReferenceValue = inputZone;
            serializedStation.FindProperty("outputZone").objectReferenceValue = outputZone;
            serializedStation.FindProperty("inputCapacity").intValue = 12;
            serializedStation.FindProperty("outputCapacity").intValue = 12;
            serializedStation.FindProperty("transferIntervalSeconds").floatValue = 0.12f;
            serializedStation.ApplyModifiedPropertiesWithoutUndo();

            FurnaceBufferStackView view = furnace.AddComponent<FurnaceBufferStackView>();
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("station").objectReferenceValue = station;
            serializedView.FindProperty("inputResource").objectReferenceValue = ore;
            serializedView.FindProperty("outputResource").objectReferenceValue = ingot;
            serializedView.FindProperty("inputAnchor").objectReferenceValue = inputZone.transform.Find("Buffer Stack Anchor");
            serializedView.FindProperty("outputAnchor").objectReferenceValue = outputZone.transform.Find("Buffer Stack Anchor");
            serializedView.FindProperty("inputCountLabel").objectReferenceValue = inputZone.transform.Find("Buffer Count").GetComponent<TextMesh>();
            serializedView.FindProperty("outputCountLabel").objectReferenceValue = outputZone.transform.Find("Buffer Count").GetComponent<TextMesh>();
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAssetAndConnect(furnace, PrefabPath, InteractionMode.AutomatedAction);
            CreateWorldLabel("COPPER FURNACE", furnace.transform.position + Vector3.up * 2.65f);
            EditorUtility.SetDirty(furnace);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = furnace;
            Debug.Log("Mint Tycoon Stage 4 furnace is ready: 1 ore becomes 1 ingot every 0.5 seconds, with separate 12-unit buffers and 0.12-second automatic transfers.");
        }

        private static ProductionRecipeDefinition LoadOrCreateRecipe(ResourceDefinition ore, ResourceDefinition ingot)
        {
            ProductionRecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<ProductionRecipeDefinition>(RecipePath);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<ProductionRecipeDefinition>();
                AssetDatabase.CreateAsset(recipe, RecipePath);
            }

            SerializedObject serialized = new SerializedObject(recipe);
            serialized.FindProperty("input").objectReferenceValue = ore;
            serialized.FindProperty("inputAmount").intValue = 1;
            serialized.FindProperty("output").objectReferenceValue = ingot;
            serialized.FindProperty("outputAmount").intValue = 1;
            serialized.FindProperty("cycleDurationSeconds").floatValue = 0.5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void BuildFurnaceVisual(Transform root, Material stone, Material chamber, Material copper)
        {
            CreateVisual("Furnace Base", PrimitiveType.Cube, root, new Vector3(0f, 0.32f, 0.20f), new Vector3(2.55f, 0.64f, 1.50f), stone);
            CreateVisual("Furnace Body", PrimitiveType.Cube, root, new Vector3(0f, 1.16f, 0.28f), new Vector3(2.28f, 1.10f, 1.30f), stone);
            CreateVisual("Furnace Crown", PrimitiveType.Cube, root, new Vector3(0f, 1.81f, 0.34f), new Vector3(2.46f, 0.22f, 1.43f), copper);
            CreateVisual("Furnace Firebox", PrimitiveType.Cube, root, new Vector3(0f, 1.05f, -0.385f), new Vector3(1.18f, 0.74f, 0.10f), chamber);
            CreateVisual("Firebox Glow", PrimitiveType.Cube, root, new Vector3(0f, 0.94f, -0.45f), new Vector3(0.42f, 0.20f, 0.025f), copper);
            CreateVisual("Chimney", PrimitiveType.Cube, root, new Vector3(0.72f, 2.35f, 0.42f), new Vector3(0.50f, 0.95f, 0.56f), chamber);
            CreateVisual("Chimney Rim", PrimitiveType.Cube, root, new Vector3(0.72f, 2.84f, 0.42f), new Vector3(0.68f, 0.16f, 0.72f), copper);
            CreateVisual("Furnace Emblem", PrimitiveType.Sphere, root, new Vector3(-0.78f, 1.31f, -0.405f), new Vector3(0.24f, 0.24f, 0.09f), copper);
        }

        private static InteractionZone BuildBufferZone(Transform root, string name, float localZ, Material marker, string labelText)
        {
            const float width = 2.35f;
            const float depth = 1.45f;
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
            label.characterSize = 0.2f;
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
