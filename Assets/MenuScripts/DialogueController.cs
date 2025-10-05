using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogueScreen dialogue; // arraste do Hierarchy
    [TextArea(3,10)] public string textoDeAbertura;

    void Start()
    {
        Debug.Log("[DialogueController] Start");
        if (dialogue == null)
        {
            Debug.LogError("[DialogueController] DialogueScreen não atribuído!");
            return;
        }
        if (!string.IsNullOrEmpty(textoDeAbertura))
            dialogue.texto = textoDeAbertura;

        dialogue.Show();
    }
}
