using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Interaction;
using MintTycoon.Orders;
using MintTycoon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MintTycoon.Editor
{
    public static class Stage6RoyalWagonSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Stations/RoyalWagon.prefab";
        private const string CoinPath = "Assets/_MintTycoon/Configs/Resources/CopperCoin.asset";
        private const string FirstOrderPath = "Assets/_MintTycoon/Configs/Orders/CopperCoinOrder_12.asset";
        private const string SecondOrderPath = "Assets/_MintTycoon/Configs/Orders/CopperCoinOrder_16.asset";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 6/Set Up Royal Wagon and Orders")]
        public static void SetUp()
        {
            ResourceDefinition coin = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(CoinPath);
            if (coin == null)
                throw new MissingReferenceException("Missing copper coin definition. Complete Stage 2 setup first.");

            OrderDefinition firstOrder = LoadOrCreateOrder(FirstOrderPath, coin, 12, 50);
            OrderDefinition secondOrder = LoadOrCreateOrder(SecondOrderPath, coin, 16, 70);
            Material wood = LoadOrCreateMaterial("Wagon_Wood", new Color(0.33f, 0.20f, 0.12f));
            Material gold = LoadOrCreateMaterial("Wagon_RoyalGold", new Color(0.88f, 0.62f, 0.18f));
            Material iron = LoadOrCreateMaterial("Wagon_Iron", new Color(0.20f, 0.23f, 0.24f));
            Material deliveryPad = LoadOrCreateMaterial("Wagon_DeliveryZone", new Color(0.91f, 0.68f, 0.16f));
            MakeUnlit(deliveryPad);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Royal Wagon");
            RemoveObject("WAGON BLOCKOUT");
            RemoveObject("WAGON BLOCKOUT Label");
            RemoveObject("ROYAL WAGON Label");

            GameObject wagon = new GameObject("Royal Wagon");
            wagon.transform.position = new Vector3(-3.5f, 0f, -5.5f);
            BuildWagonVisual(wagon.transform, wood, gold, iron);
            InteractionZone deliveryZone = BuildDeliveryZone(wagon.transform, deliveryPad);

            OrderProgress orderProgress = wagon.AddComponent<OrderProgress>();
            SerializedObject serializedProgress = new SerializedObject(orderProgress);
            SerializedProperty orderArray = serializedProgress.FindProperty("orders");
            orderArray.arraySize = 2;
            orderArray.GetArrayElementAtIndex(0).objectReferenceValue = firstOrder;
            orderArray.GetArrayElementAtIndex(1).objectReferenceValue = secondOrder;
            serializedProgress.FindProperty("orderIndex").intValue = 0;
            serializedProgress.FindProperty("loadedAmount").intValue = 0;
            serializedProgress.FindProperty("completed").boolValue = false;
            serializedProgress.ApplyModifiedPropertiesWithoutUndo();

            WagonCoinReceiver receiver = wagon.AddComponent<WagonCoinReceiver>();
            SerializedObject serializedReceiver = new SerializedObject(receiver);
            serializedReceiver.FindProperty("deliveryZone").objectReferenceValue = deliveryZone;
            serializedReceiver.FindProperty("orderProgress").objectReferenceValue = orderProgress;
            serializedReceiver.FindProperty("transferIntervalSeconds").floatValue = 0.12f;
            serializedReceiver.ApplyModifiedPropertiesWithoutUndo();

            WagonRouteMover routeMover = wagon.AddComponent<WagonRouteMover>();
            SerializedObject serializedRoute = new SerializedObject(routeMover);
            serializedRoute.FindProperty("orderProgress").objectReferenceValue = orderProgress;
            serializedRoute.FindProperty("speed").floatValue = 3.5f;
            serializedRoute.FindProperty("turnSpeedDegrees").floatValue = 180f;
            SetRouteOffsets(serializedRoute.FindProperty("waypoints"));
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            CreateWagonSign(wagon.transform);

            PrefabUtility.SaveAsPrefabAssetAndConnect(wagon, PrefabPath, InteractionMode.AutomatedAction);
            wagon = GameObject.Find("Royal Wagon");
            orderProgress = wagon.GetComponent<OrderProgress>();
            routeMover = wagon.GetComponent<WagonRouteMover>();

            GameObject player = GameObject.Find("Player");
            if (player == null)
                throw new MissingReferenceException("Player object is missing from the greybox scene.");
            GoldWallet wallet = player.GetComponent<GoldWallet>();
            if (wallet == null)
                wallet = player.AddComponent<GoldWallet>();
            EditorUtility.SetDirty(player);

            SerializedObject connectedProgress = new SerializedObject(orderProgress);
            connectedProgress.FindProperty("wallet").objectReferenceValue = wallet;
            connectedProgress.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(orderProgress);

            Canvas canvas = GameObject.Find("Mobile Controls")?.GetComponent<Canvas>();
            if (canvas == null)
                throw new MissingReferenceException("Mobile Controls canvas is missing from the greybox scene.");
            TextMesh wagonOrderLabel = wagon.transform.Find("Order Sign")?.GetComponent<TextMesh>();
            BuildOrderHud(canvas, orderProgress, wallet, wagonOrderLabel);

            EditorUtility.SetDirty(wagon);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = wagon;
            Debug.Log("Mint Tycoon Stage 6 is ready: the royal wagon accepts the exact coin target, awards gold, departs, and returns with the next order.");
        }

        private static OrderDefinition LoadOrCreateOrder(string path, ResourceDefinition coin, int amount, int reward)
        {
            OrderDefinition order = AssetDatabase.LoadAssetAtPath<OrderDefinition>(path);
            if (order == null)
            {
                order = ScriptableObject.CreateInstance<OrderDefinition>();
                AssetDatabase.CreateAsset(order, path);
            }

            SerializedObject serialized = new SerializedObject(order);
            serialized.FindProperty("requestedResource").objectReferenceValue = coin;
            serialized.FindProperty("requestedAmount").intValue = amount;
            serialized.FindProperty("goldReward").intValue = reward;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            order.name = System.IO.Path.GetFileNameWithoutExtension(path);
            EditorUtility.SetDirty(order);
            return order;
        }

        private static void SetRouteOffsets(SerializedProperty route)
        {
            Vector3[] offsets =
            {
                Vector3.zero,
                new Vector3(0f, 0f, -1.8f),
                new Vector3(-2.1f, 0f, -2.7f),
                new Vector3(-4.7f, 0f, -2.7f)
            };

            route.arraySize = offsets.Length;
            for (int index = 0; index < offsets.Length; index++)
                route.GetArrayElementAtIndex(index).vector3Value = offsets[index];
        }

        private static void BuildWagonVisual(Transform root, Material wood, Material gold, Material iron)
        {
            CreateVisual("Wagon Chassis", PrimitiveType.Cube, root, new Vector3(0f, 0.55f, 0f), new Vector3(2.65f, 0.30f, 1.55f), iron);
            CreateVisual("Wagon Bed", PrimitiveType.Cube, root, new Vector3(0f, 0.86f, 0f), new Vector3(2.62f, 0.36f, 1.52f), wood);
            CreateVisual("Wagon Left Board", PrimitiveType.Cube, root, new Vector3(-1.22f, 1.20f, 0f), new Vector3(0.18f, 0.74f, 1.62f), wood);
            CreateVisual("Wagon Right Board", PrimitiveType.Cube, root, new Vector3(1.22f, 1.20f, 0f), new Vector3(0.18f, 0.74f, 1.62f), wood);
            CreateVisual("Wagon Front Board", PrimitiveType.Cube, root, new Vector3(0f, 1.20f, 0.72f), new Vector3(2.50f, 0.74f, 0.16f), wood);
            CreateVisual("Wagon Rear Board", PrimitiveType.Cube, root, new Vector3(0f, 1.20f, -0.72f), new Vector3(2.50f, 0.74f, 0.16f), wood);
            CreateVisual("Wagon Gold Rail Left", PrimitiveType.Cube, root, new Vector3(-1.22f, 1.58f, 0f), new Vector3(0.20f, 0.08f, 1.70f), gold);
            CreateVisual("Wagon Gold Rail Right", PrimitiveType.Cube, root, new Vector3(1.22f, 1.58f, 0f), new Vector3(0.20f, 0.08f, 1.70f), gold);
            CreateVisual("Wagon Royal Crest Near", PrimitiveType.Cylinder, root, new Vector3(0f, 1.20f, -0.82f), new Vector3(0.45f, 0.08f, 0.45f), gold).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateVisual("Wagon Royal Crest Far", PrimitiveType.Cylinder, root, new Vector3(0f, 1.20f, 0.82f), new Vector3(0.45f, 0.08f, 0.45f), gold).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateVisual("Wagon Coin Emblem", PrimitiveType.Sphere, root, new Vector3(0f, 1.20f, -0.91f), new Vector3(0.20f, 0.20f, 0.07f), gold);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    GameObject wheel = CreateVisual($"Wagon Wheel {side} {axle}", PrimitiveType.Cylinder, root,
                        new Vector3(side * 1.36f, 0.38f, axle * 0.57f), new Vector3(0.76f, 0.18f, 0.76f), iron);
                    wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    CreateVisual($"Wheel Hub {side} {axle}", PrimitiveType.Cylinder, root,
                        new Vector3(side * 1.46f, 0.38f, axle * 0.57f), new Vector3(0.27f, 0.20f, 0.27f), gold).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }
        }

        private static InteractionZone BuildDeliveryZone(Transform root, Material marker)
        {
            const float width = 2.70f;
            const float depth = 1.35f;
            const float localZ = 1.78f;
            CreateVisual("Coin Delivery Highlight", PrimitiveType.Cube, root,
                new Vector3(0f, 0.175f, localZ), new Vector3(width - 0.12f, 0.05f, depth - 0.12f), marker);
            const float edge = 0.12f;
            CreateVisual("Coin Delivery Marker Front", PrimitiveType.Cube, root,
                new Vector3(0f, 0.22f, localZ - depth * 0.5f), new Vector3(width, 0.10f, edge), marker);
            CreateVisual("Coin Delivery Marker Back", PrimitiveType.Cube, root,
                new Vector3(0f, 0.22f, localZ + depth * 0.5f), new Vector3(width, 0.10f, edge), marker);
            CreateVisual("Coin Delivery Marker Left", PrimitiveType.Cube, root,
                new Vector3(-width * 0.5f, 0.22f, localZ), new Vector3(edge, 0.10f, depth), marker);
            CreateVisual("Coin Delivery Marker Right", PrimitiveType.Cube, root,
                new Vector3(width * 0.5f, 0.22f, localZ), new Vector3(edge, 0.10f, depth), marker);

            GameObject zoneObject = new GameObject("Coin Delivery Zone");
            zoneObject.transform.SetParent(root, false);
            zoneObject.transform.localPosition = new Vector3(0f, 0.85f, localZ);
            BoxCollider trigger = zoneObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(width, 1.7f, depth);
            InteractionZone zone = zoneObject.AddComponent<InteractionZone>();
            Rigidbody body = zoneObject.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            GameObject countObject = new GameObject("Delivery Count");
            countObject.transform.SetParent(zoneObject.transform, false);
            countObject.transform.localPosition = new Vector3(0f, 0.08f, -depth * 0.5f + 0.12f);
            countObject.transform.localRotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = countObject.AddComponent<TextMesh>();
            label.text = "COIN DELIVERY  0/12";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.105f;
            label.fontSize = 42;
            label.color = new Color(0.16f, 0.12f, 0.07f);
            label.fontStyle = FontStyle.Bold;
            return zone;
        }

        private static void BuildOrderHud(Canvas canvas, OrderProgress progress, GoldWallet wallet, TextMesh wagonLabel)
        {
            Transform oldPanel = canvas.transform.Find("Order HUD Panel");
            if (oldPanel != null)
                Object.DestroyImmediate(oldPanel.gameObject);

            GameObject panel = new GameObject("Order HUD Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -28f);
            panelRect.sizeDelta = new Vector2(940f, 142f);
            RawImage background = panel.GetComponent<RawImage>();
            background.color = new Color(0.12f, 0.09f, 0.06f, 0.88f);
            background.raycastTarget = false;

            Font font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            Text orderTitle = CreateHudText("Order Title", panelRect, font, 30, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(30f, -18f), new Vector2(600f, 48f), new Color(1f, 0.86f, 0.52f));
            Text orderDetails = CreateHudText("Order Details", panelRect, font, 24, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(30f, -76f), new Vector2(670f, 42f), Color.white);
            Text goldBalance = CreateHudText("Gold Balance", panelRect, font, 29, FontStyle.Bold, TextAnchor.MiddleRight,
                new Vector2(-26f, -45f), new Vector2(222f, 50f), new Color(1f, 0.84f, 0.30f));
            orderTitle.text = "ROYAL ORDER 1/2";
            orderDetails.text = "0/12 COPPER COINS     REWARD 50 GOLD";
            goldBalance.text = "GOLD  0";
            RectTransform goldRect = goldBalance.rectTransform;
            goldRect.anchorMin = goldRect.anchorMax = new Vector2(1f, 1f);
            goldRect.pivot = new Vector2(1f, 1f);

            OrderHudView view = canvas.GetComponent<OrderHudView>();
            if (view == null)
                view = canvas.gameObject.AddComponent<OrderHudView>();
            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("orderProgress").objectReferenceValue = progress;
            serialized.FindProperty("wallet").objectReferenceValue = wallet;
            serialized.FindProperty("orderTitle").objectReferenceValue = orderTitle;
            serialized.FindProperty("orderDetails").objectReferenceValue = orderDetails;
            serialized.FindProperty("goldBalance").objectReferenceValue = goldBalance;
            serialized.FindProperty("wagonOrderLabel").objectReferenceValue = wagonLabel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateHudText(string name, RectTransform parent, Font font, int size, FontStyle style,
            TextAnchor alignment, Vector2 position, Vector2 dimensions, Color color)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            Text label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = size - 4;
            label.resizeTextMaxSize = size;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void CreateWagonSign(Transform wagon)
        {
            GameObject sign = new GameObject("Order Sign");
            sign.transform.SetParent(wagon, false);
            sign.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            sign.transform.localRotation = Quaternion.Euler(50f, 45f, 0f);
            TextMesh label = sign.AddComponent<TextMesh>();
            label.text = "ROYAL ORDER\n0/12 COINS";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.14f;
            label.fontSize = 42;
            label.color = new Color(0.12f, 0.10f, 0.07f);
            label.fontStyle = FontStyle.Bold;
        }

        private static GameObject CreateVisual(string name, PrimitiveType shape, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material)
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
