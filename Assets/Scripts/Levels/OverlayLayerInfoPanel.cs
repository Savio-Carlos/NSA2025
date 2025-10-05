using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class OverlayLayerInfoPanel : MonoBehaviour
{
    [SerializeField] private RegionLoader regionLoader;
    [SerializeField] private Component textComponent;
    [SerializeField] private Image iconImage;
    [SerializeField] private Component dayTextComponent;
    [SerializeField] private Component monthTextComponent;
    [SerializeField] private Component yearTextComponent;
    [SerializeField] private string dateFormat = "dd/MM/yyyy";
    [SerializeField] private string fallbackText = string.Empty;
    [SerializeField] private string dayFormat = "dd";
    [SerializeField] private string dayFallbackText = string.Empty;
    [SerializeField] private string monthFormat = "MM";
    [SerializeField] private string monthFallbackText = string.Empty;
    [SerializeField] private string yearFormat = "yyyy";
    [SerializeField] private string yearFallbackText = string.Empty;

    private Action<string> cachedCombinedTextSetter;
    private Action<string> cachedDayTextSetter;
    private Action<string> cachedMonthTextSetter;
    private Action<string> cachedYearTextSetter;
    private Action<Sprite> cachedIconSetter;
    private RegionLoader subscribedLoader;
    private Component iconSpriteComponent;

    private void Awake()
    {
        CacheLoader();
        CacheTextSetters();
    }

    private void OnValidate()
    {
        CacheLoader();
        CacheTextSetters();

        if (isActiveAndEnabled)
        {
            SubscribeToLoader(true);
            UpdateDisplay();
        }
    }

    private void OnEnable()
    {
        SubscribeToLoader(true);
        UpdateDisplay();
    }

    private void OnDisable()
    {
        SubscribeToLoader(false);
    }

    public void Refresh()
    {
        UpdateDisplay();
    }

    private void CacheLoader()
    {
        if (!regionLoader)
        {
            regionLoader = GetComponentInParent<RegionLoader>();
        }
    }

    private void CacheTextSetters()
    {
        CacheIconSetter();
        CacheCombinedTextSetter();
        CacheDatePartSetter(ref dayTextComponent, out cachedDayTextSetter);
        CacheDatePartSetter(ref monthTextComponent, out cachedMonthTextSetter);
        CacheDatePartSetter(ref yearTextComponent, out cachedYearTextSetter);
    }

    private void CacheIconSetter()
    {
        cachedIconSetter = null;
        iconSpriteComponent = iconImage;

        if (!iconSpriteComponent)
        {
            return;
        }

        if (TryBuildSpriteSetter(iconSpriteComponent, out cachedIconSetter))
        {
            return;
        }

        var owner = iconSpriteComponent.gameObject;
        var components = owner ? owner.GetComponents<Component>() : Array.Empty<Component>();

        foreach (var component in components)
        {
            if (!component || component == iconSpriteComponent)
            {
                continue;
            }

            if (!TryBuildSpriteSetter(component, out cachedIconSetter))
            {
                continue;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(this, "Assign Overlay Icon Component");
            }
#endif

            iconSpriteComponent = component;

            if (component is Image image)
            {
                iconImage = image;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            Debug.Log($"OverlayLayerInfoPanel: Reassigned icon component to '{component.GetType().Name}' on '{owner.name}'.", this);
            return;
        }

        Debug.LogWarning($"OverlayLayerInfoPanel: Unable to locate a component with a sprite property on '{owner?.name ?? "<unknown>"}'.", this);
    }

    private void CacheCombinedTextSetter()
    {
        cachedCombinedTextSetter = null;

        if (!textComponent)
        {
            return;
        }

        if (TryBuildTextSetter(textComponent, out cachedCombinedTextSetter))
        {
            return;
        }

        var owner = textComponent.gameObject;
        var components = owner ? owner.GetComponents<Component>() : Array.Empty<Component>();

        foreach (var component in components)
        {
            if (!component || component == textComponent)
            {
                continue;
            }

            if (!TryBuildTextSetter(component, out cachedCombinedTextSetter))
            {
                continue;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(this, "Assign Overlay Text Component");
            }
#endif

            textComponent = component;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            Debug.Log($"OverlayLayerInfoPanel: Reassigned text component to '{component.GetType().Name}' on '{owner.name}'.", this);
            return;
        }

        Debug.LogWarning($"OverlayLayerInfoPanel: Unable to locate a component with a text property on '{owner?.name ?? "<unknown>"}'.", this);
    }

    private void CacheDatePartSetter(ref Component component, out Action<string> setter)
    {
        setter = null;

        if (!component)
        {
            return;
        }

        if (TryBuildTextSetter(component, out setter))
        {
            return;
        }

        var owner = component.gameObject;
        var ownerName = owner ? owner.name : "<unknown>";
        var components = owner ? owner.GetComponents<Component>() : Array.Empty<Component>();

        foreach (var candidate in components)
        {
            if (!candidate || candidate == component)
            {
                continue;
            }

            if (!TryBuildTextSetter(candidate, out setter))
            {
                continue;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(this, "Assign Overlay Date Text Component");
            }
#endif

            component = candidate;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif

            Debug.Log($"OverlayLayerInfoPanel: Reassigned date text component to '{candidate.GetType().Name}' on '{ownerName}'.", this);
            return;
        }

        Debug.LogWarning($"OverlayLayerInfoPanel: Unable to locate a component with a text property on '{ownerName}' for date text.", this);
    }

    private bool TryBuildTextSetter(Component component, out Action<string> setter)
    {
        setter = null;

        if (!component)
        {
            return false;
        }

        var type = component.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        var property = type.GetProperty("text", flags);
        if (property != null && property.CanWrite)
        {
            setter = value => property.SetValue(component, value);
            return true;
        }

        var field = type.GetField("text", flags);
        if (field != null)
        {
            setter = value => field.SetValue(component, value);
            return true;
        }

        return false;
    }

    private bool TryBuildSpriteSetter(Component component, out Action<Sprite> setter)
    {
        setter = null;

        if (!component)
        {
            return false;
        }

        var type = component.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

        var property = type.GetProperty("sprite", flags);
        if (property != null && property.CanWrite && typeof(Sprite).IsAssignableFrom(property.PropertyType))
        {
            setter = value => property.SetValue(component, value);
            return true;
        }

        var field = type.GetField("sprite", flags);
        if (field != null && typeof(Sprite).IsAssignableFrom(field.FieldType))
        {
            setter = value => field.SetValue(component, value);
            return true;
        }

        return false;
    }

    private void SubscribeToLoader(bool subscribe)
    {
        if (subscribe)
        {
            if (regionLoader == subscribedLoader)
            {
                return;
            }

            if (subscribedLoader)
            {
                subscribedLoader.OverlayLayerChanged -= HandleOverlayLayerChanged;
            }

            if (regionLoader)
            {
                regionLoader.OverlayLayerChanged += HandleOverlayLayerChanged;
            }

            subscribedLoader = regionLoader;
        }
        else if (subscribedLoader)
        {
            subscribedLoader.OverlayLayerChanged -= HandleOverlayLayerChanged;
            subscribedLoader = null;
        }
    }

    private void HandleOverlayLayerChanged(RegionLevel.OverlayLayer overlayLayer)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var overlayLayer = regionLoader ? regionLoader.CurrentOverlayLayer : null;
        var overlayGroup = regionLoader ? regionLoader.CurrentOverlayGroup : null;
        var hasDate = false;
        var date = default(DateTime);

        if (overlayLayer != null)
        {
            hasDate = overlayLayer.TryGetDate(out date);
        }

        var formattedDate = hasDate ? date.ToString(dateFormat) : fallbackText;
        SetText(textComponent, ref cachedCombinedTextSetter, formattedDate);
        SetSprite(overlayGroup != null ? overlayGroup.icon : null);

        UpdateDatePart(dayTextComponent, ref cachedDayTextSetter, hasDate, date, dayFormat, dayFallbackText);
        UpdateMonthPart(monthTextComponent, ref cachedMonthTextSetter, hasDate, date, monthFormat, monthFallbackText);
        UpdateDatePart(yearTextComponent, ref cachedYearTextSetter, hasDate, date, yearFormat, yearFallbackText);
    }

    private void UpdateDatePart(Component component, ref Action<string> setter, bool hasDate, DateTime date, string format, string fallback)
    {
        var value = hasDate ? date.ToString(format) : (fallback ?? string.Empty);
        if (!hasDate && string.IsNullOrEmpty(fallback))
        {
            value = string.Empty;
        }

        SetText(component, ref setter, value);
    }

    private void UpdateMonthPart(Component component, ref Action<string> setter, bool hasDate, DateTime date, string format, string fallback)
    {
        string value;

        if (hasDate)
        {
            value = GetMonthAbbreviation(date, format);
        }
        else
        {
            value = fallback ?? string.Empty;

            if (string.IsNullOrEmpty(fallback))
            {
                value = string.Empty;
            }
        }

        SetText(component, ref setter, value);
    }

    private string GetMonthAbbreviation(DateTime date, string format)
    {
        if (!string.IsNullOrEmpty(format) && format.Length >= 3)
        {
            var formatted = date.ToString(format, CultureInfo.InvariantCulture);
            var length = Math.Min(3, formatted.Length);

            if (length > 0)
            {
                return formatted.Substring(0, length).ToUpperInvariant();
            }
        }

        return date.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant();
    }

    private void SetText(Component component, ref Action<string> setter, string value)
    {
        if (setter != null)
        {
            setter.Invoke(value ?? string.Empty);
            return;
        }

        if (!component)
        {
            return;
        }

        if (TryBuildTextSetter(component, out setter))
        {
            setter.Invoke(value ?? string.Empty);
            return;
        }

        var type = component.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        var property = type.GetProperty("text", flags);
        if (property != null && property.CanWrite)
        {
            property.SetValue(component, value);
            return;
        }

        var field = type.GetField("text", flags);
        if (field != null)
        {
            field.SetValue(component, value);
        }
    }

    private void UpdateIconVisibility(bool isVisible)
    {
        var targetComponent = iconSpriteComponent ? iconSpriteComponent : iconImage;

        if (!targetComponent)
        {
            return;
        }

        var owner = targetComponent.gameObject;

        if (!owner || owner == gameObject)
        {
            return;
        }

        if (owner.activeSelf != isVisible)
        {
            owner.SetActive(isVisible);
        }
    }

    private void SetSprite(Sprite value)
    {
        UpdateIconVisibility(value != null);

        if (cachedIconSetter != null)
        {
            cachedIconSetter.Invoke(value);
            return;
        }

        if (!iconSpriteComponent)
        {
            return;
        }

        if (TryBuildSpriteSetter(iconSpriteComponent, out cachedIconSetter))
        {
            cachedIconSetter.Invoke(value);
            return;
        }

        var type = iconSpriteComponent.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        var property = type.GetProperty("sprite", flags);
        if (property != null && property.CanWrite && typeof(Sprite).IsAssignableFrom(property.PropertyType))
        {
            property.SetValue(iconSpriteComponent, value);
            return;
        }

        var field = type.GetField("sprite", flags);
        if (field != null && typeof(Sprite).IsAssignableFrom(field.FieldType))
        {
            field.SetValue(iconSpriteComponent, value);
        }
    }
}
