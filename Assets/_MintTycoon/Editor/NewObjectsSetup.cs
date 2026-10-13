using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MintTycoon.Configs;
using MintTycoon.Orders;
using MintTycoon.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Imports new supplied objects independently of the retained earlier model set.</summary>
    public static class NewObjectsSetup
    {
        private const string Models = "Assets/_MintTycoon/Art/Models/NewObjects";
        private const string Materials = "Assets/_MintTycoon/Art/Materials/NewObjects";
        public const string Prefabs = "Assets/_MintTycoon/Art/Prefabs/NewObjects";
        [Serializable] private class Manifest { public Model[] models; }
        [Serializable] private class Model { public string name, file; public MaterialData[] materials; public string[] textures; public int gameTriangles; }
        [Serializable] private class MaterialData { public string name; public float[] color; public float metallic, roughness; }

        [MenuItem("Mint Tycoon/Art/Import and Place New Objects")]
        public static void SetUp()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before changing art.");
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity") throw new InvalidOperationException("Open the greybox scene first.");
            TextAsset source = AssetDatabase.LoadAssetAtPath<TextAsset>(Models + "/manifest.json");
            if (source == null) throw new FileNotFoundException("Run Tools/export_new_objects.py with Blender first.");
            GameModelImportSetup.EnsureFolder(Materials); GameModelImportSetup.EnsureFolder(Prefabs);
            Manifest manifest = JsonUtility.FromJson<Manifest>(source.text);
            foreach (Model model in manifest.models) Import(model);
            ApplyToScene();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Temp/NewObjects");
            File.WriteAllText("Temp/NewObjects/import_report.txt", string.Join("\n", manifest.models.Select(m => $"{m.name}: {m.gameTriangles} triangles, normalized prefab and URP materials")));
            Selection.activeTransform = Require("Royal Wagon").Find("New Object Visuals");
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("Mint Tycoon: new mine, furnace, coin, copper ore, and wagon saved; earlier visuals retained inactive.");
        }

        private static void Import(Model model)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(model.file);
            importer.globalScale = 1f; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.importCameras = false; importer.importLights = false; importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None; importer.importBlendShapes = false;
            importer.addCollider = false; importer.isReadable = false; importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            foreach (MaterialData data in model.materials)
            {
                string path = Materials + "/" + model.name + "_" + data.name + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
                material.shader = shader;
                Color color = new Color(data.color[0], data.color[1], data.color[2], 1f).gamma;
                material.SetColor("_BaseColor", model.textures.Length > 0 ? Color.white : color);
                if (model.name == "CopperOre") material.SetColor("_BaseColor", new Color(0.9f, 0.52f, 0.27f));
                material.SetFloat("_Metallic", model.name == "Coin" ? 0.55f : model.name == "CopperOre" ? 0.25f : data.metallic);
                material.SetFloat("_Smoothness", model.name == "CopperOre" ? 0.2f : Mathf.Clamp01(1f - data.roughness));
                if (model.textures.Length > 0) material.SetTexture("_BaseMap", Texture(model.textures[0], false));
                if (model.name == "Wagon")
                {
                    material.SetTexture("_BumpMap", Texture(model.textures[1], true)); material.EnableKeyword("_NORMALMAP");
                    material.SetTexture("_OcclusionMap", Texture(model.textures[2], false, false)); material.EnableKeyword("_OCCLUSIONMAP");
                    material.SetTexture("_MetallicGlossMap", Texture(model.textures[3], false, false));
                    material.EnableKeyword("_METALLICSPECGLOSSMAP"); material.SetFloat("_Smoothness", 1f);
                }
                EditorUtility.SetDirty(material);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), data.name), material);
            }
            importer.SaveAndReimport();
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject root = new GameObject(model.name); SceneManager.MoveGameObjectToScene(root, preview);
                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(model.file), preview);
                visual.transform.SetParent(root.transform, false);
                Bounds bounds = GameModelImportSetup.BoundsOf(visual.transform);
                float scale = model.name switch {
                    "Coin" => 0.26f / Mathf.Max(bounds.size.x, bounds.size.z),
                    "CopperOre" => 0.27f / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z),
                    "Mine" => Mathf.Min(3.1f / bounds.size.x, 2.35f / bounds.size.z),
                    "Furnace" => Mathf.Min(2.25f / bounds.size.x, 2.25f / bounds.size.z),
                    "Wagon" => Mathf.Min(2.3f / bounds.size.x, 3.1f / bounds.size.z),
                    _ => 1f
                };
                visual.transform.localScale *= scale;
                bounds = GameModelImportSetup.BoundsOf(visual.transform);
                visual.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                PrefabUtility.RecordPrefabInstancePropertyModifications(visual.transform);
                PrefabUtility.SaveAsPrefabAsset(root, Prefabs + "/" + model.name + ".prefab");
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private static Texture2D Texture(string path, bool normal, bool srgb = true)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal && srgb; importer.mipmapEnabled = true; importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static void ApplyToScene()
        {
            SetResource("CopperCoin", "Coin", 0.065f); SetResource("CopperOre", "CopperOre", 0.24f);
            Transform mine = Require("Copper Mine"), furnace = Require("Copper Furnace"), wagon = Require("Royal Wagon");
            Replace(mine, "Mine");
            Transform forge = Replace(furnace, "Furnace");
            Bounds bounds = GameModelImportSetup.BoundsOf(forge);
            Transform smoke = furnace.Find("Presentation Effects/Kiln smoke");
            Transform sparks = furnace.Find("Presentation Effects/Kiln sparks");
            if (smoke != null) smoke.position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            if (sparks != null) sparks.position = bounds.center + new Vector3(0f, -bounds.size.y * 0.22f, -bounds.size.z * 0.25f);
            ConfigureBody(furnace, new Vector3(0f, bounds.size.y * 0.5f + 0.15f, 0f), new Vector3(bounds.size.x * 0.9f, bounds.size.y, bounds.size.z * 0.85f));
            ConfigureBody(mine, new Vector3(0f, 1.4f, 0.55f), new Vector3(2.7f, 2.5f, 1.05f));
            Transform cart = Replace(wagon, "Wagon");
            ConfigureCart(wagon, cart);
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(t)) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
            }
        }

        private static void SetResource(string asset, string visual, float spacing)
        {
            ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/_MintTycoon/Configs/Resources/" + asset + ".asset");
            SerializedObject data = new SerializedObject(definition);
            data.FindProperty("stackVisualPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/" + visual + ".prefab");
            data.FindProperty("stackVisualVariants").arraySize = 0; data.FindProperty("stackRowSpacing").floatValue = spacing;
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(definition);
        }

        private static Transform Replace(Transform root, string model)
        {
            foreach (string name in new[] { "Stage 9 Visuals", "Imported Visuals" })
            { Transform old = root.Find(name); if (old != null) old.gameObject.SetActive(false); }
            Transform container = root.Find("New Object Visuals");
            if (container == null) { container = new GameObject("New Object Visuals").transform; container.SetParent(root, false); }
            foreach (Transform child in container.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            GameObject modelObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/" + model + ".prefab"), container);
            modelObject.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            return modelObject.transform;
        }

        private static void ConfigureBody(Transform root, Vector3 center, Vector3 size)
        {
            BoxCollider body = root.Find("Station Body Collider").GetComponent<BoxCollider>();
            body.center = center; body.size = size;
        }

        private static void ConfigureCart(Transform wagon, Transform model)
        {
            Transform sign = wagon.Find("Imported Order Sign");
            if (sign != null)
            {
                sign.localPosition = new Vector3(0f, 1.05f, -0.86f);
                Transform surface = sign.GetComponentsInChildren<Transform>().First(t => t.name == "Order_Display_Surface");
                Transform label = wagon.Find("Order Sign");
                if (label != null) label.position = GameModelImportSetup.BoundsOf(surface).center + new Vector3(0f, -0.16f, -0.025f);
            }
            Transform oldCargo = wagon.Find("Imported Cargo"); if (oldCargo != null) oldCargo.gameObject.SetActive(false);
            Transform cargo = wagon.Find("New Coin Cargo");
            if (cargo == null) { cargo = new GameObject("New Coin Cargo").transform; cargo.SetParent(wagon, false); }
            foreach (Transform child in cargo.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            Bounds body = GameModelImportSetup.BoundsOf(model.GetComponentsInChildren<Transform>().First(t => t.name == "OPM0021_body"));
            int[] amounts = { 1, 3, 6, 10 }; List<GameObject> stages = new List<GameObject>();
            foreach (int amount in amounts)
            {
                GameObject stage = new GameObject("Loaded " + amount); stage.transform.SetParent(cargo, false);
                for (int index = 0; index < amount; index++)
                {
                    GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "/Coin.prefab"), stage.transform);
                    coin.transform.position = body.center + new Vector3(((index % 4) % 2 - 0.5f) * 0.28f, 0.065f * (index / 4), ((index % 4) / 2 - 0.5f) * 0.28f);
                }
                stage.SetActive(false); stages.Add(stage);
            }
            OrderCargoView view = wagon.GetComponent<OrderCargoView>(); SerializedObject data = new SerializedObject(view);
            SerializedProperty array = data.FindProperty("cargoStages"), thresholds = data.FindProperty("loadedThresholds");
            array.arraySize = thresholds.arraySize = amounts.Length;
            for (int i = 0; i < amounts.Length; i++) { array.GetArrayElementAtIndex(i).objectReferenceValue = stages[i]; thresholds.GetArrayElementAtIndex(i).intValue = amounts[i]; }
            data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            Transform[] wheels = model.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("OPM0021_wheel_")).ToArray();
            WagonWheelView wheelView = wagon.GetComponent<WagonWheelView>();
            SerializedObject wheelData = new SerializedObject(wheelView); array = wheelData.FindProperty("wheels"); array.arraySize = wheels.Length;
            for (int i = 0; i < wheels.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = wheels[i];
            // The imported wheels rotate around local X rather than the procedural wheel's Y.
            wheelData.FindProperty("localAxis").vector3Value = Vector3.right;
            wheelData.FindProperty("radius").floatValue = wheels.Length > 0 ? GameModelImportSetup.BoundsOf(wheels[0]).size.y * 0.5f : 0.38f;
            wheelData.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.RecordPrefabInstancePropertyModifications(wheelView);
        }
        private static Transform Require(string name) => GameObject.Find(name)?.transform ?? throw new MissingReferenceException(name);
    }
}
