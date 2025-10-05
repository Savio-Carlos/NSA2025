#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;

public class RegionLevelWizard : EditorWindow
{
    string region = "BR-Region";
    string areaRef = "Area10km";
    TerrainData terrainData;
    Vector3 sizeMeters = new(10000f, 200f, 10000f);
    Vector2 tilingMeters = new(10000f, 10000f);
    RegionTerrainRecipe recipe;
    RegionTerrainRecipe lastRecipe;
    [SerializeField]
    private RegionLevel.OverlayGroup[] overlayGroups = Array.Empty<RegionLevel.OverlayGroup>();
    [SerializeField]
    private int defaultGroupIndex = 0;
    [SerializeField]
    private RegionLevel.MapMarker[] mapMarkers = Array.Empty<RegionLevel.MapMarker>();

    void OnEnable()
    {
        SanitizeOverlayGroups();
        SanitizeMapMarkers();
    }

    [MenuItem("Tools/Levels/Create RegionLevel (Wizard)")]
    public static void ShowWindow() => GetWindow<RegionLevelWizard>("Create RegionLevel");

    [MenuItem("Assets/Create/Game/Region Level (Wizard)")]
    public static void ShowFromAssetsMenu() => ShowWindow();

    void OnGUI()
    {
        recipe = (RegionTerrainRecipe)EditorGUILayout.ObjectField("Terrain Recipe", recipe, typeof(RegionTerrainRecipe), false);
        if (recipe != lastRecipe)
        {
            lastRecipe = recipe;
            if (recipe)
            {
                overlayGroups = RegionLevel.CloneOverlayGroups(recipe.overlayGroups);
                defaultGroupIndex = recipe.defaultOverlayGroupIndex;
                mapMarkers = RegionLevel.CloneMapMarkers(recipe.mapMarkers);
                SanitizeOverlayGroups();
                SanitizeMapMarkers();
            }
        }

        SanitizeOverlayGroups();
        SanitizeMapMarkers();

        if (recipe && GUILayout.Button("Generate Terrain & RegionLevel"))
        {
            try
            {
                var result = recipe.GenerateAssets(true);
                var generatedLevel = AssetDatabase.LoadAssetAtPath<RegionLevel>(result.regionLevelAssetPath);
                if (generatedLevel)
                {
                    region = generatedLevel.regionName;
                    areaRef = generatedLevel.areaRef;
                    terrainData = generatedLevel.terrainData;
                    sizeMeters = generatedLevel.terrainSizeMeters;
                    tilingMeters = generatedLevel.overlayTilingMeters;
                    overlayGroups = RegionLevel.CloneOverlayGroups(generatedLevel.overlayGroups);
                    defaultGroupIndex = generatedLevel.defaultGroupIndex;
                    mapMarkers = RegionLevel.CloneMapMarkers(generatedLevel.mapMarkers);
                    SanitizeMapMarkers();
                    Repaint();
                }
                else
                {
                    Debug.LogWarning("RegionLevelWizard: Generated RegionLevel asset could not be loaded.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"RegionLevelWizard: Failed to generate from recipe.\n{ex}");
            }
        }

        region = EditorGUILayout.TextField("Region", region);
        areaRef = EditorGUILayout.TextField("Area Ref", areaRef);
        terrainData = (TerrainData)EditorGUILayout.ObjectField("TerrainData", terrainData, typeof(TerrainData), false);
        sizeMeters = EditorGUILayout.Vector3Field("Terrain Size (m)", sizeMeters);
        tilingMeters = EditorGUILayout.Vector2Field("Overlay Tiling (m)", tilingMeters);
        SerializedObject so = new SerializedObject(this);
        so.Update();
        SerializedProperty groupsProp = so.FindProperty("overlayGroups");
        EditorGUILayout.PropertyField(groupsProp, new GUIContent("Overlay Groups", "Configure overlay layers and assign optional icons."), true);
        SerializedProperty defaultGroupProp = so.FindProperty("defaultGroupIndex");
        SerializedProperty markersProp = so.FindProperty("mapMarkers");
        EditorGUILayout.PropertyField(markersProp, new GUIContent("Map Markers", "Configure map markers displayed on the terrain."), true);
        int groupCount = groupsProp.arraySize;
        int desiredDefaultIndex = EditorGUILayout.IntField("Default Group Index", defaultGroupProp.intValue);
        if (groupCount > 0)
        {
            desiredDefaultIndex = Mathf.Clamp(desiredDefaultIndex, 0, groupCount - 1);
        }
        else
        {
            desiredDefaultIndex = 0;
        }
        defaultGroupProp.intValue = desiredDefaultIndex;
        so.ApplyModifiedProperties();

        if (GUILayout.Button("Create Asset"))
        {
            if (!terrainData) { Debug.LogError("TerrainData required."); return; }
            var folder = $"Assets/Levels/{region}/{areaRef}/Config";
            Directory.CreateDirectory(folder);
            var asset = ScriptableObject.CreateInstance<RegionLevel>();
            asset.regionName = region;
            asset.areaRef = areaRef;
            asset.terrainData = terrainData;
            asset.terrainSizeMeters = sizeMeters;
            asset.overlayTilingMeters = tilingMeters;
            asset.overlayGroups = RegionLevel.CloneOverlayGroups(overlayGroups);
            asset.defaultGroupIndex = Mathf.Clamp(defaultGroupIndex, 0, asset.overlayGroups != null && asset.overlayGroups.Length > 0 ? asset.overlayGroups.Length - 1 : 0);
            asset.mapMarkers = RegionLevel.CloneMapMarkers(mapMarkers);
            AssetDatabase.CreateAsset(asset, $"{folder}/{region}_{areaRef}_RegionLevel.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            Debug.Log("RegionLevel created at " + folder);
        }
    }

    private void SanitizeOverlayGroups()
    {
        if (overlayGroups == null)
        {
            overlayGroups = Array.Empty<RegionLevel.OverlayGroup>();
            defaultGroupIndex = 0;
            return;
        }

        foreach (var group in overlayGroups)
        {
            group?.Sanitize();
        }

        if (overlayGroups.Length == 0)
        {
            defaultGroupIndex = 0;
        }
        else
        {
            defaultGroupIndex = Mathf.Clamp(defaultGroupIndex, 0, overlayGroups.Length - 1);
        }
    }

    private void SanitizeMapMarkers()
    {
        if (mapMarkers == null)
        {
            mapMarkers = Array.Empty<RegionLevel.MapMarker>();
            return;
        }

        for (int i = 0; i < mapMarkers.Length; i++)
        {
            var marker = mapMarkers[i];
            marker.Sanitize();
            mapMarkers[i] = marker;
        }
    }
}
#endif
