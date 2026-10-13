using System;
using System.IO;
using System.Linq;
using MintTycoon.Player;
using MintTycoon.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Integrates the new supplied rig independently of the earlier model set.</summary>
    public static class PlayerCharacterSetup
    {
        private const string SourceModel = "Assets/Player/armystickman.fbx";
        private const string RiggedModel = "Assets/Player/Walking.fbx";
        private const string TexturePath = "Assets/Player/Textures/SoldierAtlas.jpg";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials/Characters";
        private const string AnimationFolder = "Assets/_MintTycoon/Art/Animations/Characters";
        public const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Characters/ArmyWorker.prefab";
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";

        [MenuItem("Mint Tycoon/Player/Set Up Animated Character and Floating Joystick")]
        public static void SetUp()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before changing the character.");
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new InvalidOperationException("Open MintTycoon_Greybox before applying the character setup.");
            if (!File.Exists(RiggedModel) || !File.Exists(SourceModel))
                throw new FileNotFoundException("Both supplied Player FBX files are required.");
            GameModelImportSetup.EnsureFolder(MaterialFolder);
            GameModelImportSetup.EnsureFolder(AnimationFolder);
            GameModelImportSetup.EnsureFolder(Path.GetDirectoryName(PrefabPath).Replace('\\', '/'));
            ImportMaterialsAndRig();
            AnimationClip walk = AssetDatabase.LoadAllAssetsAtPath(RiggedModel).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
            BuildVisualPrefab(walk);

            Transform player = GameObject.Find("Player")?.transform;
            if (player == null) throw new MissingReferenceException("Player gameplay root is missing.");
            ApplyToPlayer(player);
            Canvas canvas = GameObject.Find("Mobile Controls")?.GetComponent<Canvas>();
            if (canvas == null) throw new MissingReferenceException("Mobile Controls canvas is missing.");
            FloatingJoystickSetup.Build(canvas, player.GetComponent<PlayerMovement>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Temp/PlayerImport");
            File.WriteAllText("Temp/PlayerImport/import_report.txt", $"Humanoid avatar valid. Walk duration: {walk.length:F3}s. Source velocity: {walk.averageSpeed}. New character prefab and full-screen floating joystick saved. Earlier model assets unchanged.\n");
            Debug.Log("Mint Tycoon: supplied animated character and translucent floating joystick saved. Old character visuals preserved but hidden.");
        }

        private static void ImportMaterialsAndRig()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new MissingReferenceException("URP Lit shader is missing.");
            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (atlas == null) throw new MissingReferenceException("Soldier texture atlas is missing.");
            TextureImporter texture = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            texture.textureType = TextureImporterType.Default;
            texture.sRGBTexture = true; texture.mipmapEnabled = true;
            texture.maxTextureSize = 1024; texture.textureCompression = TextureImporterCompression.Compressed;
            texture.SaveAndReimport();
            Material skin = Material("SoldierBody", shader, new Color(0.906f, 0.845f, 0.856f), null);
            Material outfit = Material("SoldierOutfit", shader, Color.white, atlas);
            foreach (string path in new[] { SourceModel, RiggedModel })
            {
                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(path);
                importer.globalScale = 1f; importer.useFileScale = true;
                importer.importCameras = false; importer.importLights = false;
                importer.importBlendShapes = false; importer.addCollider = false;
                importer.isReadable = false; importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.CalculateMikk;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "body"), skin);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "texture"), outfit);
                bool rigged = path == RiggedModel;
                importer.importAnimation = rigged;
                importer.animationType = rigged ? ModelImporterAnimationType.Human : ModelImporterAnimationType.None;
                if (rigged)
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.optimizeGameObjects = false;
                    ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                    if (clips.Length == 0) throw new MissingReferenceException("Walking.fbx contains no animation take.");
                    foreach (ModelImporterClipAnimation clip in clips)
                    {
                        clip.name = "Walk"; clip.loopTime = true; clip.loopPose = true;
                        // Extract horizontal motion: the CharacterController owns locomotion.
                        clip.lockRootPositionXZ = false; clip.lockRootHeightY = true; clip.lockRootRotation = true;
                        clip.keepOriginalPositionXZ = false; clip.keepOriginalPositionY = false;
                        clip.keepOriginalOrientation = true; clip.heightFromFeet = true;
                    }
                    importer.clipAnimations = clips;
                }
                importer.SaveAndReimport();
            }
        }

        private static Material Material(string name, Shader shader, Color color, Texture2D atlas)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader; material.SetColor("_BaseColor", color);
            material.SetTexture("_BaseMap", atlas); material.SetFloat("_Smoothness", 0.22f);
            material.SetFloat("_Metallic", 0f); EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildVisualPrefab(AnimationClip walk)
        {
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject root = new GameObject("ArmyWorker");
                SceneManager.MoveGameObjectToScene(root, preview);
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(RiggedModel), preview);
                model.name = "Rigged Soldier"; model.transform.SetParent(root.transform, false);
                Animator animator = model.GetComponent<Animator>();
                if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException("Walking.fbx did not produce a valid humanoid avatar.");
                AnimationClip idle = BuildIdlePose(animator);
                animator.runtimeAnimatorController = BuildController(idle, walk);
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                idle.SampleAnimation(model, 0f);
                Bounds bounds = PosedBounds(model);
                Directory.CreateDirectory("Temp/PlayerImport");
                File.WriteAllText("Temp/PlayerImport/dimensions.txt", $"Source root scale: {model.transform.localScale}; posed bounds: {bounds}; avatar scale: {animator.humanScale}\n");
                float scale = 1.6f / bounds.size.y;
                model.transform.localScale *= scale;
                bounds = PosedBounds(model);
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                Vector3 center = hips.position;
                model.transform.localPosition -= new Vector3(center.x, bounds.min.y, center.z);
                File.AppendAllText("Temp/PlayerImport/dimensions.txt", $"Final root scale: {model.transform.localScale}; position: {model.transform.localPosition}; normalized bounds: {PosedBounds(model)}\n");
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                foreach (Transform item in model.GetComponentsInChildren<Transform>(true))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(item);
                PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }

        private static AnimationClip BuildIdlePose(Animator animator)
        {
            string path = AnimationFolder + "/ArmyWorker_Idle.anim";
            AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (idle == null) { idle = new AnimationClip { name = "ArmyWorker_Idle" }; AssetDatabase.CreateAsset(idle, path); }
            idle.ClearCurves(); idle.frameRate = 30f;
            HumanPose pose = new HumanPose();
            using (HumanPoseHandler handler = new HumanPoseHandler(animator.avatar, animator.transform))
            {
                handler.GetHumanPose(ref pose);
                pose.bodyRotation = Quaternion.identity;
                pose.bodyPosition.x = pose.bodyPosition.z = 0f;
                Array.Clear(pose.muscles, 0, pose.muscles.Length);
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    string muscle = HumanTrait.MuscleName[i];
                    if (muscle.EndsWith("Arm Down-Up")) pose.muscles[i] = -0.88f;
                    if (muscle.EndsWith("Forearm Stretch") || muscle.EndsWith("Leg Stretch")) pose.muscles[i] = 1f;
                    Constant(idle, muscle, pose.muscles[i]);
                }
                Constant(idle, "RootT.x", 0f); Constant(idle, "RootT.y", pose.bodyPosition.y); Constant(idle, "RootT.z", 0f);
                Constant(idle, "RootQ.x", 0f); Constant(idle, "RootQ.y", 0f); Constant(idle, "RootQ.z", 0f); Constant(idle, "RootQ.w", 1f);
                handler.SetHumanPose(ref pose);
            }
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(idle);
            settings.loopTime = true; settings.loopBlend = true;
            AnimationUtility.SetAnimationClipSettings(idle, settings);
            EditorUtility.SetDirty(idle);
            return idle;
        }

        private static void Constant(AnimationClip clip, string property, float value)
        {
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), property), AnimationCurve.Constant(0f, 1f, value));
        }

        private static AnimatorController BuildController(AnimationClip idle, AnimationClip walk)
        {
            string path = AnimationFolder + "/ArmyWorker.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            // Rebuild this new controller only; retain its asset GUID and both original FBX files.
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState state in machine.states)
            {
                if (state.state.motion is BlendTree oldTree) Object.DestroyImmediate(oldTree, true);
                machine.RemoveState(state.state);
            }
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
            controller.AddParameter("MoveAmount", AnimatorControllerParameterType.Float);
            controller.AddParameter("PlaybackRate", AnimatorControllerParameterType.Float);
            AnimatorControllerParameter[] parameters = controller.parameters;
            parameters[1].defaultFloat = 1f; controller.parameters = parameters;
            AnimatorState locomotion = machine.AddState("Locomotion"); machine.defaultState = locomotion;
            BlendTree tree = new BlendTree { name = "Standing / Walk", blendType = BlendTreeType.Simple1D, blendParameter = "MoveAmount", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(tree, controller);
            tree.AddChild(idle, 0f); tree.AddChild(walk, 1f);
            locomotion.motion = tree; locomotion.speedParameter = "PlaybackRate"; locomotion.speedParameterActive = true;
            locomotion.writeDefaultValues = false;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Bounds PosedBounds(GameObject model)
        {
            Bounds bounds = default; bool initialized = false;
            foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh = new Mesh();
                try
                {
                    // Compensate for the imported 100x mesh transforms before mapping vertices to world space.
                    renderer.BakeMesh(mesh, true);
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        Vector3 point = renderer.transform.TransformPoint(vertex);
                        if (!initialized) { bounds = new Bounds(point, Vector3.zero); initialized = true; }
                        else bounds.Encapsulate(point);
                    }
                }
                finally { Object.DestroyImmediate(mesh); }
            }
            if (!initialized || bounds.size.y <= 0f) throw new InvalidOperationException("The animated model has no skinned geometry.");
            return bounds;
        }

        public static void ApplyToPlayer(Transform player)
        {
            foreach (string name in new[] { "Imported Visuals", "Stage 9 Visuals", "Greybox Body", "Facing Indicator" })
            {
                Transform previous = player.Find(name);
                if (previous != null) previous.gameObject.SetActive(false);
            }
            PlayerWalkView oldWalk = player.GetComponent<PlayerWalkView>();
            if (oldWalk != null) oldWalk.enabled = false;
            Transform root = player.Find("Animated Character");
            if (root == null)
            {
                root = new GameObject("Animated Character").transform;
                root.SetParent(player, false);
            }
            foreach (Transform child in root.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            Animator animator = visual.GetComponentInChildren<Animator>();
            PlayerAnimatorView view = player.GetComponent<PlayerAnimatorView>();
            if (view == null) view = player.gameObject.AddComponent<PlayerAnimatorView>();
            SerializedObject data = new SerializedObject(view);
            data.FindProperty("animator").objectReferenceValue = animator;
            AnimationClip walk = AssetDatabase.LoadAllAssetsAtPath(RiggedModel).OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__"));
            data.FindProperty("walkMetersPerSecond").floatValue = Mathf.Max(0.5f, walk.averageSpeed.magnitude * animator.transform.lossyScale.y);
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            Selection.activeGameObject = player.gameObject;
        }
    }
}
