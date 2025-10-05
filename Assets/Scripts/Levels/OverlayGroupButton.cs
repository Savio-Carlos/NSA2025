using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class OverlayGroupButton : MonoBehaviour
{
    [SerializeField] private RegionLoader regionLoader;
    [SerializeField] private Selectable selectable;
    [SerializeField] private int overlayGroupIndex = -1;
    [SerializeField] private string overlayGroupName = string.Empty;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite activeIcon;
    [SerializeField] private Sprite inactiveIcon;
    [SerializeField] private Graphic[] backgroundGraphics = Array.Empty<Graphic>();
    [SerializeField] private Graphic[] textGraphics = Array.Empty<Graphic>();
    [SerializeField] private Color activeBackgroundColor = Color.white;
    [SerializeField] private Color inactiveBackgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color activeTextColor = Color.black;
    [SerializeField] private Color inactiveTextColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private RegionLoader subscribedLoader;

    private void Awake()
    {
        CacheSelectable();
        CacheRegionLoader();
    }

    private void OnEnable()
    {
        CacheSelectable();
        CacheRegionLoader();
        SubscribeToLoader(true);
        UpdateVisualState();
    }

    private void OnDisable()
    {
        SubscribeToLoader(false);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            CacheSelectable();
            CacheRegionLoader();
            UpdateVisualState();
        }
    }

    private void CacheSelectable()
    {
        if (selectable)
        {
            return;
        }

        selectable = GetComponent<Selectable>();
    }

    private void CacheRegionLoader()
    {
        if (regionLoader && regionLoader != null)
        {
            return;
        }

        regionLoader = GetComponentInParent<RegionLoader>();
        if (regionLoader)
        {
            return;
        }

#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
        regionLoader = UnityEngine.Object.FindFirstObjectByType<RegionLoader>();
#else
        regionLoader = UnityEngine.Object.FindObjectOfType<RegionLoader>();
#endif
    }

    private void SubscribeToLoader(bool subscribe)
    {
        if (!subscribe)
        {
            if (subscribedLoader)
            {
                subscribedLoader.OverlayLayerChanged -= HandleOverlayLayerChanged;
                subscribedLoader = null;
            }

            return;
        }

        if (!regionLoader)
        {
            return;
        }

        if (subscribedLoader == regionLoader)
        {
            return;
        }

        if (subscribedLoader)
        {
            subscribedLoader.OverlayLayerChanged -= HandleOverlayLayerChanged;
        }

        regionLoader.OverlayLayerChanged += HandleOverlayLayerChanged;
        subscribedLoader = regionLoader;
    }

    private void HandleOverlayLayerChanged(RegionLevel.OverlayLayer _)
    {
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool isSelected = IsSelected();

        if (iconImage)
        {
            Sprite desiredSprite = isSelected ? activeIcon : inactiveIcon;
            if (iconImage.sprite != desiredSprite)
            {
                iconImage.sprite = desiredSprite;
            }
        }

        if (selectable)
        {
            bool shouldBeInteractable = !isSelected;
            if (selectable.interactable != shouldBeInteractable)
            {
                selectable.interactable = shouldBeInteractable;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(selectable);
                }
#endif
            }
        }

        ApplyColor(backgroundGraphics, isSelected ? activeBackgroundColor : inactiveBackgroundColor);
        ApplyColor(textGraphics, isSelected ? activeTextColor : inactiveTextColor);
    }

    private bool IsSelected()
    {
        if (!regionLoader)
        {
            return false;
        }

        if (overlayGroupIndex >= 0 && overlayGroupIndex == regionLoader.CurrentOverlayGroupIndex)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(overlayGroupName))
        {
            var currentGroup = regionLoader.CurrentOverlayGroup;
            if (currentGroup != null && !string.IsNullOrEmpty(currentGroup.groupName))
            {
                return string.Equals(currentGroup.groupName, overlayGroupName, StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    private static void ApplyColor(Graphic[] graphics, Color color)
    {
        if (graphics == null)
        {
            return;
        }

        for (int i = 0; i < graphics.Length; i++)
        {
            var graphic = graphics[i];
            if (!graphic)
            {
                continue;
            }

            if (graphic.color != color)
            {
                graphic.color = color;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(graphic);
                }
#endif
            }
        }
    }
}
