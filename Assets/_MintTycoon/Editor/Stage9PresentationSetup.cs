using System.Collections.Generic;
using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Presentation;
using MintTycoon.Production;
using MintTycoon.Upgrades;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    /// <summary>Applies original low-poly presentation to the existing gameplay objects.</summary>
    public static class Stage9PresentationSetup
    {
        private const string RootPath = "Assets/_MintTycoon";
        private static Mesh rock;
        private static Material stone, timber, metal, copper, gold, cream, green, dark, leaf, glow, particle;

        [MenuItem("Mint Tycoon/Stage 9/Set Up Minimal Presentation")]
        public static void SetUp()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode before rebuilding presentation.");
            Scene scene = EditorSceneManager.OpenScene(RootPath + "/Scenes/MintTycoon_Greybox.unity", OpenSceneMode.Single);
            EnsureFolder(RootPath + "/Art/Models");
            EnsureFolder(RootPath + "/Art/Materials/Presentation");
            rock = RockMesh();
            stone = Material("Stone", new Color(0.52f, 0.57f, 0.55f));
            timber = Material("Timber", new Color(0.31f, 0.20f, 0.13f));
            metal = Material("Iron", new Color(0.19f, 0.25f, 0.27f));
            copper = Material("Copper", new Color(0.82f, 0.39f, 0.19f));
            gold = Material("Gold", new Color(0.98f, 0.70f, 0.19f));
            cream = Material("Canvas", new Color(0.91f, 0.83f, 0.62f));
            green = Material("WorkerGreen", new Color(0.19f, 0.40f, 0.34f));
            leaf = Material("Leaves", new Color(0.32f, 0.49f, 0.31f));
            dark = Material("Tunnel", new Color(0.10f, 0.14f, 0.15f));
            glow = Material("Fire", new Color(1f, 0.46f, 0.08f), true);
            particle = Material("FeedbackParticle", Color.white, true);

            Transform player = Require("Player");
            Transform mine = Require("Copper Mine");
            Transform furnace = Require("Copper Furnace");
            Transform press = Require("Copper Mint Press");
            Transform wagon = Require("Royal Wagon");
            Transform conveyor = Require("Automatic Ingot Conveyor");
            BuildCourtyard();
            BuildWorker(player);
            BuildMine(mine);
            BuildFurnace(furnace);
            BuildPress(press);
            BuildWagon(wagon);
            CompactWorldLabels();
            Transform marker = BuildGuide();
            MinimalHudSetup.Build(marker);
            ConfigurePlayerFeedback(player, wagon.GetComponent<OrderProgress>(), conveyor.GetComponent<ResourceConveyor>());
            ConfigureCamera(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Mint Tycoon Stage 9 ready: minimal HUD, world guidance, low-poly courtyard, worker walk, recipe animations, particles, and original audio.");
        }

        private static void BuildCourtyard()
        {
            GameObject old = GameObject.Find("Mint Courtyard Visuals");
            if (old != null) Object.DestroyImmediate(old);
            Transform root = new GameObject("Mint Courtyard Visuals").transform;
            Material sand = Material("Courtyard", new Color(0.69f, 0.64f, 0.48f));
            Material paving = Material("Paving", new Color(0.77f, 0.74f, 0.60f));
            Require("Ground").GetComponent<Renderer>().sharedMaterial = sand;
            Box(root, "Meadow", new Vector3(0f, -0.20f, 0f), new Vector3(38f, 0.16f, 38f), leaf);
            foreach (string name in new[] { "North Boundary", "South Boundary", "East Boundary", "West Boundary" })
            {
                Transform wall = Require(name);
                wall.GetComponent<Renderer>().sharedMaterial = stone;
                Vector3 scale = wall.localScale; scale.y = 0.5f; wall.localScale = scale;
                Vector3 position = wall.position; position.y = 0.38f; wall.position = position;
            }
            for (int index = 0; index < 12; index++)
            {
                float z = -7.8f + index * 1.38f;
                Box(root, "Courtyard stepping stone " + index, new Vector3(-0.75f + (index % 2) * 0.16f, 0.177f, z), new Vector3(1.32f, 0.035f, 1.10f), paving).localRotation = Quaternion.Euler(0f, index % 3 * 3f, 0f);
            }
            for (int index = 0; index < 6; index++)
                Box(root, "Mine path " + index, new Vector3(-4.6f + index * 1.15f, 0.178f, 1.55f), new Vector3(1.0f, 0.036f, 0.75f), paving);
            foreach (Vector3 location in new[] { new Vector3(-6f, 0f, 7.8f), new Vector3(6f, 0f, 7.8f), new Vector3(6f, 0f, -7.8f), new Vector3(-6f, 0f, -7.8f) })
            {
                Transform tree = new GameObject("Courtyard tree").transform; tree.SetParent(root, false); tree.localPosition = location;
                Box(tree, "Trunk", new Vector3(0f, 0.65f, 0f), new Vector3(0.28f, 1.1f, 0.28f), timber);
                Facet(tree, "Canopy", new Vector3(0f, 1.85f, 0f), new Vector3(1.1f, 1.4f, 1.1f), leaf);
                Facet(tree, "Canopy highlight", new Vector3(0.2f, 2.25f, 0.14f), new Vector3(0.75f, 1.1f, 0.75f), green);
            }
            Transform spawn = GameObject.Find("Player Spawn Marker")?.transform;
            if (spawn != null) spawn.gameObject.SetActive(false);
        }

        private static void BuildWorker(Transform player)
        {
            HideLegacyMeshes(player);
            Transform root = VisualRoot(player);
            Material skin = Material("Skin", new Color(0.84f, 0.58f, 0.39f));
            Box(root, "Tunic", new Vector3(0f, 0.91f, 0f), new Vector3(0.61f, 0.60f, 0.37f), green);
            Box(root, "Apron", new Vector3(0f, 0.87f, 0.205f), new Vector3(0.44f, 0.50f, 0.045f), cream);
            Box(root, "Belt", new Vector3(0f, 0.74f, 0f), new Vector3(0.65f, 0.09f, 0.39f), timber);
            Primitive(root, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.41f, 0f), new Vector3(0.48f, 0.48f, 0.45f), skin);
            Primitive(root, "Cap brim", PrimitiveType.Cylinder, new Vector3(0f, 1.60f, 0f), new Vector3(0.59f, 0.025f, 0.57f), timber);
            Primitive(root, "Cap", PrimitiveType.Sphere, new Vector3(0f, 1.64f, -0.025f), new Vector3(0.47f, 0.19f, 0.46f), green);
            Box(root, "Nose", new Vector3(0f, 1.39f, 0.245f), new Vector3(0.10f, 0.12f, 0.11f), skin);
            Box(root, "Left eye", new Vector3(-0.105f, 1.46f, 0.208f), new Vector3(0.038f, 0.038f, 0.03f), dark);
            Box(root, "Right eye", new Vector3(0.105f, 1.46f, 0.208f), new Vector3(0.038f, 0.038f, 0.03f), dark);
            Transform leftArm = Limb(root, "Left arm", new Vector3(-0.39f, 1.15f, 0f), new Vector3(0.19f, 0.46f, 0.23f), green);
            Transform rightArm = Limb(root, "Right arm", new Vector3(0.39f, 1.15f, 0f), new Vector3(0.19f, 0.46f, 0.23f), green);
            Box(leftArm, "Hand", new Vector3(0f, -0.46f, 0f), new Vector3(0.17f, 0.16f, 0.2f), skin);
            Box(rightArm, "Hand", new Vector3(0f, -0.46f, 0f), new Vector3(0.17f, 0.16f, 0.2f), skin);
            Transform leftLeg = Limb(root, "Left leg", new Vector3(-0.16f, 0.61f, 0f), new Vector3(0.22f, 0.36f, 0.25f), timber);
            Transform rightLeg = Limb(root, "Right leg", new Vector3(0.16f, 0.61f, 0f), new Vector3(0.22f, 0.36f, 0.25f), timber);
            Box(leftLeg, "Boot", new Vector3(0f, -0.38f, 0.05f), new Vector3(0.23f, 0.17f, 0.36f), metal);
            Box(rightLeg, "Boot", new Vector3(0f, -0.38f, 0.05f), new Vector3(0.23f, 0.17f, 0.36f), metal);
            PlayerWalkView view = player.GetComponent<PlayerWalkView>();
            if (view == null) view = player.gameObject.AddComponent<PlayerWalkView>();
            SerializedObject data = new SerializedObject(view);
            Assign(data, "visualRoot", root); Assign(data, "leftArm", leftArm); Assign(data, "rightArm", rightArm);
            Assign(data, "leftLeg", leftLeg); Assign(data, "rightLeg", rightLeg); data.ApplyModifiedPropertiesWithoutUndo();
            Transform stack = player.Find("Inventory Stack Anchor");
            if (stack != null) stack.localPosition = new Vector3(0f, 1.80f, -0.18f);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, RootPath + "/Art/Prefabs/MintWorker_Visual.prefab");
        }

        private static void BuildMine(Transform mine)
        {
            HideLegacyMeshes(mine);
            Transform root = VisualRoot(mine);
            Facet(root, "Rock ridge left", new Vector3(-0.9f, 1.04f, 0.67f), new Vector3(1.15f, 1.35f, 0.9f), stone);
            Facet(root, "Rock ridge right", new Vector3(0.9f, 0.96f, 0.78f), new Vector3(1.06f, 1.25f, 0.95f), stone);
            Facet(root, "Rock crown", new Vector3(-0.05f, 1.98f, 0.74f), new Vector3(1.25f, 0.72f, 0.78f), stone);
            Box(root, "Tunnel shadow", new Vector3(0f, 0.9f, 0.34f), new Vector3(1.45f, 1.45f, 0.22f), dark);
            Box(root, "Left support", new Vector3(-0.85f, 1.03f, 0.12f), new Vector3(0.24f, 1.8f, 0.28f), timber);
            Box(root, "Right support", new Vector3(0.85f, 1.03f, 0.12f), new Vector3(0.24f, 1.8f, 0.28f), timber);
            Box(root, "Lintel", new Vector3(0f, 1.97f, 0.12f), new Vector3(2f, 0.26f, 0.3f), timber);
            Facet(root, "Copper cluster left", new Vector3(-1.04f, 0.35f, -0.03f), new Vector3(0.31f, 0.30f, 0.32f), copper);
            Facet(root, "Copper cluster right", new Vector3(1.03f, 0.37f, 0.02f), new Vector3(0.36f, 0.29f, 0.27f), copper);
            Box(root, "Rail left", new Vector3(-0.36f, 0.24f, -0.03f), new Vector3(0.075f, 0.08f, 1.1f), metal);
            Box(root, "Rail right", new Vector3(0.36f, 0.24f, -0.03f), new Vector3(0.075f, 0.08f, 1.1f), metal);
        }

        private static void BuildFurnace(Transform furnace)
        {
            HideLegacyMeshes(furnace);
            Transform root = VisualRoot(furnace);
            Box(root, "Stone footing", new Vector3(0f, 0.39f, 0.22f), new Vector3(2.5f, 0.48f, 1.65f), stone);
            Primitive(root, "Round kiln", PrimitiveType.Cylinder, new Vector3(0f, 1.10f, 0.25f), new Vector3(2.18f, 0.77f, 1.48f), stone);
            Box(root, "Copper hood", new Vector3(0f, 1.87f, 0.25f), new Vector3(2.27f, 0.15f, 1.58f), copper);
            Box(root, "Firebox", new Vector3(0f, 0.97f, -0.51f), new Vector3(1.03f, 0.71f, 0.08f), dark);
            Box(root, "Embers", new Vector3(0f, 0.84f, -0.56f), new Vector3(0.62f, 0.27f, 0.03f), glow);
            for (int index = 0; index < 4; index++) Box(root, "Firebox grate " + index, new Vector3(-0.39f + index * 0.26f, 0.98f, -0.60f), new Vector3(0.045f, 0.65f, 0.055f), metal);
            Box(root, "Chimney", new Vector3(0.65f, 2.35f, 0.46f), new Vector3(0.44f, 0.90f, 0.50f), metal);
            Box(root, "Chimney cap", new Vector3(0.65f, 2.82f, 0.46f), new Vector3(0.61f, 0.12f, 0.63f), copper);
            ParticleSystem smoke = Particles(root, "Kiln smoke", new Vector3(0.65f, 2.94f, 0.46f), new Color(0.61f, 0.65f, 0.58f), true);
            ParticleSystem sparks = Particles(root, "Kiln sparks", new Vector3(0f, 0.84f, -0.60f), new Color(1f, 0.62f, 0.12f), false);
            ConfigureStation(furnace, null, null, smoke, sparks);
        }

        private static void BuildPress(Transform press)
        {
            HideLegacyMeshes(press);
            Transform root = VisualRoot(press);
            Box(root, "Press footing", new Vector3(0f, 0.37f, 0f), new Vector3(2.3f, 0.44f, 1.5f), stone);
            Box(root, "Anvil", new Vector3(0f, 0.72f, 0f), new Vector3(1.62f, 0.27f, 1.16f), metal);
            foreach (float x in new[] { -0.78f, 0.78f })
            {
                Box(root, "Timber upright", new Vector3(x, 1.37f, 0f), new Vector3(0.27f, 1.52f, 1.28f), timber);
                Box(root, "Copper foot", new Vector3(x, 0.78f, -0.02f), new Vector3(0.32f, 0.22f, 1.31f), copper);
            }
            Box(root, "Crossbeam", new Vector3(0f, 2.09f, 0f), new Vector3(2.04f, 0.26f, 1.36f), timber);
            Transform ram = Box(root, "Moving mint die", new Vector3(0f, 1.51f, -0.10f), new Vector3(0.59f, 0.48f, 0.72f), metal);
            Primitive(ram, "Coin stamp", PrimitiveType.Cylinder, new Vector3(0f, -0.56f, 0f), new Vector3(0.84f, 0.18f, 0.70f), copper);
            Transform wheel = Primitive(root, "Press flywheel", PrimitiveType.Cylinder, new Vector3(1.0f, 1.47f, 0f), new Vector3(0.76f, 0.10f, 0.76f), copper);
            wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Box(wheel, "Flywheel spoke", new Vector3(0f, 1.04f, 0f), new Vector3(0.11f, 0.08f, 0.79f), gold);
            ParticleSystem sparks = Particles(root, "Mint strike sparks", new Vector3(0f, 0.88f, -0.17f), new Color(1f, 0.75f, 0.22f), false);
            ConfigureStation(press, ram, wheel, null, sparks);
        }

        private static void BuildWagon(Transform wagon)
        {
            HideLegacyMeshes(wagon);
            Transform root = VisualRoot(wagon);
            Box(root, "Wagon chassis", new Vector3(0f, 0.52f, 0f), new Vector3(2.57f, 0.28f, 1.53f), timber);
            Box(root, "Wagon bed", new Vector3(0f, 0.80f, 0f), new Vector3(2.5f, 0.12f, 1.44f), cream);
            foreach (float z in new[] { -0.77f, 0.77f })
                for (int row = 0; row < 3; row++) Box(root, "Side plank", new Vector3(0f, 0.95f + row * 0.19f, z), new Vector3(2.60f, 0.14f, 0.12f), timber);
            foreach (float x in new[] { -1.24f, 1.24f }) Box(root, "Royal end panel", new Vector3(x, 1.17f, 0f), new Vector3(0.13f, 0.57f, 1.56f), green);
            Box(root, "Royal crest", new Vector3(1.32f, 1.18f, 0f), new Vector3(0.035f, 0.24f, 0.24f), gold);
            List<Transform> wheels = new List<Transform>();
            foreach (float x in new[] { -1.35f, 1.35f }) foreach (float z in new[] { -0.55f, 0.55f })
            {
                Transform wheel = Primitive(root, "Carriage wheel", PrimitiveType.Cylinder, new Vector3(x, 0.48f, z), new Vector3(0.78f, 0.10f, 0.78f), metal);
                wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Box(wheel, "Spoke horizontal", new Vector3(0f, x > 0f ? -1.06f : 1.06f, 0f), new Vector3(0.73f, 0.08f, 0.09f), timber);
                Box(wheel, "Spoke vertical", new Vector3(0f, x > 0f ? -1.06f : 1.06f, 0f), new Vector3(0.09f, 0.08f, 0.73f), timber);
                wheels.Add(wheel);
            }
            WagonWheelView view = wagon.GetComponent<WagonWheelView>();
            if (view == null) view = wagon.gameObject.AddComponent<WagonWheelView>();
            SerializedObject data = new SerializedObject(view);
            SerializedProperty array = data.FindProperty("wheels"); array.arraySize = wheels.Count;
            for (int index = 0; index < wheels.Count; index++) array.GetArrayElementAtIndex(index).objectReferenceValue = wheels[index];
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CompactWorldLabels()
        {
            foreach (TextMesh text in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
            {
                if (text.name.EndsWith(" Label") || text.name == "Delivery Count") { text.gameObject.SetActive(false); continue; }
                text.characterSize = 0.095f;
                text.color = new Color(0.18f, 0.22f, 0.19f);
                if (text.name == "Buffer Count") text.text = "0";
                if (text.name == "Order Sign") text.text = "0/12";
                PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            }
            foreach (ProductionBufferStackView view in Object.FindObjectsByType<ProductionBufferStackView>())
            {
                SerializedObject data = new SerializedObject(view); data.FindProperty("compactCountLabels").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            }
            UpgradePurchaseZone purchase = Require("Automatic Conveyor Upgrade").GetComponent<UpgradePurchaseZone>();
            SerializedObject upgrade = new SerializedObject(purchase);
            upgrade.FindProperty("compactSign").boolValue = true;
            upgrade.ApplyModifiedPropertiesWithoutUndo();
            Transform sign = purchase.transform.Find("Upgrade Sign");
            if (sign != null) sign.GetComponent<TextMesh>().text = "40";
        }

        private static Transform BuildGuide()
        {
            GameObject old = GameObject.Find("Next Action Marker"); if (old != null) Object.DestroyImmediate(old);
            Transform marker = new GameObject("Next Action Marker").transform;
            Transform mineZone = Require("Copper Mine").Find("Mine Collection Zone");
            marker.position = new Vector3(mineZone.position.x, 0.29f, mineZone.position.z);
            Material guide = Material("Guide", new Color(1f, 0.96f, 0.74f), true);
            Mesh ring = RingMesh();
            GameObject circle = new GameObject("Guide ring", typeof(MeshFilter), typeof(MeshRenderer)); circle.transform.SetParent(marker, false);
            circle.GetComponent<MeshFilter>().sharedMesh = ring; circle.GetComponent<MeshRenderer>().sharedMaterial = guide;
            Box(marker, "Guide chevron left", new Vector3(-0.105f, 0.025f, 0f), new Vector3(0.08f, 0.035f, 0.28f), guide).localRotation = Quaternion.Euler(0f, -40f, 0f);
            Box(marker, "Guide chevron right", new Vector3(0.105f, 0.025f, 0f), new Vector3(0.08f, 0.035f, 0.28f), guide).localRotation = Quaternion.Euler(0f, 40f, 0f);
            return marker;
        }

        private static void ConfigureStation(Transform root, Transform ram, Transform wheel, ParticleSystem smoke, ParticleSystem sparks)
        {
            StationFeedback feedback = root.GetComponent<StationFeedback>();
            if (feedback == null) feedback = root.gameObject.AddComponent<StationFeedback>();
            SerializedObject data = new SerializedObject(feedback);
            Assign(data, "station", root.GetComponent<BufferedProductionStation>()); Assign(data, "ram", ram); Assign(data, "flywheel", wheel);
            Assign(data, "smoke", smoke); Assign(data, "sparks", sparks);
            Assign(data, "audioSource", Audio(root.gameObject, 0.12f, 0.75f)); Assign(data, "cycleSound", Clip("MintStrike"));
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigurePlayerFeedback(Transform player, OrderProgress order, ResourceConveyor conveyor)
        {
            Transform old = player.Find("Reward particles"); if (old != null) Object.DestroyImmediate(old.gameObject);
            ParticleSystem particles = Particles(player, "Reward particles", new Vector3(0f, 1.15f, 0f), new Color(1f, 0.79f, 0.22f), false);
            PlayerActionFeedback feedback = player.GetComponent<PlayerActionFeedback>();
            if (feedback == null) feedback = player.gameObject.AddComponent<PlayerActionFeedback>();
            SerializedObject data = new SerializedObject(feedback);
            Assign(data, "inventory", player.GetComponent<PlayerInventory>()); Assign(data, "order", order); Assign(data, "conveyor", conveyor);
            Assign(data, "audioSource", Audio(player.gameObject, 0.26f, 0f)); Assign(data, "celebration", particles);
            Assign(data, "oreSound", Clip("OreTick")); Assign(data, "pickupSound", Clip("CoinPickup"));
            Assign(data, "rewardSound", Clip("OrderReward")); Assign(data, "unlockSound", Clip("ConveyorUnlock"));
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void ConfigureCamera(Transform player)
        {
            Camera camera = Camera.main;
            if (camera == null) throw new MissingReferenceException("Main Camera is missing.");
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.37f, 0.49f, 0.38f);
            camera.orthographicSize = 10.5f;
            Quaternion angle = Quaternion.Euler(50f, 45f, 0f);
            camera.transform.SetPositionAndRotation(player.position + new Vector3(0f, 1.5f, 0f) - angle * Vector3.forward * 22f, angle);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.78f, 0.82f, 0.70f);
            RenderSettings.ambientEquatorColor = new Color(0.59f, 0.61f, 0.47f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.35f, 0.27f);
            Light light = Require("Directional Light").GetComponent<Light>();
            light.color = new Color(1f, 0.92f, 0.76f); light.intensity = 1.5f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

        private static ParticleSystem Particles(Transform parent, string name, Vector3 position, Color color, bool smoke)
        {
            GameObject go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            ParticleSystem system = go.AddComponent<ParticleSystem>(); system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main; main.loop = smoke; main.playOnAwake = smoke; main.duration = 2f;
            main.startLifetime = smoke ? 1.8f : 0.55f; main.startSpeed = smoke ? 0.65f : 1.8f;
            main.startSize = smoke ? 0.23f : 0.07f; main.startColor = color; main.maxParticles = smoke ? 16 : 32;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.gravityModifier = smoke ? -0.04f : 0.55f;
            var emission = system.emission; emission.rateOverTime = smoke ? 5f : 0f;
            var shape = system.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = smoke ? 8f : 35f; shape.radius = 0.08f;
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var size = system.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, smoke ? 0.5f : 1f, 1f, smoke ? 1.6f : 0.15f));
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>(); renderer.renderMode = ParticleSystemRenderMode.Mesh; renderer.mesh = rock; renderer.sharedMaterial = particle;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return system;
        }
        private static AudioSource Audio(GameObject go, float volume, float spatial)
        {
            AudioSource source = go.GetComponent<AudioSource>();
            if (source == null) source = go.AddComponent<AudioSource>();
            source.playOnAwake = false; source.volume = volume; source.spatialBlend = spatial; source.minDistance = 5f; source.maxDistance = 25f;
            return source;
        }
        private static AudioClip Clip(string name)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RootPath + "/Audio/Feedback/" + name + ".wav");
            if (clip == null) throw new MissingReferenceException("Missing original feedback audio: " + name);
            return clip;
        }
        private static Transform Limb(Transform parent, string name, Vector3 pivot, Vector3 size, Material material)
        {
            Transform root = new GameObject(name).transform; root.SetParent(parent, false); root.localPosition = pivot;
            Box(root, "Limb mesh", new Vector3(0f, -size.y * 0.5f, 0f), size, material); return root;
        }
        private static Transform VisualRoot(Transform parent)
        {
            Transform old = parent.Find("Stage 9 Visuals"); if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject("Stage 9 Visuals").transform; root.SetParent(parent, false); return root;
        }
        private static void HideLegacyMeshes(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponent<TextMesh>() != null || renderer.name.Contains("Highlight") || renderer.name.Contains("Marker")) continue;
                renderer.enabled = false; PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }
        private static Transform Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material) => Primitive(parent, name, PrimitiveType.Cube, position, scale, material);
        private static Transform Primitive(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(primitive); Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material; return go.transform;
        }
        private static Transform Facet(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<MeshFilter>().sharedMesh = rock; go.GetComponent<Renderer>().sharedMaterial = material;
            return go.transform;
        }
        private static Material Material(string name, Color color, bool unlit = false)
        {
            string path = RootPath + "/Art/Materials/Presentation/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            material.color = color;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.18f);
            if (name == "Guide") material.SetFloat("_Cull", (float)CullMode.Off);
            EditorUtility.SetDirty(material); return material;
        }
        private static Mesh RockMesh()
        {
            string path = RootPath + "/Art/Models/FacetedRock.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh != null) return mesh;
            Vector3[] corners = { Vector3.up, Vector3.down, Vector3.left, Vector3.forward, Vector3.right, Vector3.back };
            int[] faces = { 0,3,2, 0,4,3, 0,5,4, 0,2,5, 1,2,3, 1,3,4, 1,4,5, 1,5,2 };
            Vector3[] vertices = new Vector3[faces.Length]; int[] triangles = new int[faces.Length];
            for (int index = 0; index < faces.Length; index++) { vertices[index] = corners[faces[index]]; triangles[index] = index; }
            mesh = new Mesh { name = "Faceted rock", vertices = vertices, triangles = triangles }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        private static Mesh RingMesh()
        {
            string path = RootPath + "/Art/Models/GuideRing.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh != null) return mesh;
            const int segments = 32; Vector3[] vertices = new Vector3[segments * 2]; int[] triangles = new int[segments * 6];
            for (int index = 0; index < segments; index++)
            {
                float angle = index * Mathf.PI * 2f / segments; Vector3 direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                vertices[index * 2] = direction * 0.54f; vertices[index * 2 + 1] = direction * 0.61f;
                int a = index * 2, b = (index + 1) % segments * 2, t = index * 6;
                triangles[t] = a; triangles[t + 1] = b; triangles[t + 2] = a + 1;
                triangles[t + 3] = a + 1; triangles[t + 4] = b; triangles[t + 5] = b + 1;
            }
            mesh = new Mesh { name = "Guide ring", vertices = vertices, triangles = triangles }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        private static Transform Require(string name) => GameObject.Find(name)?.transform ?? throw new MissingReferenceException("Scene object missing: " + name);
        private static void Assign(SerializedObject data, string name, Object value) => data.FindProperty(name).objectReferenceValue = value;
        private static void EnsureFolder(string path) { if (!AssetDatabase.IsValidFolder(path)) { string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent); AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path)); } }
    }
}
