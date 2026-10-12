using MintTycoon.Economy;
using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Production;
using MintTycoon.Tutorial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MintTycoon.Editor
{
    public static class Stage8ContextualTutorialSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";

        [MenuItem("Mint Tycoon/Stage 8/Set Up Contextual Tutorial")]
        public static void SetUp()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player");
            GameObject canvasObject = GameObject.Find("Mobile Controls");
            BufferedProductionStation furnace = GameObject.Find("Copper Furnace")?.GetComponent<BufferedProductionStation>();
            BufferedProductionStation press = GameObject.Find("Copper Mint Press")?.GetComponent<BufferedProductionStation>();
            OrderProgress orderProgress = GameObject.Find("Royal Wagon")?.GetComponent<OrderProgress>();
            ResourceConveyor conveyor = GameObject.Find("Automatic Ingot Conveyor")?.GetComponent<ResourceConveyor>();

            if (player == null || canvasObject == null || furnace == null || press == null || orderProgress == null || conveyor == null)
                throw new MissingReferenceException("Stage 8 needs the Player, Mobile Controls, furnace, press, royal wagon, and Stage 7 conveyor.");

            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            GoldWallet wallet = player.GetComponent<GoldWallet>();
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (inventory == null || wallet == null || canvas == null)
                throw new MissingReferenceException("Player inventory, gold wallet, or Mobile Controls canvas is missing.");

            Transform oldPanel = canvas.transform.Find("Contextual Tutorial Panel");
            if (oldPanel != null)
                Object.DestroyImmediate(oldPanel.gameObject);

            GameObject panel = new GameObject("Contextual Tutorial Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            panel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -182f);
            panelRect.sizeDelta = new Vector2(900f, 116f);
            RawImage background = panel.GetComponent<RawImage>();
            background.color = new Color(0.12f, 0.09f, 0.06f, 0.86f);
            background.raycastTarget = false;

            Font font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            Text stepLabel = CreateText("Step Label", panelRect, font, 24, FontStyle.Bold,
                new Vector2(26f, -12f), new Vector2(840f, 36f), new Color(1f, 0.84f, 0.38f));
            Text instructionLabel = CreateText("Instruction", panelRect, font, 24, FontStyle.Normal,
                new Vector2(26f, -52f), new Vector2(846f, 54f), Color.white);
            SetText(stepLabel, "FIELD GUIDE  ·  1/7");
            SetText(instructionLabel, "Stand on the amber mine pad. Copper ore is collected automatically.");

            ContextualTutorialView view = canvas.GetComponent<ContextualTutorialView>();
            if (view == null)
                view = canvas.gameObject.AddComponent<ContextualTutorialView>();
            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("inventory").objectReferenceValue = inventory;
            serialized.FindProperty("furnace").objectReferenceValue = furnace;
            serialized.FindProperty("press").objectReferenceValue = press;
            serialized.FindProperty("orderProgress").objectReferenceValue = orderProgress;
            serialized.FindProperty("wallet").objectReferenceValue = wallet;
            serialized.FindProperty("conveyor").objectReferenceValue = conveyor;
            serialized.FindProperty("stepLabel").objectReferenceValue = stepLabel;
            serialized.FindProperty("instructionLabel").objectReferenceValue = instructionLabel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(canvasObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = canvasObject;
            Debug.Log("Mint Tycoon Stage 8 is ready: the field guide updates from inventory, station, order, wallet, and conveyor events.");
        }

        private static Text CreateText(string name, RectTransform parent, Font font, int size, FontStyle style,
            Vector2 position, Vector2 dimensions, Color color)
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
            label.alignment = TextAnchor.MiddleLeft;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = size - 4;
            label.resizeTextMaxSize = size;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private static void SetText(Text label, string value)
        {
            SerializedObject serialized = new SerializedObject(label);
            serialized.FindProperty("m_Text").stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(label);
        }
    }
}
