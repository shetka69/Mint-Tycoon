using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Production;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MintTycoon.Editor
{
    public static class Stage4FurnaceSetup
    {
        private const string ScenePath = "Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity";
        private const string PrefabPath = "Assets/_MintTycoon/Art/Prefabs/Stations/CopperFurnace.prefab";
        private const string RecipePath = "Assets/_MintTycoon/Configs/Recipes/CopperSmelting.asset";
        private const string OrePath = "Assets/_MintTycoon/Configs/Resources/CopperOre.asset";
        private const string IngotPath = "Assets/_MintTycoon/Configs/Resources/CopperIngot.asset";
        private const string PrefabFolder = "Assets/_MintTycoon/Art/Prefabs/Stations";
        private const string MaterialFolder = "Assets/_MintTycoon/Art/Materials";

        [MenuItem("Mint Tycoon/Stage 4/Set Up Copper Furnace")]
        public static void SetUp()
        {
            ResourceDefinition ore = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(OrePath);
            ResourceDefinition ingot = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(IngotPath);
            if (ore == null || ingot == null)
                throw new MissingReferenceException("Missing copper ore or ingot definition. Complete Stage 2 setup first.");

            ProductionRecipeDefinition recipe = LoadOrCreateRecipe(ore, ingot);
            Material stone = LoadOrCreateMaterial("Furnace_Stone", new Color(0.30f, 0.32f, 0.31f));
            Material chamber = LoadOrCreateMaterial("Furnace_Chamber", new Color(0.10f, 0.12f, 0.12f));
            Material copper = LoadOrCreateMaterial("Furnace_Copper", new Color(0.78f, 0.35f, 0.16f));
            Material inputPad = LoadOrCreateMaterial("Furnace_InputZone", new Color(0.96f, 0.52f, 0.17f));
            Material outputPad = LoadOrCreateMaterial("Furnace_OutputZone", new Color(0.20f, 0.68f, 0.69f));
            MakeUnlit(inputPad);
            MakeUnlit(outputPad);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveObject("Copper Furnace");
            RemoveObject("FURNACE BLOCKOUT");
            RemoveObject("FURNACE BLOCKOUT Label");
            RemoveObject("COPPER FURNACE Label");

            GameObject furnace = new GameObject("Copper Furnace");
            furnace.transform.position = new Vector3(2.5f, 0f, 4.5f);
            BuildFurnaceVisual(furnace.transform, stone, chamber, copper);
            InteractionZone inputZone = BuildBufferZone(furnace.transform, "Input Zone", -1.95f, inputPad, "INPUT  ORE");
            InteractionZone outputZone = BuildBufferZone(furnace.transform, "Output Zone", 1.95f, outputPad, "OUTPUT  INGOTS");

            FurnaceStation station = furnace.AddComponent<FurnaceStation>();
            SerializedObject serializedStation = new SerializedObject(station);
            serializedStation.FindProperty("recipe").objectReferenceValue = recipe;
            serializedStation.FindProperty("inputZone").objectReferenceValue = inputZone;
            serializedStation.FindProperty("outputZone").objectReferenceValue = outputZone;
            serializedStation.FindProperty("inputCapacity").intValue = 12;
            serializedStation.FindProperty("outputCapacity").intValue = 12;
            serializedStation.FindProperty("transferIntervalSeconds").floatValue = 0.12f;
            serializedStation.ApplyModifiedPropertiesWithoutUndo();

            FurnaceBufferStackView view = furnace.AddComponent<FurnaceBufferStackView>();
            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("station").objectReferenceValue = station;
            serializedView.FindProperty("inputResource").objectReferenceValue = ore;
            serializedView.FindProperty("outputResource").objectReferenceValue = ingot;
            serializedView.FindProperty("inputAnchor").objectReferenceValue = inputZone.transform.Find("Buffe