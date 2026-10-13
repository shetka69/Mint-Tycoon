using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Imports clean FBX exports, shared URP materials, and bottom-pivot visual prefabs.</summary>
    public static class GameModelImportSetup
    {
        public const string ModelFolder = "Assets/_MintTycoon/Art/Models/Imported";
        public const string PrefabFolder = "Assets/_MintTycoon/Art/Prefabs/Imported";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials/Imported";
        [Serializable] private class Manifest { public Model[] models; public MaterialData[] materials; }
        [Serializable] private class Model { public string name, file; public Mapping[] materials; }
        [Serializable] private class Mapping { public string sourceName, key; }
        [Serializable] private class MaterialData { public string key; public float[] color, emission; public float roughness, metallic, emissionStrength; }
        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        [MenuItem("Mint Tycoon/Art/Import Model Assets")]
        public static void ImportAssets()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before importing art.");
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(ModelFolder + "/model_manifest.json");
            if (json == null) throw new MissingReferenceException("Run Tools/export_game_models.py in Blender first.");
            Manifest manifest = JsonUtility.FromJson<Manifest>(json.text);
            EnsureFolder(MaterialFolder); EnsureFolder(PrefabFolder);
            materials.Clear();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new MissingReferenceException("URP Lit shader is unavailable.");
            foreach (MaterialData item in manifest.materials)
            {
                string path = MaterialFolder + "/" + item.key + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
                material.shader = shader;
                Color baseColor = ReadColor(item.color).gamma; baseColor.a = item.color[3];
                material.SetColor("_BaseColor", baseColor);
                material.SetFloat("_Smoothness", Mathf.Clamp01(1f - item.roughness));
                material.SetFloat("_Metallic", Mathf.Clamp01(item.metallic));
                if (item.emissionStrength > 0f)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", ReadColor(item.emission) * item.emissionStrength);
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                else { material.DisableKeyword("_EMISSION"); material.SetColor("_EmissionColor", Color.black); }
                EditorUtility.SetDirty(material); materials.Add(item.key, material);
            }
            Scene preview = EditorSceneManager.NewPreviewScene();
            List<string> report = new List<string>();
            try
            {
                foreach (Model item in manifest.models)
                {
                    string path = ModelFolder + "/" + item.file;
                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null) throw new MissingReferenceException("FBX not imported: " + path);
                    importer.globalScale = 1f; importer.useFileScale = true; importer.bakeAxisConversion = true;
                    importer.importAnimation = false; importer.animationType = ModelImporterAnimationType.None;
                    importer.importCameras = false; importer.importLights = false; importer.importBlendShapes = false;
                    importer.importVisibility = false; importer.preserveHierarchy = true;
                    importer.isReadable = false; importer.addCollider = false;
                    importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.None;
                    importer.meshCompression = ModelImporterMeshCompression.Off;
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                    foreach (Mapping mapping in item.materials)
                        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mapping.sourceName), materials[mapping.key]);
                    importer.SaveAndReimport();
                    GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (model == null) throw new MissingReferenceException("FBX model is unavailable: " + path);
                    GameObject root = new GameObject(item.name + " Visual");
                    SceneManager.MoveGameObjectToScene(root, preview);
                    GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model, preview);
                    visual.transform.SetParent(root.transform, false);
                    visual.name = "Model";
                    Bounds bounds = BoundsOf(root.transform);
                    float scale = FitScale(item.name, bounds.size);
                    visual.transform.localScale *= scale;
                    if (item.name.StartsWith("UpgradePad"))
                    {
                        Vector3 platformScale = visual.transform.localScale;
                        platformScale.y *= 0.5f; // Keep the walkable stone lip below the controller's step height.
                        visual.transform.localScale = platformScale;
                    }
                    bounds = BoundsOf(root.transform);
                    visual.transform.localPosition -= Vector3.up * bounds.min.y;
                    foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                        foreach (Material material in renderer.sharedMaterials)
                            if (material == null || !material.shader.name.StartsWith("Universal Render Pipeline/"))
                                throw new InvalidOperationException("Material remap failed: " + item.name + "/" + renderer.name);
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + "/" + item.name + ".prefab");
                    bounds = BoundsOf(root.transform);
                    report.Add(item.name + ": size=" + bounds.size.ToString("F3") + ", baseY=" + bounds.min.y.ToString("F4"));
                    if (item.name == "Furnace" || item.name == "PlayerCharacter_Stickman" || item.name == "MintPress")
                        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                            if (Regex.IsMatch(child.name, "^(Eye_L|Arm_L|Leg_L|Fire_OrangeHearth|Press_Head|Head_GoldDie|Screw_Shaft)$"))
                                report.Add("  " + child.name + " position=" + child.position.ToString("F3") + " rotation=" + child.localEulerAngles.ToString("F1"));
                    Object.DestroyImmediate(root);
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Temp/ModelImport");
            File.WriteAllLines("Temp/ModelImport/unity_import_report.txt", report);
            Debug.Log("Mint Tycoon art imported: " + manifest.models.Length + " normalized visual prefabs and " + materials.Count + " shared URP materials.");
        }

        public static GameObject Prefab(string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + name + ".prefab");
            if (prefab == null) throw new MissingReferenceException("Imported visual prefab is missing: " + name);
            return prefab;
        }
        public static Bounds BoundsOf(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("No renderers in " + root.name);
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
            return bounds;
        }
        private static float FitScale(string name, Vector3 size)
        {
            if (name.StartsWith("CopperOre_")) return 0.27f / Mathf.Max(size.x, size.y, size.z);
            if (name == "CopperIngot") return 0.27f / size.x;
            if (name == "CopperCoin") return 0.26f / Mathf.Max(size.x, size.z);
            if (name.StartsWith("CoinStack_")) return 0.56f / Mathf.Max(size.x, size.z);
            if (name == "PlayerCharacter_Stickman") return 1.6f / size.y;
            if (name == "Barrel") return 0.70f / size.x;
            if (name == "WoodenCrate") return 0.76f / Mathf.Max(size.x, size.z);
            if (name == "AnvilWorkbench") return 1.45f / size.x;
            if (name == "OrderSign") return 1.12f / size.x;
            if (name.StartsWith("UpgradePad")) return 2.0f / Mathf.Max(size.x, size.z);
            if (name == "CopperMine") return Mathf.Min(2.9f / size.x, 2.5f / size.z);
            if (name == "Furnace") return Mathf.Min(2.65f / size.x, 2.35f / size.z);
            if (name == "MintPress") return Mathf.Min(2.3f / size.x, 2.2f / size.z);
            if (name == "Conveyor") return Mathf.Min(1.2f / size.x, 2.6f / size.z);
            return 1f;
        }
        private static Color ReadColor(float[] values) => new Color(values[0], values[1], values[2], values[3]);
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
