using UnityEngine;
using UnityEngine.SceneManagement;

public class VereditoPanelController : MonoBehaviour
{
    public static VereditoPanelController instance { get; private set; }

    // Referência para a UI, que será registrada dinamicamente
    private VereditoUI _ui;

    // Lógica interna do veredito
    private bool esperandoFimDialogoVeredito = false;
    [SerializeField] private string nomeDoKnotCerto = "veredito_certo";
    [SerializeField] private string nomeDoKnotErrado = "veredito_errado";
    [SerializeField] private int indiceRespostaCerta = 0;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.onDialogueFinished += HandleDialogueFinished;
        if (FaseManager.instance != null)
            FaseManager.instance.OnFaseStateChanged += HandleFaseStateChanged;
        if (DialogueManager.instance != null)
        DialogueManager.instance.onDialogueStarted += HandleDialogueStarted;
    }

    private void OnDisable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.onDialogueFinished -= HandleDialogueFinished;
        if (FaseManager.instance != null)
            FaseManager.instance.OnFaseStateChanged -= HandleFaseStateChanged;
        if (DialogueManager.instance != null)
        DialogueManager.instance.onDialogueStarted -= HandleDialogueStarted;
    }

    // A UI vai chamar este método para se "registrar"
    public void RegisterUI(VereditoUI uiInstance)
    {
        Debug.Log("--- Passo 1: VereditoPanelController.RegisterUI foi chamado. ---");
        _ui = uiInstance;
        if (_ui == null)
        {
            Debug.LogError("ERRO: A instância da UI ficou nula imediatamente após a atribuição!");
            return;
        }
        Debug.Log("--- Passo 2: A referência da UI foi guardada. Chamando HandleFaseStateChanged... ---");
        HandleFaseStateChanged(FaseManager.instance.estadoAtual);
    }

    private void HandleFaseStateChanged(FaseState novoEstado)
    {
        if (_ui != null)
        {
            Debug.Log("--- Passo 3: HandleFaseStateChanged executado. Regra é 'deveMostrarBotao = true'. ---");
            bool deveMostrarBotao = true;
            Debug.Log("--- Passo 4: Enviando comando para a UI... ---");
            _ui.SetVisibilityBotaoVeredito(deveMostrarBotao);
        }
        else
        {
            Debug.LogError("ERRO: HandleFaseStateChanged foi chamado, mas a referência _ui é NULA!");
        }
    }

    // Funções chamadas pela UI
    public void AbrirPainelVeredito() => _ui?.AbrirPainel();
    public void FecharPainelVeredito() => _ui?.FecharPainel();

    public void FazerEscolha(int indiceDaEscolha)
    {
        _ui?.FecharPainel();
        esperandoFimDialogoVeredito = true;

        if (indiceDaEscolha == indiceRespostaCerta)
        {
            string nomeFaseAtual = FaseManager.instance.faseAtual.nomeDaFase;
            GameProgressManager.instance.MarcarFaseComoConcluida(nomeFaseAtual);
            GameEventsManager.instance.DialogueEvents.enterDialogue(nomeDoKnotCerto);
        }
        else
        {
            GameEventsManager.instance.DialogueEvents.enterDialogue(nomeDoKnotErrado);
        }
    }
    private void HandleDialogueStarted()
    {
        // Quando um diálogo começa, simplesmente dizemos para a UI esconder o botão.
        if (_ui != null)
        {
            Debug.Log("Diálogo iniciado. Escondendo o botão de Veredito.");
            _ui.SetVisibilityBotaoVeredito(false);
        }
    }

    private void HandleDialogueFinished()
    {
        // Primeiro, verificamos se este é o diálogo de veredito que acabou de terminar.
        if (esperandoFimDialogoVeredito)
        {
            esperandoFimDialogoVeredito = false;
            Debug.Log("Veredito concluído. Retornando ao seletor de níveis...");

            // ATUALIZE ESTA LINHA
            SceneManager.LoadScene("LevelSelect"); 
        }

        // Se a condição acima for falsa, significa que era um diálogo normal da fase.
        // Neste caso, nós reavaliamos se o botão de veredito deve reaparecer.
        if (_ui != null)
        {
            Debug.Log("Diálogo de fase finalizado. Reavaliando a visibilidade do botão de Veredito.");
            HandleFaseStateChanged(FaseManager.instance.estadoAtual);
        }
    }
}