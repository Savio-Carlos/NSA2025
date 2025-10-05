using UnityEngine;
using UnityEngine.UI;

public class UIDebugPrint : MonoBehaviour
{
    void Start()
    {
        var canvas = GetComponentInParent<Canvas>();
        var scaler = GetComponentInParent<CanvasScaler>();
        Debug.Log($"Screen: {Screen.width}x{Screen.height}  Aspect: {(float)Screen.width/Screen.height:F2}");
        if (canvas) Debug.Log($"Canvas: {canvas.renderMode}  pixelPerfect:{canvas.pixelPerfect}  scale:{canvas.transform.localScale}");
        if (scaler)
            Debug.Log($"Scaler: {scaler.uiScaleMode}  ref:{scaler.referenceResolution}  match:{scaler.matchWidthOrHeight:F2}  dynPPU:{scaler.dynamicPixelsPerUnit}");
        var cam = Camera.main;
        if (cam) Debug.Log($"Camera: HDR:{cam.allowHDR}  viewport:{cam.rect}");
    }
}
