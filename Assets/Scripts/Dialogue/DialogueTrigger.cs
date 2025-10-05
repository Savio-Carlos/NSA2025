using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Ink Knot")]
    [Tooltip("O nome do 'knot' no arquivo Ink para iniciar o di�logo.")]
    [SerializeField] private string knotName;

    private void OnMouseDown()
    {
        Debug.Log("CLIQUE DETECTADO NA CÁPSULA!");

        if (GameEventsManager.instance == null) {
            Debug.LogError("FALHA CRÍTICA: O DialogueTrigger não consegue encontrar o GameEventsManager.instance!");
            return;
        }

        // A linha que dispara o evento
        GameEventsManager.instance.DialogueEvents.enterDialogue(knotName);
    }

    
}