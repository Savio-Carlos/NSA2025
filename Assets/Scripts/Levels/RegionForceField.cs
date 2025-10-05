using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(RegionLoader))]
public class RegionForceField : MonoBehaviour
{
    [SerializeField] private Material forceFieldMaterial;
    [SerializeField] private Color baseColor = new Color(0.3f, 0.6f, 1f, 1f);
    [SerializeField] private float fadeHeight = 50f;
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float intensity = 1f;

    private RegionLoader regionLoader;
    private Terrain currentTerrain;
    [SerializeField] private float wallThickness = 5f;

    private GameObject forceFieldInstance;
    private Material materialInstance;
    private readonly List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private readonly List<Transform> segmentTransforms = new List<Transform>();

    private void Awake()
    {
        regionLoader = GetComponent<RegionLoader>();
    }

    private void OnEnable()
    {
        if (!regionLoader)
        {
            regionLoader = GetComponent<RegionLoader>();
        }

        if (regionLoader)
        {
            regionLoader.TerrainCreated += HandleTerrainCreated;
            if (regionLoader.Terrain)
            {
                HandleTerrainCreated(regionLoader.Terrain);
            }
        }
    }

    private void OnDisable()
    {
        if (regionLoader)
        {
            regionLoader.TerrainCreated -= HandleTerrainCreated;
        }

        DestroyForceField();
    }

    private void OnDestroy()
    {
        if (regionLoader)
        {
            regionLoader.TerrainCreated -= HandleTerrainCreated;
        }

        DestroyForceField();
    }

    private void Update()
    {
        UpdateForceFieldTransform();
        UpdateMaterialProperties();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && isActiveAndEnabled)
        {
            UpdateMaterialProperties();
            UpdateForceFieldTransform();
        }
    }

    private void HandleTerrainCreated(Terrain terrain)
    {
        currentTerrain = terrain;

        if (!terrain)
        {
            DestroyForceField();
            return;
        }

        CreateForceField();
    }

    private void CreateForceField()
    {
        DestroyForceField();

        if (!currentTerrain || currentTerrain.terrainData == null || forceFieldMaterial == null)
        {
            return;
        }

        forceFieldInstance = new GameObject($"{currentTerrain.name}_ForceField");
        forceFieldInstance.transform.SetParent(currentTerrain.transform, false);
        forceFieldInstance.hideFlags = HideFlags.DontSave;

        meshRenderers.Clear();
        segmentTransforms.Clear();

        materialInstance = new Material(forceFieldMaterial)
        {
            name = forceFieldMaterial.name + " (Instance)"
        };
        materialInstance.hideFlags = HideFlags.DontSave;

        for (int i = 0; i < 4; i++)
        {
            var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = $"{forceFieldInstance.name}_Segment_{i}";
            segment.transform.SetParent(forceFieldInstance.transform, false);
            segment.hideFlags = HideFlags.DontSave;

            var collider = segment.GetComponent<Collider>();
            if (collider)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            var renderer = segment.GetComponent<MeshRenderer>();
            if (renderer)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.sharedMaterial = materialInstance;

                meshRenderers.Add(renderer);
            }

            segmentTransforms.Add(segment.transform);
        }

        UpdateForceFieldTransform();
        UpdateMaterialProperties();
    }

    private void UpdateForceFieldTransform()
    {
        if (!forceFieldInstance || currentTerrain == null || currentTerrain.terrainData == null)
        {
            return;
        }

        var terrainSize = currentTerrain.terrainData.size;
        forceFieldInstance.transform.localPosition = terrainSize * 0.5f;
        forceFieldInstance.transform.localRotation = Quaternion.identity;

        float wallHeight = Mathf.Max(terrainSize.y, 0.001f);
        float clampedThickness = Mathf.Max(0.001f, wallThickness);

        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            var segment = segmentTransforms[i];
            if (!segment)
            {
                continue;
            }

            segment.localRotation = Quaternion.identity;

            switch (i)
            {
                case 0: // North wall (+Z)
                    segment.localPosition = new Vector3(0f, 0f, (terrainSize.z - clampedThickness) * 0.5f);
                    segment.localScale = new Vector3(terrainSize.x, wallHeight, clampedThickness);
                    break;
                case 1: // South wall (-Z)
                    segment.localPosition = new Vector3(0f, 0f, -(terrainSize.z - clampedThickness) * 0.5f);
                    segment.localScale = new Vector3(terrainSize.x, wallHeight, clampedThickness);
                    break;
                case 2: // East wall (+X)
                    segment.localPosition = new Vector3((terrainSize.x - clampedThickness) * 0.5f, 0f, 0f);
                    segment.localScale = new Vector3(clampedThickness, wallHeight, terrainSize.z);
                    break;
                case 3: // West wall (-X)
                    segment.localPosition = new Vector3(-(terrainSize.x - clampedThickness) * 0.5f, 0f, 0f);
                    segment.localScale = new Vector3(clampedThickness, wallHeight, terrainSize.z);
                    break;
            }
        }
    }

    private void UpdateMaterialProperties()
    {
        if (!materialInstance)
        {
            return;
        }

        materialInstance.SetColor("_BaseColor", baseColor);
        materialInstance.SetFloat("_FadeHeight", Mathf.Max(0.001f, fadeHeight));
        materialInstance.SetFloat("_ScrollSpeed", scrollSpeed);
        materialInstance.SetFloat("_Intensity", intensity);
    }

    private void DestroyForceField()
    {
        foreach (var renderer in meshRenderers)
        {
            if (renderer)
            {
                renderer.sharedMaterial = null;
            }
        }

        if (materialInstance)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }

        if (forceFieldInstance)
        {
            if (Application.isPlaying)
            {
                Destroy(forceFieldInstance);
            }
            else
            {
                DestroyImmediate(forceFieldInstance);
            }
        }

        forceFieldInstance = null;
        materialInstance = null;
        meshRenderers.Clear();
        segmentTransforms.Clear();
    }
}
