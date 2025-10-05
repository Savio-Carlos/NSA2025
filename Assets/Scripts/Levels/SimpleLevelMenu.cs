using UnityEngine;

public class SimpleLevelMenu : MonoBehaviour
{
    public RegionLevel[] levels;
    public RegionLoader loaderPrefab;
    private RegionLoader current;

    public void Load(int index)
    {
        if (levels == null || levels.Length == 0) { Debug.LogWarning("No levels assigned."); return; }
        if (index < 0 || index >= levels.Length) index = 0;

        if (current) Destroy(current.gameObject);
        current = Instantiate(loaderPrefab);
        current.name = "RegionLoaderRuntime";
        current.SendMessage("LoadRegion", levels[index], SendMessageOptions.DontRequireReceiver);
    }
}