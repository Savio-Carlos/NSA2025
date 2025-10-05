using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Game/Region Level", fileName = "RegionLevel")]
public class RegionLevel : ScriptableObject
{
    [Serializable]
    public class OverlayLayer
    {
        public Texture2D texture;
        public int day = 1;
        public int month = 1;
        public int year = 2000;

        public void Sanitize()
        {
            if (month < 1) month = 1;
            if (month > 12) month = 12;

            if (year < 1) year = 1;
            if (year > 9999) year = 9999;

            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (day < 1) day = 1;
            if (day > daysInMonth) day = daysInMonth;
        }

        public bool TryGetDate(out DateTime date)
        {
            try
            {
                date = new DateTime(year, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                date = default;
                return false;
            }
        }

        public string GetFormattedDate(string format = "yyyy-MM-dd")
        {
            return TryGetDate(out var date) ? date.ToString(format) : string.Empty;
        }

        public OverlayLayer Clone()
        {
            return new OverlayLayer
            {
                texture = texture,
                day = day,
                month = month,
                year = year
            };
        }

        public static OverlayLayer FromTexture(Texture2D legacyTexture)
        {
            return new OverlayLayer
            {
                texture = legacyTexture
            };
        }
    }

    [Serializable]
    public class OverlayGroup
    {
        public string groupName = "Default";
        public Sprite icon;
        public OverlayLayer[] layers = Array.Empty<OverlayLayer>();
        public int defaultLayerIndex = 0;

        [FormerlySerializedAs("layers")]
        [SerializeField, HideInInspector]
        private Texture2D[] legacyLayers = Array.Empty<Texture2D>();

        public OverlayLayer GetLayer(int index)
        {
            if (layers == null || layers.Length == 0)
            {
                return null;
            }

            index = Mathf.Clamp(index, 0, layers.Length - 1);
            return layers[index];
        }

        public OverlayLayer GetDefaultLayer()
        {
            if (layers == null || layers.Length == 0)
            {
                return null;
            }

            int index = Mathf.Clamp(defaultLayerIndex, 0, layers.Length - 1);
            return layers[index];
        }

        public OverlayLayer GetLayerData(int index)
        {
            return GetLayer(index);
        }

        public OverlayLayer GetDefaultLayerData()
        {
            return GetDefaultLayer();
        }

        public Texture2D GetLayerTexture(int index)
        {
            return GetLayer(index)?.texture;
        }

        public Texture2D GetDefaultLayerTexture()
        {
            return GetDefaultLayer()?.texture;
        }

        public void Sanitize()
        {
            MigrateLegacyLayersIfNeeded();

            if (layers == null)
            {
                layers = Array.Empty<OverlayLayer>();
            }

            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] == null)
                {
                    layers[i] = new OverlayLayer();
                }

                layers[i].Sanitize();
            }

            if (layers.Length == 0)
            {
                defaultLayerIndex = 0;
            }
            else
            {
                defaultLayerIndex = Mathf.Clamp(defaultLayerIndex, 0, layers.Length - 1);
            }
        }

        private void MigrateLegacyLayersIfNeeded()
        {
            if ((layers == null || layers.Length == 0) && legacyLayers != null && legacyLayers.Length > 0)
            {
                layers = new OverlayLayer[legacyLayers.Length];
                for (int i = 0; i < legacyLayers.Length; i++)
                {
                    layers[i] = OverlayLayer.FromTexture(legacyLayers[i]);
                }

                legacyLayers = Array.Empty<Texture2D>();
            }
        }
    }

    [Header("Identity")]
    public string regionName = "BR-Region";
    public string areaRef = "Area10km";

    [Header("Terrain")]
    public TerrainData terrainData;
    public Vector3 terrainSizeMeters = new Vector3(10000f, 200f, 10000f);
    public bool rawNormalizedToHeight = true;

    [Header("Overlays")]
    public OverlayGroup[] overlayGroups = Array.Empty<OverlayGroup>();
    public int defaultGroupIndex = 0;

    [FormerlySerializedAs("overlays")]
    [SerializeField, HideInInspector]
    private Texture2D[] overlaysLegacy;

    [FormerlySerializedAs("defaultOverlayIndex")]
    [SerializeField, HideInInspector]
    private int defaultOverlayIndexLegacy = 0;

    [Header("Overlay tiling (meters)")]
    public Vector2 overlayTilingMeters = new Vector2(10000f, 10000f);

    [Serializable]
    public struct MapMarker
    {
        public string displayName;
        [Range(0f, 1f)]
        public float normalizedX;
        [Range(0f, 1f)]
        public float normalizedZ;
        [Tooltip("Optional vertical offset in meters applied on top of the sampled terrain height.")]
        public float heightOffsetMeters;
        public Sprite icon;
        [Tooltip("Marker size in world space meters (X = width, Y = height).")]
        public Vector2 worldSizeMeters;

        public void Sanitize()
        {
            if (float.IsNaN(normalizedX) || float.IsInfinity(normalizedX))
            {
                normalizedX = 0f;
            }
            if (float.IsNaN(normalizedZ) || float.IsInfinity(normalizedZ))
            {
                normalizedZ = 0f;
            }

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);

            if (float.IsNaN(heightOffsetMeters) || float.IsInfinity(heightOffsetMeters))
            {
                heightOffsetMeters = 0f;
            }

            if (!float.IsNaN(worldSizeMeters.x) && !float.IsInfinity(worldSizeMeters.x))
            {
                worldSizeMeters.x = Mathf.Max(Mathf.Abs(worldSizeMeters.x), Mathf.Epsilon);
            }
            else
            {
                worldSizeMeters.x = 1f;
            }

            if (!float.IsNaN(worldSizeMeters.y) && !float.IsInfinity(worldSizeMeters.y))
            {
                worldSizeMeters.y = Mathf.Max(Mathf.Abs(worldSizeMeters.y), Mathf.Epsilon);
            }
            else
            {
                worldSizeMeters.y = 1f;
            }
        }
    }

    [Header("Map Markers")]
    public MapMarker[] mapMarkers = Array.Empty<MapMarker>();

    [Header("Optional info")]
    public float zMinMeters = 0f;
    public float zMaxMeters = 200f;

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

    public OverlayGroup GetOverlayGroup(int index)
    {
        EnsureOverlayGroups();
        if (overlayGroups == null || overlayGroups.Length == 0)
        {
            return null;
        }

        index = Mathf.Clamp(index, 0, overlayGroups.Length - 1);
        return overlayGroups[index];
    }

    public OverlayGroup GetDefaultOverlayGroup()
    {
        return GetOverlayGroup(defaultGroupIndex);
    }

    public Texture2D GetDefaultOverlayLayer()
    {
        EnsureOverlayGroups();
        var group = GetDefaultOverlayGroup();
        return group?.GetDefaultLayerTexture();
    }

    public Texture2D GetOverlayLayer(int groupIndex, int layerIndex)
    {
        EnsureOverlayGroups();
        var group = GetOverlayGroup(groupIndex);
        return group?.GetLayerTexture(layerIndex);
    }

    public OverlayLayer GetOverlayLayerData(int groupIndex, int layerIndex)
    {
        EnsureOverlayGroups();
        var group = GetOverlayGroup(groupIndex);
        return group?.GetLayer(layerIndex);
    }

    public OverlayLayer GetDefaultOverlayLayerData()
    {
        EnsureOverlayGroups();
        var group = GetDefaultOverlayGroup();
        return group?.GetDefaultLayer();
    }

    public static OverlayGroup[] CloneOverlayGroups(OverlayGroup[] source)
    {
        if (source == null)
        {
            return Array.Empty<OverlayGroup>();
        }

        var clone = new OverlayGroup[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == null)
            {
                clone[i] = new OverlayGroup();
                continue;
            }

            source[i].Sanitize();

            var layers = Array.Empty<OverlayLayer>();
            if (source[i].layers != null && source[i].layers.Length > 0)
            {
                layers = new OverlayLayer[source[i].layers.Length];
                for (int j = 0; j < source[i].layers.Length; j++)
                {
                    layers[j] = source[i].layers[j]?.Clone() ?? new OverlayLayer();
                    layers[j].Sanitize();
                }
            }

            clone[i] = new OverlayGroup
            {
                groupName = source[i].groupName,
                icon = source[i].icon,
                layers = layers,
                defaultLayerIndex = source[i].defaultLayerIndex
            };
            clone[i].Sanitize();
        }

        return clone;
    }

    public static MapMarker[] CloneMapMarkers(MapMarker[] source)
    {
        if (source == null || source.Length == 0)
        {
            return Array.Empty<MapMarker>();
        }

        var clone = new MapMarker[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            var marker = source[i];
            marker.Sanitize();
            clone[i] = marker;
        }

        return clone;
    }

    private void EnsureOverlayGroups()
    {
        if (overlayGroups == null || overlayGroups.Length == 0)
        {
            if (overlaysLegacy != null && overlaysLegacy.Length > 0)
            {
                overlayGroups = new[]
                {
                    new OverlayGroup
                    {
                        groupName = "Default",
                        layers = CreateLayersFromLegacyTextures(overlaysLegacy),
                        defaultLayerIndex = Mathf.Clamp(defaultOverlayIndexLegacy, 0, overlaysLegacy.Length - 1)
                    }
                };
                defaultGroupIndex = 0;
            }
            else if (overlayGroups == null)
            {
                overlayGroups = Array.Empty<OverlayGroup>();
            }
        }

        if (overlayGroups != null)
        {
            foreach (var group in overlayGroups)
            {
                group?.Sanitize();
            }
        }

        if (overlayGroups == null || overlayGroups.Length == 0)
        {
            defaultGroupIndex = 0;
        }
        else
        {
            defaultGroupIndex = Mathf.Clamp(defaultGroupIndex, 0, overlayGroups.Length - 1);
        }
    }

    private void EnsureMapMarkers()
    {
        if (mapMarkers == null)
        {
            mapMarkers = Array.Empty<MapMarker>();
            return;
        }

        for (int i = 0; i < mapMarkers.Length; i++)
        {
            var marker = mapMarkers[i];
            marker.Sanitize();
            mapMarkers[i] = marker;
        }
    }

    internal static OverlayLayer[] CreateLayersFromLegacyTextures(Texture2D[] textures)
    {
        if (textures == null || textures.Length == 0)
        {
            return Array.Empty<OverlayLayer>();
        }

        var result = new OverlayLayer[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            result[i] = OverlayLayer.FromTexture(textures[i]);
            result[i].Sanitize();
        }

        return result;
    }
}
