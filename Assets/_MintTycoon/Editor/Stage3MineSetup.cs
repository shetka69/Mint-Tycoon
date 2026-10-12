using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Resources;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage3MineSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string MinePrefabPath = "Assets/_MintTycoon/Art/Prefabs/Stations/CopperMine.prefab";
        private const string ResourcePath = "Assets/_MintTycoon/Configs/Resources/CopperOre.asset";

        [MenuItem("Mint Tycoon/Stage 3/Set Up Copper Mine")]
        public static void SetUp()
        {
            EnsureFolder("Assets/_MintTycoon/Art/Prefabs", "Stations");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Copper Mine");
            RemoveObject("MINE BLOCKOUT");
            RemoveObject("MINE BLOCKOUT Label");
            RemoveObject("COPPER MINE Label");

            ResourceDefinition copperOre = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(ResourcePath);
            if (copperOre == null)
                throw new MissingReferenceException($"Missing resource definition at {ResourcePath}. Complete Stage 2 setup first.");

            Material stone = LoadOrCreateMaterial("Mine_Stone", new Color(0.32f, 0.34f, 0.32f));
            Material timber = LoadOrCreateMaterial("Mine_Timber", new Color(0.35f, 0.20f, 0.11f));
            Material tunnel = LoadOrCreateMaterial("Mine_Tunnel", new Color(0.12f, 0.13f, 0.12f));
            Material copper = LoadOrCreateMaterial("Mine_CopperVein", new Color(0.70f, 0.33f, 0.16f));
            Material collectionMarker = LoadOrCreateMaterial("Mine_CollectionMarker", new Color(0.95f, 0.48f, 0.16f));
            Material collectionHighlight = LoadOrCreateMaterial("Mine_CollectionHighlight", new Color(0.95f, 0.70f, 0.30f));
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader != null)
            {
                collectionHighlight.shader = unlitShader;
                EditorUtility.SetDirty(collectionHighlight);
            }

            GameObject mine = new GameObject("Copper Mine");
            mine.transform.position = new Vector3(-3.5f, 0f, 4.5f);
            BuildMineVisual(mine.transform, stone, timber, tunnel, copper);
            BuildCollectionZone(mine.transform, copperOre, collectionMarker, collectionHighlight);

            PrefabUtility.SaveAsPrefabAssetAndConnect(mine, MinePrefabPath, InteractionMode.AutomatedAction);
            CreateMineLabel(mine.transform.position + Vector3.up * 2.35f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = mine;
            Debug.Log("Mint Tycoon Stage 3 copper mine is ready: one copper ore every 0.4 seconds while a compatible inventory remains inside the zone.");
        }

        private static void BuildMineVisual(Transform root, Material stone, Material timber, Material tunnel, Material copper)
        {
            CreateVisual("Rock Face", PrimitiveType.Cube, root, new Vector3(0f, 0.92f, 0.62f), new Vector3(2.55f, 1.95f, 0.38f), stone);
            CreateVisual("Mine Opening", PrimitiveType.Cube, root, new Vector3(0f, 0.86f, 0.39f), new Vector3(1.48f, 1.46f, 0.12f), tunnel);
            CreateVisual("Left Timber Support", PrimitiveType.Cube, root, new Vector3(-0.84f, 0.98f, 0.22f), new Vector3(0.18f, 1.98f, 0.22f), timber);
            CreateVisual("Right Timber Support", PrimitiveType.Cube, root, new Vector3(0.84f, 0.98f, 0.22f), new Vector3(0.18f, 1.98f, 0.22f), timber);
            CreateVisual("Timber Header", PrimitiveType.Cube, root, new Vector3(0f, 1.96f, 0.22f), new Vector3(1.88f, 0.22f, 0.24f), timber);
            CreateVisual("Copper Vein Left", PrimitiveType.Sphere, root, new Vector3(-1.03f, 0.32f, 0.02f), new Vector3(0.48f, 0.36f, 0.34f), copper);
            CreateVisual("Copper Vein Right", PrimitiveType.Sphere, root, new Vector3(1.00f, 0.30f, 0.12f), new Vector3(0.43f, 0.34f, 0.32f), copper);
        }

        private static void BuildCollectionZone(Transform root, ResourceDefinition resource, Material markerMaterial, Material highlightMaterial)
        {
            // A colored floor pad and raised border make the invisible trigger legible in Game view.
            // Keep the pad ahead of the mine face so the station does not hide the zone markers.
            const float zoneWidth = 3.2f;
            const float zoneDepth = 2.6f;
            const float zoneCenterZ = -1.3f;
            const float markerThickness = 0.12f;
            const float markerHeight = 0.06f;
            const float groundTop = 0.15f;
            CreateVisual("Collection Zone Highlight", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + 0.02f, zoneCenterZ),
                new Vector3(zoneWidth - 0.16f, 0.035f, zoneDepth - 0.16f), highlightMaterial);
            CreateVisual("Collection Zone Marker Front", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + markerHeight * 0.5f + 0.03f, zoneCenterZ - zoneDepth * 0.5f),
                new Vector3(zoneWidth, markerHeight, markerThickness), markerMaterial);
            CreateVisual("Collection Zone Marker Back", PrimitiveType.Cube, root,
                new Vector3(0f, groundTop + markerHeight * 0.5f + 0.03f, zoneCenterZ + zoneDepth * 0.5f),
                new Vector3(zoneWidth, markerHeight, markerThickness), markerMaterial);
            CreateVisual("Collection Zone Marker Left", PrimitiveType.Cube, root,
                new Vector3(-zoneWidth * 0.5f, groundTop + markerHeight * 0.5f + 0.03f, zoneCenterZ),
                new Vector3(markerThickness, markerHeight, zoneDepth), markerMaterial);
            CreateVisual("Collection Zone Marker Right", PrimitiveType.Cube, root,
                new Vector3(zoneWidth * 0.5f, groundTop + markerHeight * 0.5f + 0.03f, zoneCenterZ),
                new Vector3(markerThickness, markerHeight, zoneDepth), markerMaterial);

            GameObject zoneObject = new GameObject("Mine Collection Zone");
            zoneObject.transform.SetParent(root, false);
            zoneObject.transform.localPosition = new Vector3(0f, 0.95f, zoneCenterZ);
            BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(zoneWidth, 1.9f, zoneDepth);
            InteractionZone zone = zoneObject.AddComponent<InteractionZone>();
            Rigidbody triggerBody = zoneObject.GetComponent<Rigidbody>();
            triggerBody.isKinematic = true;
            triggerBody.useGravity = false;

            MiningStation station = root.gameObject.AddComponent<MiningStation>();
            SerializedObject serialized = new SerializedObject(station);
            serialized.FindProperty("outputResource").objectReferenceValue = resource;
            serialized.FindProperty("collectionZone").objectReferenceValue = zone;
            serialized.FindProperty("secondsPerUnit").floatValue = 0.4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(station);
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

        private static void CreateMineLabel(Vector3 position)
        {
            GameObject labelObject = new GameObject("COPPER MINE Label");
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = "COPPER MINE";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.2f;
            label.fontSize = 42;
            label.color = new Color(0.12f, 0.14f, 0.13f);
            label.fontStyle = FontStyle.Bold;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"Assets/_MintTycoon/Art/Materials/{name}.mat";
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

        private static void RemoveObject(string name)
        {
            GameObject target = GameObject.Find(name);
            if (target != null)
                Object.DestroyImmediate(target);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
