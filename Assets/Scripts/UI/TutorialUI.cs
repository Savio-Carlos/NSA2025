using UnityEngine;

public class TutorialUI : MonoBehaviour
{
        // Variável pública para referenciar o GameObject que você deseja ativar/desativar
    public GameObject objectToToggle;

    // Função pública que será chamada pelo botão
    public void Toggle()
    {
        // Verifica se o objeto de referência foi atribuído no Inspector
        if (objectToToggle != null)
        {
            // Inverte o estado de ativação do GameObject
            objectToToggle.SetActive(!objectToToggle.activeSelf);
        }
        else
        {
            Debug.LogError("Nenhum GameObject foi atribuído à variável 'TUTORIAL' no Inspector.");
        }
    }
}

