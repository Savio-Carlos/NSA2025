using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class RegionLoader : MonoBehaviour
{
    [SerializeField] private RegionLevel regionLevel;

    [Serializable]
    public class OverlayLayerUnityEvent : UnityEvent<RegionLevel.OverlayLayer> { }

    [SerializeField] private OverlayLayerUnityEvent onOverlayLayerChanged = new OverlayLayerUnityEvent();

    private Terrain terrain;
    private TerrainLayer baseLayer;
    private Coroutine runtimeLoadCoroutine;
    private RegionLevel pendingRuntimeLevel;
    private int currentGroupIndex;
    private int currentLayerIndex;
    private Transform markerRoot;

    public event Action<RegionLevel.OverlayLayer> OverlayLayerChanged;
    public event Action<Terrain> TerrainCreated;

    public RegionLevel.OverlayLayer CurrentOverlayLayer => GetCurrentOverlayLayer();
    public RegionLevel.OverlayGroup CurrentOverlayGroup => regionLevel?.GetOverlayGroup(currentGroupIndex);
    public int CurrentOverlayGroupIndex => currentGroupIndex;
    public Terrain Terrain => terrain;

    void Start()
    {
        if (regionLevel) LoadRegion(regionLevel);
    }

    public void LoadRegion(RegionLevel level)
    {
        regionLevel = level;

        if (!regionLevel) { Debug.LogError("RegionLoader: RegionLevel missing."); return; }
        if (!regionLevel.terrainData) { Debug.LogError("RegionLoader: TerrainData missing."); return; }

        ResetOverlayState(regionLevel);

        if (Application.isPlaying)
        {
            QueueRuntimeLoad(regionLevel);
        }
        else
        {
            DestroyExistingTerrainImmediate();
            CreateTerrain(regionLevel);
        }
    }

    private void QueueRuntimeLoad(RegionLevel level)
    {
        pendingRuntimeLevel = level;

        if (runtimeLoadCoroutine == null)
        {
            runtimeLoadCoroutine = StartCoroutine(RuntimeLoad());
        }
    }

    private System.Collections.IEnumerator RuntimeLoad()
    {
        while (pendingRuntimeLevel != null)
        {
            var level = pendingRuntimeLevel;
            pendingRuntimeLevel = null;

            var existing = FindExistingTerrain();
            if (existing)
            {
                TerrainCreated?.Invoke(null);
                ClearMarkerRoot();
                Destroy(existing.gameObject);

                // Wait for destruction to be processed before creating the new terrain
                do
                {
                    yield return null;
                } while (existing != null);
            }

            CreateTerrain(level);
        }

        runtimeLoadCoroutine = null;
    }

    private Terrain FindExistingTerrain()
    {
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        return Object.FindFirstObjectByType<Terrain>();
#else
        return Object.FindObjectOfType<Terrain>();
#endif
    }

    private void DestroyExistingTerrainImmediate()
    {
        var existing = FindExistingTerrain();
        if (existing)
        {
            TerrainCreated?.Invoke(null);
            ClearMarkerRoot();
            DestroyImmediate(existing.gameObject);
        }
    }

    private void OnDestroy()
    {
        ClearMarkerRoot();
    }

    private void CreateTerrain(RegionLevel level)
    {
        var go = Terrain.CreateTerrainGameObject(level.terrainData);
        go.name = $"{level.regionName}_{level.areaRef}_Terrain";
        terrain = go.GetComponent<Terrain>();
        terrain.terrainData.size = level.terrainSizeMeters;
        terrain.basemapDistance = 20000f;
        terrain.drawInstanced = true;

        TerrainCreated?.Invoke(terrain);

        // Base TerrainLayer for overlay
        baseLayer = new TerrainLayer();
        baseLayer.tileSize = level.overlayTilingMeters;
        baseLayer.tileOffset = Vector2.zero;

        var layers = terrain.terrainData.terrainLayers;
        if (layers == null || layers.Length == 0) layers = new TerrainLayer[] { baseLayer };
        else layers[0] = baseLayer;
        terrain.terrainData.terrainLayers = layers;

        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);

        RefreshMarkers(level);

        Debug.Log($"Loaded {level.regionName}/{level.areaRef}");
    }

    private void RefreshMarkers(RegionLevel level)
    {
        ClearMarkerRoot();

        if (!terrain || !terrain.terrainData || level == null || level.mapMarkers == null || level.mapMarkers.Length == 0)
        {
            return;
        }

        var root = new GameObject($"{terrain.name}_Markers");
        root.transform.SetParent(terrain.transform, false);
        root.hideFlags = HideFlags.DontSave;
        markerRoot = root.transform;

        var terrainData = terrain.terrainData;
        foreach (var marker in level.mapMarkers)
        {
            if (!marker.icon)
            {
                continue;
            }

            var normalizedX = Mathf.Clamp01(marker.normalizedX);
            var normalizedZ = Mathf.Clamp01(marker.normalizedZ);
            float worldX = normalizedX * terrainData.size.x;
            float worldZ = normalizedZ * terrainData.size.z;
            float worldY = terrainData.GetInterpolatedHeight(normalizedX, normalizedZ) + marker.heightOffsetMeters;
            var position = new Vector3(worldX, worldY, worldZ);

            var markerName = string.IsNullOrEmpty(marker.displayName) ? "MapMarker" : marker.displayName;
            var markerGo = new GameObject(markerName);
            markerGo.transform.SetParent(markerRoot, false);
            markerGo.transform.localPosition = position;
            markerGo.transform.localRotation = Quaternion.identity;
            markerGo.hideFlags = HideFlags.DontSave;

            var spriteRenderer = markerGo.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = marker.icon;
            bool useSliced = marker.icon.border.sqrMagnitude > 0f;
            spriteRenderer.drawMode = useSliced ? SpriteDrawMode.Sliced : SpriteDrawMode.Simple;

            var spriteBounds = marker.icon.bounds.size;
            if (spriteBounds.x <= Mathf.Epsilon || spriteBounds.y <= Mathf.Epsilon)
            {
                spriteBounds = Vector3.one;
            }

            if (useSliced)
            {
                spriteRenderer.size = marker.worldSizeMeters;
                markerGo.transform.localScale = Vector3.one;
            }
            else
            {
                markerGo.transform.localScale = new Vector3(
                    marker.worldSizeMeters.x / spriteBounds.x,
                    marker.worldSizeMeters.y / spriteBounds.y,
                    1f);
            }

            var billboard = markerGo.AddComponent<RegionMarkerBillboard>();
            billboard.Initialize(this);
        }
    }

    private void ClearMarkerRoot()
    {
        if (!markerRoot)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(markerRoot.gameObject);
        }
        else
        {
            DestroyImmediate(markerRoot.gameObject);
        }

        markerRoot = null;
    }

    public void SetOverlay(int groupIndex)
    {
        if (!regionLevel) return;

        int groupCount = GetOverlayGroupCount();
        if (groupCount == 0)
        {
            currentGroupIndex = 0;
            currentLayerIndex = 0;
            ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
            return;
        }

        currentGroupIndex = Mathf.Clamp(groupIndex, 0, groupCount - 1);
        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        if (group == null || group.layers == null || group.layers.Length == 0)
        {
            currentLayerIndex = 0;
            ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
            return;
        }

        currentLayerIndex = Mathf.Clamp(currentLayerIndex, 0, group.layers.Length - 1);
        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
    }

    public void SetOverlay(int groupIndex, int layerIndex)
    {
        currentGroupIndex = groupIndex;
        currentLayerIndex = layerIndex;
        SetOverlay(currentGroupIndex);
    }

    public void SetOverlayGroup(int groupIndex)
    {
        if (!regionLevel) return;

        int groupCount = GetOverlayGroupCount();
        if (groupCount == 0)
        {
            currentGroupIndex = 0;
            currentLayerIndex = 0;
            ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
            return;
        }

        currentGroupIndex = Mathf.Clamp(groupIndex, 0, groupCount - 1);
        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        if (group != null && group.layers != null && group.layers.Length > 0)
        {
            currentLayerIndex = Mathf.Clamp(group.defaultLayerIndex, 0, group.layers.Length - 1);
        }
        else
        {
            currentLayerIndex = 0;
        }

        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
    }

    public void SetOverlayGroup(string groupName)
    {
        if (!regionLevel || string.IsNullOrEmpty(groupName)) return;

        var groups = regionLevel.overlayGroups;
        if (groups == null || groups.Length == 0) return;

        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] != null && string.Equals(groups[i].groupName, groupName, StringComparison.OrdinalIgnoreCase))
            {
                SetOverlayGroup(i);
                return;
            }
        }
    }

    public void SetOverlayLayer(int layerIndex)
    {
        if (!regionLevel) return;

        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        if (group == null || group.layers == null || group.layers.Length == 0) return;

        currentLayerIndex = Mathf.Clamp(layerIndex, 0, group.layers.Length - 1);
        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
    }

    public void NextOverlay()
    {
        if (!regionLevel) return;

        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        if (group == null || group.layers == null || group.layers.Length == 0) return;

        int count = group.layers.Length;
        currentLayerIndex = (currentLayerIndex + 1) % count;
        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
    }

    public void PreviousOverlay()
    {
        if (!regionLevel) return;

        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        if (group == null || group.layers == null || group.layers.Length == 0) return;

        int count = group.layers.Length;
        currentLayerIndex = (currentLayerIndex - 1 + count) % count;
        ApplyOverlayTexture(currentGroupIndex, currentLayerIndex);
    }

    public RegionLevel.OverlayLayer GetCurrentOverlayLayer()
    {
        if (!regionLevel)
        {
            return null;
        }

        var group = regionLevel.GetOverlayGroup(currentGroupIndex);
        return group?.GetLayerData(currentLayerIndex);
    }

    private void ResetOverlayState(RegionLevel level)
    {
        currentGroupIndex = 0;
        currentLayerIndex = 0;

        if (level == null)
        {
            return;
        }

        int groupCount = GetOverlayGroupCount(level);
        if (groupCount == 0)
        {
            return;
        }

        currentGroupIndex = Mathf.Clamp(level.defaultGroupIndex, 0, groupCount - 1);
        var group = level.GetOverlayGroup(currentGroupIndex);
        if (group != null && group.layers != null && group.layers.Length > 0)
        {
            currentLayerIndex = Mathf.Clamp(group.defaultLayerIndex, 0, group.layers.Length - 1);
        }
        else
        {
            currentLayerIndex = 0;
        }
    }

    private int GetOverlayGroupCount()
    {
        return GetOverlayGroupCount(regionLevel);
    }

    private int GetOverlayGroupCount(RegionLevel level)
    {
        if (level == null)
        {
            return 0;
        }

        level.GetOverlayGroup(level.defaultGroupIndex);
        return level.overlayGroups?.Length ?? 0;
    }

    private void ApplyOverlayTexture(int groupIndex, int layerIndex)
    {
        if (!terrain || terrain.terrainData == null)
        {
            return;
        }

        Texture2D texture = Texture2D.blackTexture;

        if (regionLevel)
        {
            var groups = regionLevel.overlayGroups;
            if (groups != null && groups.Length > 0)
            {
                groupIndex = Mathf.Clamp(groupIndex, 0, groups.Length - 1);
                var group = regionLevel.GetOverlayGroup(groupIndex);
                if (group != null && group.layers != null && group.layers.Length > 0)
                {
                    layerIndex = Mathf.Clamp(layerIndex, 0, group.layers.Length - 1);
                    texture = group.GetLayerTexture(layerIndex) ?? Texture2D.blackTexture;
                    currentLayerIndex = layerIndex;
                }
                else
                {
                    currentLayerIndex = 0;
                }

                currentGroupIndex = groupIndex;
            }
            else
            {
                currentGroupIndex = 0;
                currentLayerIndex = 0;
            }
        }
        else
        {
            currentGroupIndex = 0;
            currentLayerIndex = 0;
        }

        var layers = terrain.terrainData.terrainLayers;
        if (layers == null || layers.Length == 0)
        {
            baseLayer ??= new TerrainLayer();
            baseLayer.tileSize = regionLevel ? regionLevel.overlayTilingMeters : Vector2.one;
            baseLayer.tileOffset = Vector2.zero;
            layers = new TerrainLayer[] { baseLayer };
        }

        if (baseLayer == null)
        {
            baseLayer = layers[0] ?? new TerrainLayer();
        }

        baseLayer.diffuseTexture = texture;
        layers[0] = baseLayer;
        terrain.terrainData.terrainLayers = layers;

        RaiseOverlayLayerChanged();
    }

    private void RaiseOverlayLayerChanged()
    {
        var overlayLayer = GetCurrentOverlayLayer();
        OverlayLayerChanged?.Invoke(overlayLayer);
        onOverlayLayerChanged?.Invoke(overlayLayer);
    }
}
