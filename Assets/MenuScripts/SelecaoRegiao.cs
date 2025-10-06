using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelecaoRegiao : MonoBehaviour
{
    // Variáveis de Escala e Destaque
    private Vector3 escalaOriginal; 
    public float fatorEscala = 1.05f; // Aumento de 5% no eixo Z

    // NOVO: Fator que define a intensidade do clareamento (0.0 a 1.0)
    public float fatorClareamento = 0.3f; 
    
    // Configure no Inspector: Arraste os componentes de UI aqui
    public TextMeshProUGUI nomeRegiaoPainel;
    public Button botaoPronto;
    public string regiaoPadrao = "Norte Nordeste";

    // REMOVIDA A VARIÁVEL 'corDestaque'
    
    private string regiaoSelecionada;
    private Renderer ultimaRegiaoSelecionada; 
    
    // Variável para guardar a cor original da última região (necessário para o reset)
    private Color corOriginalUltimaRegiao; 

    void Start()
    {
        // 1. Inicializa com a fase padrão (Norte)
        regiaoSelecionada = regiaoPadrao;
        nomeRegiaoPainel.text = regiaoPadrao; 

        // 2. Configura a ação de carregar a cena para o botão Pronto
        botaoPronto.onClick.AddListener(CarregarFase);
        
        // Aplica o destaque inicial na primeira região (Norte)
        GameObject objInicial = GameObject.Find("Norte Nordeste"); 
        if (objInicial != null)
        {
            ultimaRegiaoSelecionada = objInicial.GetComponent<Renderer>();
            if (ultimaRegiaoSelecionada != null)
            {
                // Salva a escala e a cor originais do objeto inicial
                escalaOriginal = objInicial.transform.localScale;
                corOriginalUltimaRegiao = ultimaRegiaoSelecionada.material.color; 
                
                // --- DESTAQUE DE CLAREAMENTO NO START ---
                // Mistura a cor original com o branco
                Color corClareadaInicial = Color.Lerp(corOriginalUltimaRegiao, Color.white, fatorClareamento);
                
                ultimaRegiaoSelecionada.material.color = corClareadaInicial;
                
                // Aplica o destaque de escala NO EIXO Z
                Vector3 novaEscalaZ = escalaOriginal;
                novaEscalaZ.z *= fatorEscala;
                ultimaRegiaoSelecionada.transform.localScale = novaEscalaZ;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)){
                string nomeObjeto = hit.transform.name;
                Renderer novoRenderer = hit.transform.GetComponent<Renderer>();

                // Verifica se o objeto clicado é um dos nomes de fase válidos
                if (nomeObjeto.Equals("Norte Nordeste") ||
                    nomeObjeto.Equals("Centro Oeste") ||
                    nomeObjeto.Equals("Sul"))
                {
                    if (novoRenderer != null)
                    {
                        // --- 1. LIMPEZA DA REGIÃO ANTERIOR ---
                        
                        // Reseta a cor e a escala da ÚLTIMA região que foi clicada
                        if (ultimaRegiaoSelecionada != null)
                        {
                            ultimaRegiaoSelecionada.material.color = corOriginalUltimaRegiao; // Restaura a cor base
                            ultimaRegiaoSelecionada.transform.localScale = escalaOriginal; // Restaura a escala base salva
                        }
                        
                        // --- 2. ATUALIZAÇÃO DA NOVA SELEÇÃO E DESTAQUE ---
                        
                        // 2a. CAPTURA A COR E ESCALA ORIGINAIS DO NOVO OBJETO
                        corOriginalUltimaRegiao = novoRenderer.material.color;
                        escalaOriginal = hit.transform.localScale; // Salva a escala base antes de aplicar o aumento

                        // 2b. Atualiza a UI
                        regiaoSelecionada = nomeObjeto;
                        nomeRegiaoPainel.text = regiaoSelecionada;
                        
                        // 2c. APLICA O DESTAQUE: Clareamento
                        // Cria a nova cor misturando a cor original com branco, usando o fator
                        Color corClareada = Color.Lerp(corOriginalUltimaRegiao, Color.white, fatorClareamento);
                        novoRenderer.material.color = corClareada;
                        
                        // 2d. APLICA O DESTAQUE: Aumento de Escala SOMENTE NO EIXO Z
                        Vector3 novaEscalaZ = escalaOriginal;
                        novaEscalaZ.z *= fatorEscala;
                        novoRenderer.transform.localScale = novaEscalaZ; 
                        
                        // 2e. ATUALIZA: Guarda a referência do novo objeto
                        ultimaRegiaoSelecionada = novoRenderer;
                    }
                }
            }
        }
    }

    // Função para carregar a cena/fase
    void CarregarFase()
    {
        SceneManager.LoadScene(regiaoSelecionada);
    }
}