using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Region Terrain Recipe", fileName = "RegionTerrainRecipe")]
public class RegionTerrainRecipe : ScriptableObject
{
    [Header("RAW Heightmap")]
    [SerializeField]
    private DefaultAsset rawFile;
    public DefaultAsset RawFile => rawFile;
    public int heightmapWidth = 1025;
    public int heightmapHeight = 1025;
    [Tooltip("Bit depth per sample (8 or 16).")]
    public int bitDepth = 16;
    [Tooltip("Treat RAW samples as big endian (defaults to little endian).")]
    public bool bigEndian = false;
    [Tooltip("Flip RAW rows vertically when importing (useful for GIS exports).")]
    public bool flipVertically = true;

    [Header("Terrain Settings")]
    public Vector2 terrainSizeMetersXZ = new Vector2(10000f, 10000f);
    public float maxTerrainHeightMeters = 200f;

    [Header("Region Metadata")]
    public string regionName = "BR-Region";
    public string areaRef = "Area10km";
    public Vector2 overlayTilingMeters = new Vector2(10000f, 10000f);
    public RegionLevel.OverlayGroup[] overlayGroups = Array.Empty<RegionLevel.OverlayGroup>();
    public int defaultOverlayGroupIndex = 0;
    [FormerlySerializedAs("overlays")]
    [SerializeField, HideInInspector]
    private Texture2D[] overlaysLegacy;
    [FormerlySerializedAs("defaultOverlayIndex")]
    [SerializeField, HideInInspector]
    private int defaultOverlayIndexLegacy = 0;
    [Header("Map Markers")]
    public RegionLevel.MapMarker[] mapMarkers = Array.Empty<RegionLevel.MapMarker>();
    public float zMinMeters = 0f;
    public float zMaxMeters = 200f;

    [Header("Asset Output")]
    [Tooltip("Root folder (must start with Assets) used to place generated assets.")]
    public string outputRootFolder = "Assets/Levels";
    public string terrainSubfolderName = "Terrain";
    public string regionLevelSubfolderName = "Config";
    public string terrainAssetFileName = "TerrainData.asset";
    public string regionLevelAssetFileName = "RegionLevel.asset";

#if UNITY_EDITOR
    [Serializable]
    public struct GenerationResult
    {
        public string terrainAssetPath;
        public bool terrainCreated;
        public string regionLevelAssetPath;
        public bool regionLevelCreated;
        public float minHeightMeters;
        public float maxHeightMeters;
    }

    [ContextMenu("Generate Terrain & RegionLevel")]
    public void GenerateAssetsFromContextMenu()
    {
        try
        {
            var result = GenerateAssetsInternal();
            LogSuccess(result);
        }
        catch (Exception ex)
        {
            Debug.LogError($"RegionTerrainRecipe: Failed to generate assets.\n{ex}");
        }
    }

    public GenerationResult GenerateAssets(bool logSuccess)
    {
        var result = GenerateAssetsInternal();
        if (logSuccess)
        {
            LogSuccess(result);
        }
        return result;
    }

    private GenerationResult GenerateAssetsInternal()
    {
        if (heightmapWidth <= 0 || heightmapHeight <= 0)
        {
            throw new InvalidOperationException("Heightmap dimensions must be positive.");
        }

        if (heightmapWidth != heightmapHeight)
        {
            throw new InvalidOperationException("Unity terrains require square heightmaps (width == height).");
        }

        if (bitDepth != 8 && bitDepth != 16)
        {
            throw new InvalidOperationException("Only 8-bit and 16-bit RAW files are supported.");
        }

        if (string.IsNullOrEmpty(outputRootFolder) || !outputRootFolder.StartsWith("Assets", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Output root folder must start with 'Assets'.");
        }

        if (!rawFile)
        {
            throw new InvalidOperationException("RAW heightmap asset is required.");
        }

        string path = AssetDatabase.GetAssetPath(rawFile);
        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidOperationException("Unable to determine path for RAW heightmap asset.");
        }

        if (!path.EndsWith(".raw", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"RAW heightmap asset must be a .raw or .bytes file (got '{path}').");
        }

        int bytesPerSample = bitDepth / 8;
        int expectedSamples = heightmapWidth * heightmapHeight;
        int expectedBytes = expectedSamples * bytesPerSample;
        byte[] bytes = File.ReadAllBytes(path);

        if (bytes.Length < expectedBytes)
        {
            throw new InvalidOperationException($"RAW file is smaller than expected ({bytes.Length} bytes, expected {expectedBytes}).");
        }

        float[,] heights = new float[heightmapHeight, heightmapWidth];
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        using (var stream = new MemoryStream(bytes))
        using (var reader = new BinaryReader(stream))
        {
            for (int y = 0; y < heightmapHeight; y++)
            {
                for (int x = 0; x < heightmapWidth; x++)
                {
                    float normalized = ReadNormalizedSample(reader);
                    int targetY = flipVertically ? (heightmapHeight - 1 - y) : y;
                    heights[targetY, x] = normalized;

                    if (normalized < minValue) minValue = normalized;
                    if (normalized > maxValue) maxValue = normalized;
                }
            }
        }

        if (float.IsNaN(minValue) || float.IsInfinity(minValue)) minValue = 0f;
        if (float.IsNaN(maxValue) || float.IsInfinity(maxValue)) maxValue = 1f;

        string regionRoot = CombineAssetPath(outputRootFolder, regionName, areaRef);
        string terrainFolder = CombineAssetPath(regionRoot, terrainSubfolderName);
        string regionLevelFolder = CombineAssetPath(regionRoot, regionLevelSubfolderName);
        EnsureFolderExists(terrainFolder);
        EnsureFolderExists(regionLevelFolder);

        string terrainAssetPath = CombineAssetPath(terrainFolder, EnsureAssetFileName(terrainAssetFileName, $"{regionName}_{areaRef}_TerrainData.asset"));
        string regionLevelAssetPath = CombineAssetPath(regionLevelFolder, EnsureAssetFileName(regionLevelAssetFileName, $"{regionName}_{areaRef}_RegionLevel.asset"));

        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(terrainAssetPath);
        bool createdTerrainAsset = false;
        if (!terrainData)
        {
            terrainData = new TerrainData();
            AssetDatabase.CreateAsset(terrainData, terrainAssetPath);
            createdTerrainAsset = true;
        }

        terrainData.heightmapResolution = heightmapWidth;
        terrainData.size = new Vector3(terrainSizeMetersXZ.x, maxTerrainHeightMeters, terrainSizeMetersXZ.y);
        terrainData.SetHeights(0, 0, heights);
        EditorUtility.SetDirty(terrainData);

        RegionLevel regionLevel = AssetDatabase.LoadAssetAtPath<RegionLevel>(regionLevelAssetPath);
        bool createdRegionLevel = false;
        if (!regionLevel)
        {
            regionLevel = ScriptableObject.CreateInstance<RegionLevel>();
            AssetDatabase.CreateAsset(regionLevel, regionLevelAssetPath);
            createdRegionLevel = true;
        }

        regionLevel.regionName = regionName;
        regionLevel.areaRef = areaRef;
        regionLevel.terrainData = terrainData;
        regionLevel.terrainSizeMeters = new Vector3(terrainSizeMetersXZ.x, maxTerrainHeightMeters, terrainSizeMetersXZ.y);
        regionLevel.rawNormalizedToHeight = true;
        EnsureOverlayGroups();
        EnsureMapMarkers();
        var clonedGroups = RegionLevel.CloneOverlayGroups(overlayGroups);
        regionLevel.overlayGroups = clonedGroups;
        regionLevel.defaultGroupIndex = ClampDefaultGroupIndex(defaultOverlayGroupIndex, clonedGroups);
        regionLevel.overlayTilingMeters = overlayTilingMeters;
        regionLevel.mapMarkers = RegionLevel.CloneMapMarkers(mapMarkers);

        float minHeight = minValue * maxTerrainHeightMeters;
        float maxHeight = maxValue * maxTerrainHeightMeters;
        regionLevel.zMinMeters = minHeight;
        regionLevel.zMaxMeters = maxHeight;

        zMinMeters = minHeight;
        zMaxMeters = maxHeight;

        EditorUtility.SetDirty(regionLevel);
        EditorUtility.SetDirty(this);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return new GenerationResult
        {
            terrainAssetPath = terrainAssetPath,
            terrainCreated = createdTerrainAsset,
            regionLevelAssetPath = regionLevelAssetPath,
            regionLevelCreated = createdRegionLevel,
            minHeightMeters = minHeight,
            maxHeightMeters = maxHeight
        };
    }

    private void LogSuccess(GenerationResult result)
    {
        Debug.Log($"RegionTerrainRecipe: Terrain data {(result.terrainCreated ? "created" : "updated")} at {result.terrainAssetPath}; RegionLevel {(result.regionLevelCreated ? "created" : "updated")} at {result.regionLevelAssetPath}. Height range: {result.minHeightMeters:F2}m - {result.maxHeightMeters:F2}m.");
    }

    private void OnEnable()
    {
        EnsureOverlayGroups();
        EnsureMapMarkers();
    }

    private void OnValidate()
    {
        EnsureOverlayGroups();
        EnsureMapMarkers();
    }

    private void EnsureOverlayGroups()
    {
        if (overlayGroups == null || overlayGroups.Length == 0)
        {
            if (overlaysLegacy != null && overlaysLegacy.Length > 0)
            {
                overlayGroups = new[]
                {
                    new RegionLevel.OverlayGroup
                    {
                        groupName = "Default",
                        layers = RegionLevel.CreateLayersFromLegacyTextures(overlaysLegacy),
                        defaultLayerIndex = Mathf.Clamp(defaultOverlayIndexLegacy, 0, overlaysLegacy.Length - 1)
                    }
                };
                defaultOverlayGroupIndex = 0;
                overlaysLegacy = Array.Empty<Texture2D>();
            }
            else if (overlayGroups == null)
            {
                overlayGroups = Array.Empty<RegionLevel.OverlayGroup>();
            }
        }

        if (overlayGroups != null)
        {
            foreach (var group in overlayGroups)
            {
                group?.Sanitize();
            }
        }

        defaultOverlayGroupIndex = ClampDefaultGroupIndex(defaultOverlayGroupIndex, overlayGroups);
    }

    private void EnsureMapMarkers()
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

    private static int ClampDefaultGroupIndex(int index, RegionLevel.OverlayGroup[] groups)
    {
        if (groups == null || groups.Length == 0)
        {
            return 0;
        }

        return Mathf.Clamp(index, 0, groups.Length - 1);
    }

    private float ReadNormalizedSample(BinaryReader reader)
    {
        switch (bitDepth)
        {
            case 8:
                return reader.ReadByte() / 255f;
            case 16:
                var sampleBytes = reader.ReadBytes(2);
                if (sampleBytes.Length < 2)
                {
                    throw new EndOfStreamException("Unexpected end of RAW stream (16-bit sample).");
                }
                if (bigEndian)
                {
                    Array.Reverse(sampleBytes);
                }
                ushort value = BitConverter.ToUInt16(sampleBytes, 0);
                return value / 65535f;
            default:
                throw new NotSupportedException($"Bit depth {bitDepth} is not supported.");
        }
    }

    private static string CombineAssetPath(params string[] parts)
    {
        string result = string.Empty;
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;
            if (string.IsNullOrEmpty(result))
            {
                result = part.Trim().TrimEnd('/');
            }
            else
            {
                result = result.TrimEnd('/') + "/" + part.Trim().Trim('/');
            }
        }
        return result.Replace("\\", "/");
    }

    private static string EnsureAssetFileName(string providedName, string fallback)
    {
        if (string.IsNullOrWhiteSpace(providedName))
        {
            return fallback;
        }

        string trimmed = providedName.Trim();
        if (!trimmed.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
        {
            trimmed += ".asset";
        }
        return trimmed;
    }

    private static void EnsureFolderExists(string assetFolder)
    {
        assetFolder = assetFolder.Replace("\\", "/").TrimEnd('/');
        if (string.IsNullOrEmpty(assetFolder) || assetFolder == "Assets")
        {
            return;
        }

        if (AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        int slashIndex = assetFolder.LastIndexOf('/');
        if (slashIndex < 0)
        {
            throw new InvalidOperationException($"Cannot create folder for path '{assetFolder}'.");
        }

        string parent = assetFolder.Substring(0, slashIndex);
        string name = assetFolder.Substring(slashIndex + 1);
        EnsureFolderExists(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
#endif
}
