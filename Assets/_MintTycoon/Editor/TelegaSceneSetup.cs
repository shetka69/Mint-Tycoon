using System;
using System.IO;
using System.Linq;
using MintTycoon.Orders;
using MintTycoon.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Creates an animated cart and a separate stationary delivery station.</summary>
    public static class TelegaSceneSetup
    {
        private const string ModelPath = "Assets/_MintTycoon/Art/Models/Telega/Telega.fbx";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials/Telega";
        private const string SourceFolder = "Assets/Obj/Telega_FBX_ready";
        public const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Telega/Telega.prefab";
        public const string StationName = "Royal Delivery Station";
        [Serializable] private class ExportManifest { public ExportMesh[] meshes; }
        [Serializable] private class ExportMesh { public string[] materials; }
        private static readonly string[][] WheelParts = {
            new[] { "Cube.003", "Cube.010", "Cube.013" },
            new[] { "Cube.012", "Cube.009", "Cube.011" },
            new[] { "Cube.015", "Cube.016", "Cube.018" },
            new[] { "Cube.019", "Cube.014", "Cube.017" }
        };

        [MenuItem("Mint Tycoon/Art/Set Up Independent Telega")]
        public static void SetUp()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before changing the cart.");
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity") throw new InvalidOperationException("Open the greybox scene first.");
            BuildPrefab();
            ApplyToScene();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Selection.activeGameObject = GameObject.Find("Royal Wagon");
            SceneView.lastActiveSceneView?.FrameSelected();
            Directory.CreateDirectory("Logs/Telega");
            File.WriteAllText("Logs/Telega/import_report.txt", "Saved independent Telega: 4 wheel pivots with hubs, preserved Wood UV1, separate stationary delivery zone and order sign, existing order/route references retained.\n");
            Debug.Log("Mint Tycoon: artist Telega saved; delivery pad and order sign are on an independent stationary root.");
        }

        public static void BatchSetUp()
        {
            EditorSceneManager.OpenScene("Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity", OpenSceneMode.Single);
            SetUp();
        }

        private static void BuildPrefab()
        {
            GameModelImportSetup.EnsureFolder(MaterialFolder);
            GameModelImportSetup.EnsureFolder(Path.GetDirectoryName(PrefabPath).Replace('\\', '/'));
            ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException("Run Tools/export_telega.py with Blender first.");
            importer.globalScale = 1f; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.importAnimation = false; importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false; importer.importLights = false; importer.importBlendShapes = false;
            importer.generateSecondaryUV = false; importer.isReadable = false; importer.addCollider = false;
            importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            Material wood = Material("TelegaWood", "Telega/Wood UV1", "Group 79.png", true);
            Material wheel = Material("TelegaWheels", "Universal Render Pipeline/Lit", "wheel_1.png", false);
            Material metal = Material("TelegaFittings", "Universal Render Pipeline/Lit", "Nakonechnik.png", false);
            ExportManifest manifest = JsonUtility.FromJson<ExportManifest>(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_MintTycoon/Art/Models/Telega/manifest.json").text);
            foreach (string name in manifest.meshes.SelectMany(m => m.materials).Distinct())
            {
                Material target = name == "Telega_Stylized_Wood" ? wood :
                    new[] { "Material.006", "Material.008", "Material.012", "Material.013", "Material.018", "Material.022" }.Contains(name) ? wheel : metal;
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), target);
            }
            importer.SaveAndReimport();
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject root = new GameObject("Telega"); SceneManager.MoveGameObjectToScene(root, preview);
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath), preview);
                PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                model.transform.SetParent(root.transform, false);
                Bounds bounds = GameModelImportSetup.BoundsOf(model.transform);
                model.transform.localScale *= 2.3f / bounds.size.x;
                bounds = GameModelImportSetup.BoundsOf(model.transform);
                // Centre the two axles, not the long handles, on the route's turning point.
                Vector3 axleCentre = Vector3.zero;
                foreach (string[] parts in WheelParts) axleCentre += BoundsOf(Find(model.transform, parts[0])).center;
                axleCentre /= WheelParts.Length;
                model.transform.localPosition -= new Vector3(axleCentre.x, bounds.min.y, axleCentre.z);
                Transform chassis = Child("Chassis", root.transform);
                Transform[] pivots = new Transform[WheelParts.Length];
                float radius = 0f;
                for (int i = 0; i < WheelParts.Length; i++)
                {
                    Bounds rim = BoundsOf(Find(model.transform, WheelParts[i][0]));
                    Transform pivot = Child("Wheel Pivot " + (i + 1), root.transform);
                    pivot.position = rim.center; pivots[i] = pivot; radius += rim.size.y * 0.5f;
                    foreach (string part in WheelParts[i]) Find(model.transform, part).SetParent(pivot, true);
                }
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>().ToArray()) renderer.transform.SetParent(chassis, true);
                Object.DestroyImmediate(model);
                Transform floor = Find(chassis, "Cube.001");
                Transform cargo = Child("Cargo Anchor", chassis);
                Bounds floorBounds = BoundsOf(floor);
                cargo.position = new Vector3(floorBounds.center.x, floorBounds.max.y + 0.015f, floorBounds.center.z);
                WagonWheelView wheels = root.AddComponent<WagonWheelView>();
                SerializedObject data = new SerializedObject(wheels);
                SerializedProperty array = data.FindProperty("wheels"); array.arraySize = pivots.Length;
                for (int i = 0; i < pivots.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = pivots[i];
                data.FindProperty("radius").floatValue = radius / pivots.Length;
                data.FindProperty("localAxis").vector3Value = Vector3.right;
                data.FindProperty("rotationSign").floatValue = 1f; data.ApplyModifiedPropertiesWithoutUndo();
                WagonSuspensionView suspension = root.AddComponent<WagonSuspensionView>();
                data = new SerializedObject(suspension); data.FindProperty("chassis").objectReferenceValue = chassis; data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private static Material Material(string name, string shaderName, string textureName, bool wood)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null) throw new MissingReferenceException(shaderName);
            string texturePath = SourceFolder + "/Textures/" + textureName;
            TextureImporter texture = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (texture == null) throw new FileNotFoundException(texturePath);
            texture.maxTextureSize = 1024; texture.mipmapEnabled = true; texture.sRGBTexture = true;
            texture.textureCompression = TextureImporterCompression.Compressed; texture.SaveAndReimport();
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader; material.SetTexture(wood ? "_MainTex" : "_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetColor(wood ? "_Color" : "_BaseColor", Color.white); material.SetFloat("_Smoothness", 0.22f);
            EditorUtility.SetDirty(material); return material;
        }

        public static Transform DeliveryTarget(Transform wagon) => GameObject.Find(StationName)?.transform.Find("Coin Delivery Zone") ?? wagon.Find("Coin Delivery Zone");
        public static TextMesh OrderLabel(Transform wagon) => (GameObject.Find(StationName)?.transform.Find("Order Sign") ?? wagon.Find("Order Sign"))?.GetComponent<TextMesh>();

        public static void ApplyToScene()
        {
            Transform wagon = GameObject.Find("Royal Wagon")?.transform;
            if (wagon == null) throw new MissingReferenceException("Royal Wagon is missing.");
            if (PrefabUtility.IsPartOfPrefabInstance(wagon))
                PrefabUtility.UnpackPrefabInstance(wagon.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            Transform station = GameObject.Find(StationName)?.transform;
            if (station == null)
            {
                station = new GameObject(StationName).transform;
                station.SetPositionAndRotation(wagon.position, wagon.rotation); station.localScale = wagon.lossyScale;
            }
            // Keep the user's world placement of every pad, trigger, and board intact.
            foreach (Transform child in wagon.Cast<Transform>().ToArray())
                if (child.name.StartsWith("Coin Delivery", StringComparison.Ordinal) || child.name == "Imported Order Sign" || child.name == "Order Sign") child.SetParent(station, true);
            Transform board = station.Find("Imported Order Sign"), label = station.Find("Order Sign");
            if (board != null && label != null)
            {
                Transform surface = board.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Order_Display_Surface");
                if (surface != null)
                    label.SetPositionAndRotation(BoundsOf(surface).center + board.TransformVector(new Vector3(0f, -0.16f, -0.025f)), board.rotation);
            }
            foreach (string name in new[] { "Stage 9 Visuals", "Imported Visuals", "New Object Visuals", "Imported Cargo", "New Coin Cargo" })
            { Transform old = wagon.Find(name); if (old != null) old.gameObject.SetActive(false); }
            WagonWheelView previous = wagon.GetComponent<WagonWheelView>(); if (previous != null) previous.enabled = false;
            Transform holder = wagon.Find("Artist Telega");
            if (holder == null) holder = Child("Artist Telega", wagon);
            foreach (Transform old in holder.Cast<Transform>().ToArray()) Object.DestroyImmediate(old.gameObject);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Transform telega = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, holder)).transform;
            telega.localPosition = new Vector3(0f, 0.15f, 0f);
            ConfigureCargo(wagon, telega.Find("Chassis/Cargo Anchor"));
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(t)) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(t); PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
            }
        }

        private static void ConfigureCargo(Transform wagon, Transform anchor)
        {
            Transform cargo = Child("Order Cargo", anchor);
            int[] thresholds = { 1, 3, 6, 10 };
            GameObject[] stages = new GameObject[thresholds.Length];
            for (int i = 0; i < thresholds.Length; i++)
            {
                Transform stage = Child("Loaded " + thresholds[i], cargo); stages[i] = stage.gameObject;
                for (int count = 0; count < thresholds[i]; count++)
                {
                    Transform coin = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(NewObjectsSetup.Prefabs + "/Coin.prefab"), stage)).transform;
                    coin.localPosition = new Vector3(((count % 4) % 2 - 0.5f) * 0.28f, (count / 4) * 0.065f, ((count % 4) / 2 - 0.5f) * 0.28f);
                }
                stage.gameObject.SetActive(false);
            }
            OrderCargoView view = wagon.GetComponent<OrderCargoView>(); SerializedObject data = new SerializedObject(view);
            SerializedProperty array = data.FindProperty("cargoStages"), values = data.FindProperty("loadedThresholds");
            array.arraySize = values.arraySize = thresholds.Length;
            for (int i = 0; i < thresholds.Length; i++) { array.GetArrayElementAtIndex(i).objectReferenceValue = stages[i]; values.GetArrayElementAtIndex(i).intValue = thresholds[i]; }
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform Child(string name, Transform parent) { Transform t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        private static Transform Find(Transform root, string name) => root.GetComponentsInChildren<Transform>().First(t => t.name == name);
        private static Bounds BoundsOf(Transform t) => GameModelImportSetup.BoundsOf(t);
    }
}
