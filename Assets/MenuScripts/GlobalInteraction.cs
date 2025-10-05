using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobeController : MonoBehaviour
{
    [Header("Rotação")]
    public float rotationSpeed = 50f;
    [Tooltip("Velocidade de rotação automática (graus por segundo)")]
    public float autoRotationSpeed = 5f;

    [Header("Lista de Materiais (defina no Inspector)")]
    public Material[] materiais;

    [Header("Renderer do Globo")]
    public Renderer globeRenderer;

    [Header("GameObject UI que contém o texto do nome do material")]
    public GameObject materialNameDisplay;

    private Text uiText;      
    private TMP_Text tmpText; 
    private int indiceAtual = 0;

    void Start()
    {
        // 🧭 Tenta achar o Renderer automaticamente
        if (globeRenderer == null)
            globeRenderer = GetComponent<Renderer>();

        if (globeRenderer == null)
            return;

        if (materialNameDisplay != null)
        {
            uiText = materialNameDisplay.GetComponentInChildren<Text>();
            tmpText = materialNameDisplay.GetComponentInChildren<TMP_Text>();

            if (uiText == null && tmpText == null)
                Debug.LogWarning("⚠️ Nenhum componente Text ou TMP_Text encontrado dentro do objeto de exibição!");
        }

        if (materiais != null && materiais.Length > 0)
        {
            globeRenderer.material = materiais[indiceAtual];
            AtualizarTextoMaterial();
        }
        else
        {
            Debug.LogWarning("⚠️ Nenhum material definido na lista!");
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, autoRotationSpeed * Time.deltaTime, Space.World);

        if (Input.GetMouseButton(0))
        {
            float x = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -x, Space.World);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (materiais == null || materiais.Length == 0)
                return;

            indiceAtual = (indiceAtual + 1) % materiais.Length;
            globeRenderer.material = materiais[indiceAtual];
            AtualizarTextoMaterial();
        }
    }

    void AtualizarTextoMaterial()
    {
        string nome = materiais[indiceAtual].name;

        if (uiText != null)
            uiText.text = $"{nome}";
        else if (tmpText != null)
            tmpText.text = $"{nome}";
    }
}
