using MintTycoon.CameraSystem;
using MintTycoon.Player;
using MintTycoon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MintTycoon.Editor
{
    public static class Stage1SceneBuilder
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Player_Greybox.prefab";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 1/Build Greybox Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Material groundMaterial = CreateMaterial("Greybox_Ground", new Color(0.38f, 0.42f, 0.39f));
            Material wallMaterial = CreateMaterial("Greybox_Border", new Color(0.30f, 0.33f, 0.31f));
            Material structureMaterial = CreateMaterial("Greybox_Structure", new Color(0.58f, 0.60f, 0.56f));
            Material playerMaterial = CreateMaterial("Greybox_Player", new Color(0.28f, 0.62f, 0.72f));
            Material spawnMaterial = CreateMaterial("Greybox_Spawn", new Color(0.36f, 0.65f, 0.69f));

            CreateBlock("Ground", Vector3.zero, new Vector3(14f, 0.3f, 18f), groundMaterial, true);
            CreateBlock("North Boundary", new Vector3(0f, 0.6f, 9f), new Vector3(14f, 0.9f, 0.25f), wallMaterial, true);
            CreateBlock("South Boundary", new Vector3(0f, 0.6f, -9f), new Vector3(14f, 0.9f, 0.25f), wallMaterial, true);
            CreateBlock("East Boundary", new Vector3(7f, 0.6f, 0f), new Vector3(0.25f, 0.9f, 18f), wallMaterial, true);
            CreateBlock("West Boundary", new Vector3(-7f, 0.6f, 0f), new Vector3(0.25f, 0.9f, 18f), wallMaterial, true);

            CreateMarker("MINE BLOCKOUT", new Vector3(-3.5f, 0f, 4.5f), new Vector3(2.8f, 2.1f, 2.5f), structureMaterial);
            CreateMarker("FURNACE BLOCKOUT", new Vector3(2.5f, 0f, 4.5f), new Vector3(2.6f, 1.8f, 2.5f), structureMaterial);
            CreateMarker("PRESS BLOCKOUT", new Vector3(2.5f, 0f, -1.5f), new Vector3(2.6f, 1.8f, 2.5f), structureMaterial);
            CreateMarker("WAGON BLOCKOUT", new Vector3(-3.5f, 0f, -5.5f), new Vector3(3.2f, 1.6f, 2f), structureMaterial);
            CreateBlock("UPGRADE PAD BLOCKOUT", new Vector3(2.5f, 0.08f, -5.5f), new Vector3(1.6f, 0.16f, 1.6f), spawnMaterial, false);

            GameObject spawn = CreateBlock("Player Spawn Marker", new Vector3(-3f, 0.07f, 2.5f), new Vector3(1.1f, 0.08f, 1.1f), spawnMaterial, false);
            CreateWorldLabel("PLAYER SPAWN", spawn.transform.position + Vector3.up * 0.12f, 0.2f);

            GameObject player = CreatePlayer(playerMaterial);
            player.transform.position = new Vector3(-3f, 0.25f, 2.5f);
            PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FollowCamera));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 11f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.backgroundColor = new Color(0.72f, 0.79f, 0.77f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            FollowCamera follow = cameraObject.GetComponent<FollowCamera>();
            follow.Target = player.transform;

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            movement.SetMovementReference(cameraObject.transform);
            CreateJoystick(player);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;

            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            Light directionalLight = lightObject.GetComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };

            Selection.activeGameObject = player;
            Debug.Log($"Mint Tycoon Stage 1 greybox scene created at {ScenePath}");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_MintTycoon/Editor"))
                AssetDatabase.CreateFolder("Assets/_MintTycoon", "Editor");
        }

        private static Material CreateMaterial(string name, Color color)
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

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
                Object.DestroyImmediate(block.GetComponent<Collider>());
            return block;
        }

        private static void CreateMarker(string label, Vector3 groundPosition, Vector3 size, Material material)
        {
            Vector3 center = groundPosition + Vector3.up * (size.y * 0.5f + 0.15f);
            CreateBlock(label, center, size, material, true);
            CreateWorldLabel(label, center + Vector3.up * (size.y * 0.5f + 0.35f), 0.2f);
        }

        private static void CreateWorldLabel(string text, Vector3 position, float characterSize)
        {
            GameObject labelObject = new GameObject(text + " Label");
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = characterSize;
            label.fontSize = 42;
            label.color = new Color(0.12f, 0.14f, 0.13f);
            label.fontStyle = FontStyle.Bold;
        }

        private static GameObject CreatePlayer(Material material)
        {
            GameObject player = new GameObject("Player", typeof(CharacterController), typeof(PlayerMovement));
            CharacterController controller = player.GetComponent<CharacterController>();
            controller.height = 1.6f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0.8f, 0f);
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Greybox Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            face.name = "Facing Indicator";
            face.transform.SetParent(player.transform, false);
            face.transform.localPosition = new Vector3(0f, 1.38f, 0.24f);
            face.transform.localScale = new Vector3(0.2f, 0.2f, 0.18f);
            face.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(face.GetComponent<Collider>());
            return player;
        }

        private static void CreateJoystick(GameObject player)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            GameObject canvasObject = new GameObject("Mobile Controls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            FloatingJoystickSetup.Build(canvas, player.GetComponent<PlayerMovement>());
        }

    }
}
