using UnityEditor;
using UnityEngine;

// Applies only to the FBX delivered with this script. Never writes mesh or UV data.
public sealed class TelegaMaterialImporter : AssetPostprocessor
{
    const string Root = "Assets/Obj/Telega_FBX_ready/";
    const string Model = Root + "Telega_materials_fixed.fbx";

    void OnPreprocessModel()
    {
        if (assetPath != Model) return;
        var importer = (ModelImporter)assetImporter;
        importer.generateSecondaryUV = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
    }

    public override int GetPostprocessOrder() => 10000;

    void OnPostprocessMaterial(Material source)
    {
        ConfigureMaterial(source);
    }

    Material ConfigureMaterial(Material source)
    {
        if (assetPath != Model) return null;
        bool wood = source.name == "Telega_Stylized_Wood";
        string texture = wood ? "Group 79.png" :
            (source.name == "Material.006" || source.name == "Material.008" ||
             source.name == "Material.012" || source.name == "Material.013" ||
             source.name == "Material.018" || source.name == "Material.022")
            ? "wheel_1.png" : "Nakonechnik.png";
        bool urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        var shader = Shader.Find(wood ? "Telega/Wood UV1" :
            urp ? "Universal Render Pipeline/Lit" : "Standard");
        if (shader == null) { Debug.LogError("Telega shader unavailable"); return null; }
        var material = source;
        material.shader = shader;
        var image = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/" + texture);
        if (image == null) { Debug.LogError("Telega texture unavailable: " + texture); return null; }
        material.SetTexture(wood || !urp ? "_MainTex" : "_BaseMap", image);
        material.SetFloat("_Smoothness", 0.22f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.22f);
        return material;
    }
}
