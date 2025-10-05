using UnityEngine;

public class VereditoUI : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private GameObject botaoVeredito;
    [SerializeField] private GameObject painelVeredito;

    void Start()
    {
        // Ao iniciar, encontra o "cérebro" e se registra
        if (VereditoPanelController.instance != null)
        {
            VereditoPanelController.instance.RegisterUI(this);
        }
        else
        {
            Debug.LogError("VereditoUI não conseguiu encontrar o VereditoPanelController.instance!");
        }

        // Garante que tudo comece escondido
        // botaoVeredito.SetActive(false);
        painelVeredito.SetActive(false);    
    }

    // Métodos para o "cérebro" controlar a UI
    public void SetVisibilityBotaoVeredito(bool isVisible)
    {
        Debug.Log($"--- Passo 5: VereditoUI.SetVisibilityBotaoVeredito recebido. Comando é para deixar visível: {isVisible}. ---");

        if (botaoVeredito == null)
        {
            Debug.LogError("FALHA CRÍTICA: O comando para mostrar o botão foi recebido, mas a variável 'botaoVeredito' está NULA neste ponto! Verifique novamente a referência no Inspector do Prefab.");
            return;
        }

        botaoVeredito.SetActive(isVisible);
        Debug.Log($"--- Passo 6: O botão '{botaoVeredito.name}' teve seu SetActive definido para {isVisible}. O botão deveria estar visível agora. ---");
    }
    public void AbrirPainel() => painelVeredito.SetActive(true);
    public void FecharPainel() => painelVeredito.SetActive(false);

    // Métodos para os botões chamarem
    public void OnBotaoVereditoClicked() => VereditoPanelController.instance.AbrirPainelVeredito();
    public void OnEscolhaClicked(int index) => VereditoPanelController.instance.FazerEscolha(index);
}