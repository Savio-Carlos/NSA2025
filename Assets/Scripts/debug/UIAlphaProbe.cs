using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIAlphaProbe : MonoBehaviour
{
    void Awake()
    {
        foreach (var cg in GetComponentsInParent<CanvasGroup>(true))
            Debug.Log($"CanvasGroup em {cg.name} alpha={cg.alpha}");

        foreach (var img in GetComponentsInChildren<Image>(true))
            Debug.Log($"Image {img.name} colorA={img.color.a}, mat={(img.material ? img.material.name : "default")}");

        foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            Debug.Log($"TMP {tmp.name} faceA={tmp.color.a}, vertexA={tmp.canvasRenderer.GetAlpha()}");
    }
}
