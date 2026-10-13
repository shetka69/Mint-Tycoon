using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Player;
using MintTycoon.Production;
using MintTycoon.Tutorial;
using MintTycoon.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MintTycoon.Editor
{
    /// <summary>Shared scene setup for the small, safe-area-aware HUD.</summary>
    public static class MinimalHudSetup
    {
        public static void Build(Transform marker = null)
        {
            GameObject player = GameObject.Find("Player");
            Canvas canvas = GameObject.Find("Mobile Controls")?.GetComponent<Canvas>();
            BufferedProductionStation furnace = GameObject.Find("Copper Furnace")?.GetComponent<BufferedProductionStation>();
            BufferedProductionStation press = GameObject.Find("Copper Mint Press")?.GetComponent<BufferedProductionStation>();
            GameObject wagon = GameObject.Find("Royal Wagon");
            ResourceConveyor conveyor = GameObject.Find("Automatic Ingot Conveyor")?.GetComponent<ResourceConveyor>();
            if (player == null || canvas == null || furnace == null || press == null || wagon == null || conveyor == null)
                throw new MissingReferenceException("The compact HUD requires the complete Stage 7 scene.");
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            GoldWallet wallet = player.GetComponent<GoldWallet>();
            OrderProgress order = wagon.GetComponent<OrderProgress>();
            foreach (VirtualJoystick joystick in canvas.GetComponentsInChildren<VirtualJoystick>(true))
                joystick.transform.SetParent(canvas.transform, false);
            Remove(canvas.transform, "Safe Area");
            Remove(canvas.transform, "Order HUD Panel");
            Remove(canvas.transform, "Contextual Tutorial Panel");

            RectTransform safeArea = Rect("Safe Area", canvas.transform, Vector2.zero, Vector2.zero, Vector2.zero);
            safeArea.anchorMin = Vector2.zero; safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
            safeArea.gameObject.AddComponent<SafeAreaView>();
            Color background = new Color(0.12f, 0.15f, 0.13f, 0.82f);
            RectTransform coins = Pill("Order Counter", safeArea, new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(208f, 68f), background);
            RectTransform gold = Pill("Gold Counter", safeArea, Vector2.one, new Vector2(-28f, -28f), new Vector2(178f, 68f), background);
            Icon(coins, new Color(0.86f, 0.44f, 0.22f));
            Icon(gold, new Color(1f, 0.76f, 0.22f));
            Text orderCounter = Label("Coins", coins, "0/12", 34, new Vector2(65f, 0f), new Vector2(132f, 68f));
            Text goldCounter = Label("Gold", gold, "0", 36, new Vector2(65f, 0f), new Vector2(99f, 68f));
            OrderHudView orderView = canvas.GetComponent<OrderHudView>();
            if (orderView == null) orderView = canvas.gameObject.AddComponent<OrderHudView>();
            SerializedObject orderData = new SerializedObject(orderView);
            Assign(orderData, "orderProgress", order); Assign(orderData, "wallet", wallet);
            Assign(orderData, "orderTitle", null); Assign(orderData, "orderDetails", orderCounter); Assign(orderData, "goldBalance", goldCounter);
            Assign(orderData, "wagonOrderLabel", wagon.transform.Find("Order Sign")?.GetComponent<TextMesh>());
            orderData.FindProperty("compactLayout").boolValue = true;
            orderData.ApplyModifiedPropertiesWithoutUndo();

            RectTransform hint = Pill("Context Hint", safeArea, new Vector2(0.5f, 0f), new Vector2(0f, 265f), new Vector2(540f, 60f), background);
            Text hintLabel = Label("Instruction", hint, "Stand here to mine", 30, new Vector2(20f, 0f), new Vector2(500f, 60f));
            hintLabel.alignment = TextAnchor.MiddleCenter;
            CanvasGroup hintGroup = hint.gameObject.AddComponent<CanvasGroup>();
            hintGroup.alpha = 0f; hintGroup.blocksRaycasts = false; hintGroup.interactable = false;
            ContextualTutorialView tutorial = canvas.GetComponent<ContextualTutorialView>();
            if (tutorial == null) tutorial = canvas.gameObject.AddComponent<ContextualTutorialView>();
            SerializedObject guide = new SerializedObject(tutorial);
            Assign(guide, "inventory", inventory); Assign(guide, "furnace", furnace); Assign(guide, "press", press);
            Assign(guide, "orderProgress", order); Assign(guide, "wallet", wallet); Assign(guide, "conveyor", conveyor);
            Assign(guide, "upgrade", AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/_MintTycoon/Configs/Upgrades/AutomaticIngotConveyor.asset"));
            Assign(guide, "stepLabel", null); Assign(guide, "instructionLabel", hintLabel); Assign(guide, "hintGroup", hintGroup);
            Assign(guide, "guideMarker", marker != null ? marker : GameObject.Find("Next Action Marker")?.transform);
            Assign(guide, "deliveryTarget", wagon.transform.Find("Coin Delivery Zone"));
            Transform[] targets = {
                GameObject.Find("Copper Mine").transform.Find("Mine Collection Zone"),
                furnace.transform.Find("Input Zone"), furnace.transform.Find("Output Zone"),
                press.transform.Find("Input Zone"), press.transform.Find("Output Zone"),
                wagon.transform.Find("Coin Delivery Zone"), GameObject.Find("Automatic Conveyor Upgrade").transform.Find("Purchase Zone")
            };
            SerializedProperty targetArray = guide.FindProperty("stepTargets");
            targetArray.arraySize = targets.Length;
            for (int index = 0; index < targets.Length; index++) targetArray.GetArrayElementAtIndex(index).objectReferenceValue = targets[index];
            guide.ApplyModifiedPropertiesWithoutUndo();

            FloatingJoystickSetup.Build(canvas, player.GetComponent<PlayerMovement>());
            EditorUtility.SetDirty(canvas.gameObject);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }
        private static RectTransform Pill(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = Rect(name, parent, anchor, position, size);
            PillGraphic graphic = rect.gameObject.AddComponent<PillGraphic>();
            graphic.color = color; graphic.raycastTarget = false;
            return rect;
        }
        private static Text Label(string name, RectTransform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            RectTransform rect = Rect(name, parent, new Vector2(0f, 0.5f), position, dimensions);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.fontStyle = FontStyle.Bold; text.color = new Color(1f, 0.98f, 0.91f);
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            SerializedObject data = new SerializedObject(text);
            data.FindProperty("m_Text").stringValue = value; data.ApplyModifiedPropertiesWithoutUndo();
            return text;
        }
        private static void Icon(RectTransform parent, Color color)
        {
            RectTransform rect = Rect("Coin Icon", parent, new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(35f, 35f));
            CircleGraphic circle = rect.gameObject.AddComponent<CircleGraphic>(); circle.color = color; circle.raycastTarget = false;
            RectTransform slash = Rect("Stamp", rect, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 19f));
            RawImage stamp = slash.gameObject.AddComponent<RawImage>(); stamp.color = new Color(1f, 0.96f, 0.75f, 0.7f); stamp.raycastTarget = false;
        }
        private static void Assign(SerializedObject data, string name, Object value) => data.FindProperty(name).objectReferenceValue = value;
        private static void Remove(Transform parent, string name) { Transform item = parent.Find(name); if (item != null) Object.DestroyImmediate(item.gameObject); }
    }
}
