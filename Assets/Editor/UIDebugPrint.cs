using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways] // imprime também fora do Play quando habilitar/selecionar
public class UIDebugPrint : MonoBehaviour
{
    void OnEnable()  { Log("OnEnable"); }
    void Awake()     { Log("Awake"); }
    void Start()     { Log("Start"); }

    [ContextMenu("Print UI Debug Now")]
    public void PrintNow() { Log("ContextMenu"); }

    void Log(string where)
    {
        var canvas = GetComponentInParent<Canvas>();
        var scaler = GetComponentInParent<CanvasScaler>();
        var cam = Camera.main;

        Debug.Log($"[UIDebugPrint::{where}] " +
                  $"Screen {Screen.width}x{Screen.height} ({(float)Screen.width/Screen.height:F2})");

        if (canvas)
            Debug.Log($"Canvas: {canvas.name} | mode:{canvas.renderMode} | pixelPerfect:{canvas.pixelPerfect} | scale:{canvas.transform.localScale}");

        if (scaler)
            Debug.Log($"Scaler: {scaler.uiScaleMode} | ref:{scaler.referenceResolution} | match:{scaler.matchWidthOrHeight:F2} | dynPPU:{scaler.dynamicPixelsPerUnit}");

        if (cam)
            Debug.Log($"Camera: {cam.name} | HDR:{cam.allowHDR} | viewport:{cam.rect}");
    }
}
