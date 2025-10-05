using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Configuração da Fase")]
    [Tooltip("Arraste aqui o ScriptableObject 'FaseData' correspondente a esta cena.")]
    [SerializeField] private FaseData faseDataDaCena;

    void Start()
    {
        // Validação para garantir que você não esqueceu de atribuir os dados da fase
        if (faseDataDaCena == null)
        {
            Debug.LogError("Erro: Nenhum 'FaseData' foi atribuído ao GameController nesta cena!");
            return;
        }

        // Esta é a linha chave!
        // Diz ao FaseManager para carregar todas as informações da fase que definimos.
        FaseManager.instance.IniciarFase(faseDataDaCena);
        Debug.Log($"GAME CONTROLLER INICIALIZOU A FASE: {faseDataDaCena.nomeDaFase}");
    }
}