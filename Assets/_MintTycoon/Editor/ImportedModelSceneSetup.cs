using System;
using System.Collections.Generic;
using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Orders;
using MintTycoon.Presentation;
using MintTycoon.Production;
using MintTycoon.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Fits authored visuals to existing gameplay roots without replacing their logic.</summary>
    public static class ImportedModelSceneSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";

        [MenuItem("Mint Tycoon/Art/Use Imported Models in Scene")]
        public static void SetUp()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before changing scene art.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GameModelImportSetup.PrefabFolder + "/Furnace.prefab") == null)
                GameModelImportSetup.ImportAssets();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetResource("CopperOre", "CopperOre_01", 0.24f, new[] { "CopperOre_01", "CopperOre_02", "CopperOre_03" });
            SetResource("CopperIngot", "CopperIngot", 0.085f, null);
            SetResource("CopperCoin", "CopperCoin", 0.045f, null);
            Transform mine = Require("Copper Mine"), furnace = Require("Copper Furnace"), press = Require("Copper Mint Press");
            Transform player = Require("Player"), wagon = Require("Royal Wagon"), belt = Require("Automatic Ingot Conveyor");
            ReplaceVisual(mine, "CopperMine");
            ConfigureBodyCollider(mine, new Vector3(0f, 1.3f, 0.43f), new Vector3(2.1f, 2.4f, 0.95f));
            Transform furnaceVisual = ReplaceVisual(furnace, "Furnace");
            ConfigureBodyCollider(furnace, new Vector3(0f, 1.1f, 0.20f), new Vector3(1.6f, 2.2f, 1.5f));
            Transform pressVisual = ReplaceVisual(press, "MintPress");
            ConfigureBodyCollider(press, new Vector3(0f, 1.05f, 0f), new Vector3(1.65f, 2.1f, 1.20f));
            ConfigureFeedback(furnace, furnaceVisual, false);
            ConfigureFeedback(press, pressVisual, true);
            ConfigureWorker(player);
            ConfigureConveyor(belt);
            ConfigureUpgrade(Require("Automatic Conveyor Upgrade"), belt.GetComponent<ResourceConveyor>());
            ConfigureWagon(wagon);
            ConfigureDecor();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(NewObjectsSetup.Prefabs + "/Mine.prefab") != null)
                NewObjectsSetup.ApplyToScene();
            RecordSceneOverrides();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Mint Tycoon imported art placed: artist stations, worker, carried resources, conveyor, upgrade variants, order sign, cargo, and courtyard props. Gameplay roots and zones retained.");
        }

        private static void SetResource(string asset, string visual, float rowSpacing, string[] variants)
        {
            ResourceDefinition resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/_MintTycoon/Configs/Resources/" + asset + ".asset");
            if (resource == null) throw new MissingReferenceException("Resource definition missing: " + asset);
            SerializedObject data = new SerializedObject(resource);
            Assign(data, "stackVisualPrefab", GameModelImportSetup.Prefab(visual));
            data.FindProperty("stackRowSpacing").floatValue = rowSpacing;
            SerializedProperty array = data.FindProperty("stackVisualVariants"); array.arraySize = variants?.Length ?? 0;
            if (variants != null) for (int index = 0; index < variants.Length; index++) array.GetArrayElementAtIndex(index).objectReferenceValue = GameModelImportSetup.Prefab(variants[index]);
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(resource);
        }
        private static Transform ReplaceVisual(Transform root, string model)
        {
            // Move existing feedback particles to a stable root before hiding the old models.
            Transform legacy = root.Find("Stage 9 Visuals");
            if (legacy != null)
            {
                Transform effects = root.Find("Presentation Effects");
                if (effects == null) { effects = new GameObject("Presentation Effects").transform; effects.SetParent(root, false); }
                foreach (ParticleSystem system in legacy.GetComponentsInChildren<ParticleSystem>(true)) system.transform.SetParent(effects, true);
                legacy.gameObject.SetActive(false);
            }
            Transform container = ReplaceContainer(root, "Imported Visuals");
            Instantiate(model, container, new Vector3(0f, 0.15f, 0f));
            return container;
        }
        private static void ConfigureFeedback(Transform station, Transform model, bool isPress)
        {
            StationFeedback feedback = station.GetComponent<StationFeedback>();
            if (feedback == null) throw new MissingReferenceException("Station feedback is missing: " + station.name);
            SerializedObject data = new SerializedObject(feedback);
            Assign(data, "ram", isPress ? Find(model, "Press_Head") : null);
            Assign(data, "flywheel", isPress ? Find(model, "Press_Screw") : null);
            data.FindProperty("ramTravel").floatValue = isPress ? 0.28f : 0f;
            data.ApplyModifiedPropertiesWithoutUndo();
            Transform effects = station.Find("Presentation Effects");
            if (effects == null) return;
            Transform smoke = effects.Find("Kiln smoke");
            if (smoke != null)
            {
                Bounds chimney = GameModelImportSetup.BoundsOf(Find(model, "Chimney"));
                smoke.position = new Vector3(chimney.center.x, chimney.max.y + 0.08f, chimney.center.z);
            }
            Transform sparks = effects.Find(isPress ? "Mint strike sparks" : "Kiln sparks");
            if (sparks != null)
            {
                Transform point = Find(model, isPress ? "Coin_Holder" : "Fire_OrangeHearth");
                Bounds bounds = GameModelImportSetup.BoundsOf(point);
                sparks.position = bounds.center + new Vector3(0f, isPress ? 0.04f : 0f, -0.06f);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(feedback);
        }
        private static void ConfigureWorker(Transform player)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCharacterSetup.PrefabPath) != null)
            {
                PlayerCharacterSetup.ApplyToPlayer(player);
                return;
            }
            Transform old = player.Find("Stage 9 Visuals"); if (old != null) old.gameObject.SetActive(false);
            Transform root = ReplaceContainer(player, "Imported Visuals");
            Transform visual = Instantiate("PlayerCharacter_Stickman", root, Vector3.zero);
            visual.localRotation = Quaternion.Euler(0f, 180f, 0f); // Eyes face the movement component's +Z forward.
            PlayerWalkView view = player.GetComponent<PlayerWalkView>();
            SerializedObject data = new SerializedObject(view);
            Assign(data, "visualRoot", root);
            Assign(data, "leftArm", Find(root, "Arm_L")); Assign(data, "rightArm", Find(root, "Arm_R"));
            Assign(data, "leftLeg", Find(root, "Leg_L")); Assign(data, "rightLeg", Find(root, "Leg_R"));
            data.ApplyModifiedPropertiesWithoutUndo();
            Transform stack = player.Find("Inventory Stack Anchor"); if (stack != null) stack.localPosition = new Vector3(0f, 1.80f, -0.18f);
        }
        private static void ConfigureConveyor(Transform belt)
        {
            foreach (Renderer renderer in belt.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            Transform root = ReplaceContainer(belt, "Imported Visuals");
            Transform visual = Instantiate("Conveyor", root, new Vector3(0f, 0.15f, 0f));
            Renderer surface = Find(visual, "Belt_ContinuousLoop").GetComponent<Renderer>();
            if (surface == null) throw new MissingReferenceException("The conveyor's belt surface is missing.");
            Material active = surface.sharedMaterial;
            string lockedPath = "Assets/_MintTycoon/Art/Materials/Imported/Conveyor_Locked.mat";
            Material locked = AssetDatabase.LoadAssetAtPath<Material>(lockedPath);
            if (locked == null) { locked = new Material(active); AssetDatabase.CreateAsset(locked, lockedPath); }
            locked.SetColor("_BaseColor", new Color(0.12f, 0.14f, 0.15f)); EditorUtility.SetDirty(locked);
            ConveyorBeltView view = belt.GetComponent<ConveyorBeltView>();
            SerializedObject data = new SerializedObject(view);
            SerializedProperty array = data.FindProperty("beltRenderers"); array.arraySize = 1; array.GetArrayElementAtIndex(0).objectReferenceValue = surface;
            Assign(data, "lockedMaterial", locked); Assign(data, "unlockedMaterial", active); data.ApplyModifiedPropertiesWithoutUndo();
            surface.sharedMaterial = locked;
            ConveyorMotionView motion = belt.GetComponent<ConveyorMotionView>(); if (motion == null) motion = belt.gameObject.AddComponent<ConveyorMotionView>();
            SerializedObject movement = new SerializedObject(motion); Assign(movement, "conveyor", belt.GetComponent<ResourceConveyor>());
            SerializedProperty rollers = movement.FindProperty("rollers"); rollers.arraySize = 2;
            rollers.GetArrayElementAtIndex(0).objectReferenceValue = Find(visual, "Roller_Front");
            rollers.GetArrayElementAtIndex(1).objectReferenceValue = Find(visual, "Roller_Rear"); movement.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureUpgrade(Transform pad, ResourceConveyor conveyor)
        {
            foreach (Renderer renderer in pad.GetComponentsInChildren<Renderer>(true)) if (renderer.GetComponent<TextMesh>() == null) renderer.enabled = false;
            Transform root = ReplaceContainer(pad, "Imported Visuals");
            Transform locked = Instantiate("UpgradePad_off", root, new Vector3(0f, 0.15f, 0f));
            Transform active = Instantiate("UpgradePad", root, new Vector3(0f, 0.15f, 0f)); active.gameObject.SetActive(false);
            UpgradeVisualView view = pad.GetComponent<UpgradeVisualView>(); if (view == null) view = pad.gameObject.AddComponent<UpgradeVisualView>();
            SerializedObject data = new SerializedObject(view); Assign(data, "conveyor", conveyor); Assign(data, "lockedVisual", locked.gameObject); Assign(data, "unlockedVisual", active.gameObject); data.ApplyModifiedPropertiesWithoutUndo();
            Transform sign = pad.Find("Upgrade Sign"); if (sign != null) sign.localPosition = new Vector3(0f, 0.91f, 0f);
            Bounds stone = GameModelImportSetup.BoundsOf(Find(locked, "Upgrade_StoneBase"));
            float height = stone.max.y - 0.15f;
            Transform walkSurface = pad.Find("Upgrade Walk Surface");
            if (walkSurface == null) { walkSurface = new GameObject("Upgrade Walk Surface").transform; walkSurface.SetParent(pad, false); }
            BoxCollider platform = walkSurface.GetComponent<BoxCollider>();
            if (platform == null) platform = walkSurface.gameObject.AddComponent<BoxCollider>();
            platform.center = new Vector3(0f, 0.15f + height * 0.5f, 0f);
            platform.size = new Vector3(1.84f, height, 1.84f);
            ContextualTutorialView guide = Require("Mobile Controls").GetComponent<ContextualTutorialView>();
            SerializedObject tutorial = new SerializedObject(guide);
            tutorial.FindProperty("upgradeMarkerHeight").floatValue = GameModelImportSetup.BoundsOf(locked).max.y + 0.05f;
            tutorial.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureWagon(Transform wagon)
        {
            Transform previous = wagon.Find("Imported Order Sign"); if (previous != null) Object.DestroyImmediate(previous.gameObject);
            Transform signModel = Instantiate("OrderSign", wagon, new Vector3(0f, 0.80f, -0.56f)); signModel.name = "Imported Order Sign";
            Transform display = Find(signModel, "Order_Display_Surface");
            Bounds board = display.GetComponent<Renderer>().bounds;
            Transform label = wagon.Find("Order Sign");
            if (label != null)
            {
                label.position = board.center + new Vector3(0f, -0.16f, -0.025f);
                label.localRotation = Quaternion.identity;
                TextMesh text = label.GetComponent<TextMesh>(); text.characterSize = 0.065f;
                text.color = new Color(0.19f, 0.15f, 0.10f); PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            }
            Transform cargo = ReplaceContainer(wagon, "Imported Cargo");
            string[] names = { "CopperCoin", "CoinStack_03", "CoinStack_06", "CoinStack_10" };
            List<GameObject> stages = new List<GameObject>();
            foreach (string name in names)
            {
                Transform stack = Instantiate(name, cargo, new Vector3(0.40f, 0.86f, 0.03f));
                stack.gameObject.SetActive(false); stages.Add(stack.gameObject);
            }
            OrderCargoView view = wagon.GetComponent<OrderCargoView>(); if (view == null) view = wagon.gameObject.AddComponent<OrderCargoView>();
            SerializedObject data = new SerializedObject(view); Assign(data, "order", wagon.GetComponent<OrderProgress>());
            SerializedProperty objects = data.FindProperty("cargoStages"), thresholds = data.FindProperty("loadedThresholds");
            objects.arraySize = thresholds.arraySize = stages.Count;
            int[] values = { 1, 3, 6, 10 };
            for (int index = 0; index < stages.Count; index++) { objects.GetArrayElementAtIndex(index).objectReferenceValue = stages[index]; thresholds.GetArrayElementAtIndex(index).intValue = values[index]; }
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureDecor()
        {
            GameObject existing = GameObject.Find("Imported Courtyard Props"); if (existing != null) Object.DestroyImmediate(existing);
            Transform root = new GameObject("Imported Courtyard Props").transform;
            Instantiate("WoodenCrate", root, new Vector3(-5.50f, 0.15f, 5.0f));
            Instantiate("CopperOre_02", root, new Vector3(-5.65f, 0.80f, 4.98f)).localScale *= 1.5f;
            Instantiate("Barrel", root, new Vector3(-5.65f, 0.15f, 3.6f));
            Instantiate("WoodenCrate", root, new Vector3(5.65f, 0.15f, -3.5f));
            Instantiate("Barrel", root, new Vector3(-5.60f, 0.15f, -6.8f));
            Instantiate("AnvilWorkbench", root, new Vector3(5.48f, 0.15f, -1.9f));
            Vector3[] corners = { new Vector3(-6.65f, 0.15f, -8.65f), new Vector3(-6.65f, 0.15f, 8.65f), new Vector3(6.65f, 0.15f, 8.65f), new Vector3(6.65f, 0.15f, -8.65f) };
            for (int index = 0; index < corners.Length; index++) Instantiate("WoodenFence_Corner", root, corners[index]).localRotation = Quaternion.Euler(0f, index * 90f, 0f);
        }
        private static void ConfigureBodyCollider(Transform station, Vector3 center, Vector3 size)
        {
            Transform body = station.Find("Station Body Collider");
            if (body == null) { body = new GameObject("Station Body Collider").transform; body.SetParent(station, false); }
            BoxCollider collider = body.GetComponent<BoxCollider>(); if (collider == null) collider = body.gameObject.AddComponent<BoxCollider>();
            collider.center = center; collider.size = size; collider.isTrigger = false;
        }
        private static Transform ReplaceContainer(Transform parent, string name)
        {
            Transform old = parent.Find(name); if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject(name).transform; root.SetParent(parent, false); return root;
        }
        private static Transform Instantiate(string name, Transform parent, Vector3 position)
        {
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(GameModelImportSetup.Prefab(name), parent);
            model.name = name; model.transform.localPosition = position; model.transform.localRotation = Quaternion.identity;
            return model.transform;
        }
        private static Transform Find(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child;
            throw new MissingReferenceException("Model node missing: " + parent.name + "/" + name);
        }
        private static void RecordSceneOverrides()
        {
            foreach (Transform transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (!PrefabUtility.IsPartOfPrefabInstance(transform)) continue;
                PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(transform.gameObject);
            }
            foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
                if (PrefabUtility.IsPartOfPrefabInstance(renderer)) PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        private static Transform Require(string name) => GameObject.Find(name)?.transform ?? throw new MissingReferenceException("Scene object missing: " + name);
        private static void Assign(SerializedObject data, string name, Object value) => data.FindProperty(name).objectReferenceValue = value;
    }
}
