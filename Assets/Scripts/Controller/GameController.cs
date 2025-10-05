using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Configuração da Fase")]
    [Tooltip("Arraste aqui o ScriptableObject 'FaseData' correspondente a esta cena.")]
    [SerializeField] private FaseData faseDataDaCena;

    void Start()
    {
        // Validação crucial
        if (faseDataDaCena == null)
        {
            Debug.LogError("Erro: Nenhum 'FaseData' foi atribuído ao GameController nesta cena!");
            return;
        }

        // ---- LÓGICA DE CONTROLE ADICIONADA ----
        // Verifica se uma fase já está rodando E se essa fase é a mesma que este GameController gerencia.
        // Se for, significa que estamos apenas voltando para a cena, então não fazemos nada.
        if (FaseManager.instance.faseAtual != null && FaseManager.instance.faseAtual == faseDataDaCena)
        {
            Debug.Log($"Retornando para a cena da fase '{faseDataDaCena.nomeDaFase}'. O estado será preservado. Nenhuma reinicialização é necessária.");
            return; // Sai da função Start()
        }
        // ------------------------------------

        // Se a lógica acima não foi acionada, significa que esta é a primeira vez
        // que estamos carregando esta fase. Então, iniciamos ela.
        Debug.Log($"GAME CONTROLLER INICIALIZANDO A FASE PELA PRIMEIRA VEZ: {faseDataDaCena.nomeDaFase}");
        FaseManager.instance.IniciarFase(faseDataDaCena);
    }
}