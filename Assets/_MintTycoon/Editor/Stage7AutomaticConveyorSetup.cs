using System.Collections.Generic;
using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Production;
using MintTycoon.Upgrades;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage7AutomaticConveyorSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string UpgradePath = "Assets/_MintTycoon/Configs/Upgrades/AutomaticIngotConveyor.asset";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 7/Set Up Automatic Conveyor Upgrade")]
        public static void SetUp()
        {
            EnsureFolder("Assets/_MintTycoon/Configs/Upgrades");
            UpgradeDefinition upgrade = LoadOrCreateUpgrade();
            Material locked = LoadOrCreateMaterial("Conveyor_Locked", new Color(0.23f, 0.25f, 0.26f));
            Material active = LoadOrCreateMaterial("Conveyor_Active", new Color(0.79f, 0.53f, 0.16f));
            Material gold = LoadOrCreateMaterial("Conveyor_UpgradeGold", new Color(0.93f, 0.67f, 0.17f));
            Material dark = LoadOrCreateMaterial("Conveyor_UpgradeDark", new Color(0.17f, 0.13f, 0.08f));

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Automatic Ingot Conveyor");
            RemoveObject("Automatic Conveyor Upgrade");
            RemoveObject("UPGRADE PAD BLOCKOUT");
            RemoveObject("UPGRADE PAD BLOCKOUT Label");

            BufferedProductionStation furnace = GameObject.Find("Copper Furnace")?.GetComponent<BufferedProductionStation>();
            BufferedProductionStation press = GameObject.Find("Copper Mint Press")?.GetComponent<BufferedProductionStation>();
            if (furnace == null || press == null)
                throw new MissingReferenceException("Copper Furnace and Copper Mint Press must be set up before Stage 7.");

            GameObject belt = BuildConveyor(furnace, press, locked, active);
            BuildUpgradePad(upgrade, belt.GetComponent<ResourceConveyor>(), gold, dark);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = belt;
            Debug.Log("Mint Tycoon Stage 7 is ready: the 40-gold upgrade enables automatic ingot transfer from the furnace to the press.");
        }

        private static UpgradeDefinition LoadOrCreateUpgrade()
        {
            UpgradeDefinition upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(UpgradePath);
            if (upgrade == null)
            {
                upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
                AssetDatabase.CreateAsset(upgrade, UpgradePath);
            }

            SerializedObject serialized = new SerializedObject(upgrade);
            serialized.FindProperty("upgradeId").stringValue = "automatic_ingot_conveyor";
            serialized.FindProperty("displayName").stringValue = "Automatic Ingot Conveyor";
            serialized.FindProperty("goldCost").intValue = 40;
            serialized.FindProperty("description").stringValue = "Automatically transfers finished ingots from the furnace to the mint press when the press has room.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(upgrade);
            return upgrade;
        }

        private static GameObject BuildConveyor(BufferedProductionStation furnace, BufferedProductionStation press, Material locked, Material active)
        {
            GameObject root = new GameObject("Automatic Ingot Conveyor");
            root.transform.position = new Vector3(4.45f, 0f, 1.50f);

            List<Renderer> renderers = new List<Renderer>();
            AddVisual(root.transform, "Belt Deck", PrimitiveType.Cube, new Vector3(0f, 0.23f, 0f), new Vector3(0.92f, 0.26f, 2.55f), locked, renderers);
            AddVisual(root.transform, "Belt Left Rail", PrimitiveType.Cube, new Vector3(-0.46f, 0.40f, 0f), new Vector3(0.09f, 0.16f, 2.55f), locked, renderers);
            AddVisual(root.transform, "Belt Right Rail", PrimitiveType.Cube, new Vector3(0.46f, 0.40f, 0f), new Vector3(0.09f, 0.16f, 2.55f), locked, renderers);
            for (int index = 0; index < 7; index++)
                AddVisual(root.transform, $"Belt Roller {index + 1}", PrimitiveType.Cylinder,
                    new Vector3(0f, 0.39f, -1.02f + index * 0.34f), new Vector3(0.66f, 0.055f, 0.66f), locked, renderers, Quaternion.Euler(90f, 0f, 0f));

            AddVisual(root.transform, "Belt Direction Marker 1", PrimitiveType.Cube, new Vector3(0f, 0.405f, 0.42f), new Vector3(0.20f, 0.025f, 0.10f), active, renderers);
            AddVisual(root.transform, "Belt Direction Marker 2", PrimitiveType.Cube, new Vector3(0f, 0.405f, -0.05f), new Vector3(0.20f, 0.025f, 0.10f), active, renderers);
            AddVisual(root.transform, "Belt Direction Marker 3", PrimitiveType.Cube, new Vector3(0f, 0.405f, -0.52f), new Vector3(0.20f, 0.025f, 0.10f), active, renderers);

            ResourceConveyor conveyor = root.AddComponent<ResourceConveyor>();
            SerializedObject serializedConveyor = new SerializedObject(conveyor);
            serializedConveyor.FindProperty("source").objectReferenceValue = furnace;
            serializedConveyor.FindProperty("destination").objectReferenceValue = press;
            serializedConveyor.FindProperty("transferIntervalSeconds").floatValue = 0.12f;
            serializedConveyor.FindProperty("isUnlocked").boolValue = false;
            serializedConveyor.ApplyModifiedPropertiesWithoutUndo();

            ConveyorBeltView view = root.AddComponent<ConveyorBeltView>();
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("conveyor").objectReferenceValue = conveyor;
            SerializedProperty rendererArray = serializedView.FindProperty("beltRenderers");
            rendererArray.arraySize = renderers.Count;
            for (int index = 0; index < renderers.Count; index++)
                rendererArray.GetArrayElementAtIndex(index).objectReferenceValue = renderers[index];
            serializedView.FindProperty("lockedMaterial").objectReferenceValue = locked;
            serializedView.FindProperty("unlockedMaterial").objectReferenceValue = active;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
            return root;
        }

        private static void BuildUpgradePad(UpgradeDefinition upgrade, ResourceConveyor conveyor, Material gold, Material dark)
        {
            GameObject root = new GameObject("Automatic Conveyor Upgrade");
            root.transform.position = new Vector3(2.5f, 0f, -5.5f);
            AddVisual(root.transform, "Upgrade Pad", PrimitiveType.Cube, new Vector3(0f, 0.12f, 0f), new Vector3(2.8f, 0.22f, 2.2f), gold);
            AddVisual(root.transform, "Upgrade Pedestal", PrimitiveType.Cube, new Vector3(0f, 0.48f, 0f), new Vector3(0.78f, 0.50f, 0.62f), dark);
            AddVisual(root.transform, "Upgrade Emblem", PrimitiveType.Cylinder, new Vector3(0f, 0.78f, 0f), new Vector3(0.39f, 0.08f, 0.39f), gold, null, Quaternion.Euler(90f, 0f, 0f));

            GameObject signObject = new GameObject("Upgrade Sign");
            signObject.transform.SetParent(root.transform, false);
            signObject.transform.localPosition = new Vector3(0f, 1.42f, 0f);
            signObject.transform.localRotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh sign = signObject.AddComponent<TextMesh>();
            sign.text = "AUTO INGOT BELT\n40 GOLD";
            sign.anchor = TextAnchor.MiddleCenter;
            sign.alignment = TextAlignment.Center;
            sign.characterSize = 0.13f;
            sign.fontSize = 48;
            sign.color = new Color(0.16f, 0.12f, 0.07f);
            sign.fontStyle = FontStyle.Bold;

            GameObject zoneObject = new GameObject("Purchase Zone");
            zoneObject.transform.SetParent(root.transform, false);
            zoneObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.8f, 1.8f, 2.2f);
            InteractionZone zone = zoneObject.AddComponent<InteractionZone>();
            Rigidbody body = zoneObject.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            UpgradePurchaseZone purchase = root.AddComponent<UpgradePurchaseZone>();
            SerializedObject serializedPurchase = new SerializedObject(purchase);
            serializedPurchase.FindProperty("interactionZone").objectReferenceValue = zone;
            serializedPurchase.FindProperty("upgrade").objectReferenceValue = upgrade;
            serializedPurchase.FindProperty("conveyor").objectReferenceValue = conveyor;
            serializedPurchase.FindProperty("sign").objectReferenceValue = sign;
            serializedPurchase.FindProperty("purchased").boolValue = false;
            serializedPurchase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
        }

        private static GameObject AddVisual(Transform parent, string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material,
            List<Renderer> rendererCollection = null, Quaternion? rotation = null)
        {
            GameObject visual = GameObject.CreatePrimitive(shape);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = position;
            visual.transform.localScale = scale;
            if (rotation.HasValue)
                visual.transform.localRotation = rotation.Value;
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            if (rendererCollection != null && renderer != null)
                rendererCollection.Add(renderer);
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return visual;
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

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;
            string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void RemoveObject(string name)
        {
            GameObject target = GameObject.Find(name);
            if (target != null)
                Object.DestroyImmediate(target);
        }
    }
}
