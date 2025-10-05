using UnityEngine;

[RequireComponent(typeof(RegionLoader))]
public class OverlayController : MonoBehaviour
{
    [SerializeField]
    private RegionLoader loader;

    void Awake()
    {
        CacheLoader();
    }

    void OnValidate()
    {
        CacheLoader();
    }

    public void SelectGroup(int index)
    {
        CacheLoader();
        if (loader)
            loader.SetOverlayGroup(index);
    }

    public void SelectGroupByName(string groupName)
    {
        CacheLoader();
        if (loader)
            loader.SetOverlayGroup(groupName);
    }

    public void NextLayer()
    {
        CacheLoader();
        if (loader)
            loader.NextOverlay();
    }

    public void PreviousLayer()
    {
        CacheLoader();
        if (loader)
            loader.PreviousOverlay();
    }

    void CacheLoader()
    {
        if (!loader)
            loader = GetComponent<RegionLoader>();
    }
}
