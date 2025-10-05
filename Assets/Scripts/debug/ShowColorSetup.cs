#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering; // Sempre existe (namespace base), mas tipos específicos podem não.

public static class ShowColorSetup
{
    [MenuItem("Tools/Debug/Show Color Setup")]
    public static void Show()
    {
        // 1) Color space
        Debug.Log($"Color Space: {PlayerSettings.colorSpace}");

        // 2) Câmeras e HDR
        foreach (var cam in Object.FindObjectsOfType<Camera>())
            Debug.Log($"Camera: {cam.name} | HDR: {cam.allowHDR}");

        // 3) Pipeline (URP) se presente
        var rp = GraphicsSettings.currentRenderPipeline;
        Debug.Log($"Render Pipeline Asset: {(rp ? rp.name : "None (Built-in)")}");

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        // Tenta pegar info extra do URP, se disponível
        var urpType = rp ? rp.GetType().FullName : "";
        Debug.Log($"URP Detected: {(string.IsNullOrEmpty(urpType) ? "No" : urpType)}");
#endif

        // 4) Volumes (URP) via reflexão segura
        TryListComponentByName("UnityEngine.Rendering.Volume", comp =>
        {
            var isGlobalProp = comp.GetType().GetField("isGlobal");
            var profileProp  = comp.GetType().GetProperty("sharedProfile");
            var isGlobal = isGlobalProp != null && (bool)isGlobalProp.GetValue(comp);
            var profile  = profileProp != null ? profileProp.GetValue(comp) : null;
            Debug.Log($"[URP Volume] {comp.name} | Global: {isGlobal} | Profile: {(profile != null ? profile.ToString() : "None")}");
        });

        // 5) Post-Processing v2 (Built-in) via reflexão segura
        TryListComponentByName("UnityEngine.Rendering.PostProcessing.PostProcessVolume", comp =>
        {
            var isGlobalProp = comp.GetType().GetField("isGlobal");
            var profileProp  = comp.GetType().GetField("sharedProfile");
            var isGlobal = isGlobalProp != null && (bool)isGlobalProp.GetValue(comp);
            var profile  = profileProp != null ? profileProp.GetValue(comp) : null;
            Debug.Log($"[PPv2 Volume] {comp.name} | Global: {isGlobal} | Profile: {(profile != null ? profile.ToString() : "None")}");
        });

        Debug.Log("Done. Compare as diferenças entre projetos no Console.");
    }

    // Procura componentes por nome totalmente qualificado sem referenciar o tipo em compile-time
    static void TryListComponentByName(string typeName, System.Action<Component> onEach)
    {
        var t = System.Type.GetType(typeName);
        if (t == null) return;
        var objs = Object.FindObjectsOfType(typeof(Component)) as Component[];
        foreach (var c in objs)
        {
            if (c != null && t.IsAssignableFrom(c.GetType()))
                onEach?.Invoke(c);
        }
    }
}
#endif
