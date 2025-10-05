using UnityEngine;

public class BotaoObservacao : MonoBehaviour
{
    [Header("Configuração da Observação")]
    [Tooltip("Digite o nome EXATO da observação que este botão deve registrar (ex: 'fogo', 'vegetacao').")]
    [SerializeField] private string nomeDaObservacao;

    public void RegistrarObservacao()
    {
        if (string.IsNullOrEmpty(nomeDaObservacao))
        {
            Debug.LogError("O 'Nome da Observação' não foi definido no Inspector para o botão: " + gameObject.name);
            return;
        }

        if (FaseManager.instance != null)
        {
            if (FaseManager.instance.ChecarObservacao(nomeDaObservacao))
            {
                Debug.Log($"O jogador clicou no botão '{gameObject.name}', mas a observação '{nomeDaObservacao}' já foi registrada. Nenhuma ação necessária.");
                return; 
            }
            Debug.Log($"Botão '{gameObject.name}' clicado. Registrando observação pela primeira vez: '{nomeDaObservacao}'");
            FaseManager.instance.RegistrarObservacao(nomeDaObservacao);
        }
        else
        {
            Debug.LogError("FaseManager.instance não foi encontrado! O gerenciador está na cena inicial?");
        }
    }
}