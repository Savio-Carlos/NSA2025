#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RegionTerrainRecipe))]
public class RegionTerrainRecipeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        var recipe = (RegionTerrainRecipe)target;
        if (GUILayout.Button("Generate Terrain & RegionLevel"))
        {
            try
            {
                recipe.GenerateAssets(true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RegionTerrainRecipe: Failed to generate assets.\n{ex}");
            }
        }
    }
}
#endif
