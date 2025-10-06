using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelecaoRegiaoRaycast : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI nomeRegiaoPainel;
    public Button botaoPronto;
    public string regiaoPadrao = "Norte Nordeste";

    [Header("Destaque")]
    public float fatorEscala = 1.05f;
    public float fatorClareamento = 0.3f;

    [Header("Raycast / Layers")]
    public LayerMask regiaoLayer;
    public Transform regioesParent;

    [Header("Distância Máxima Clique")]
    [Tooltip("Distância máxima entre o ponto de impacto do raycast e o centro do objeto filho para considerar o clique válido.")]
    public float distanciaMaximaClique = 1.0f;

    [Header("Nomes válidos (opcional)")]
    public string[] nomesRegioes = new string[] { "Norte Nordeste", "Centro Oeste", "Sul" };

    [Header("Mapa de nomes para exibição")]
    [Tooltip("Mapeie o nome do objeto/scene (chave) para o nome exibido (exibição).")]
    public NomeExibicao[] mapaNomes;

    private Dictionary<string, string> nomeParaExibicao; // chave normalizada -> exibição

    private Renderer ultimaRegiaoSelecionada;
    private Color corOriginalUltimaRegiao;
    private Vector3 escalaOriginalUltimaRegiao;

    [Serializable]
    public struct NomeExibicao
    {
        public string chave;     // nome do objeto/scene
        public string exibicao;  // nome que aparece no painel
    }

    void Awake()
    {
        // Monta dicionário de exibição (normaliza chave)
        nomeParaExibicao = new Dictionary<string, string>();
        if (mapaNomes != null)
        {
            foreach (var par in mapaNomes)
            {
                if (!string.IsNullOrWhiteSpace(par.chave))
                {
                    string key = NormalizarChave(par.chave);
                    if (!nomeParaExibicao.ContainsKey(key))
                        nomeParaExibicao.Add(key, par.exibicao ?? par.chave);
                    else
                        nomeParaExibicao[key] = par.exibicao ?? par.chave;
                }
            }
        }
    }

    void Start()
    {
        if (regioesParent == null)
        {
            GameObject go = GameObject.Find("Sphere");
            if (go != null) regioesParent = go.transform;
        }

        if (botaoPronto != null)
            botaoPronto.onClick.AddListener(CarregarFase);

        if (regioesParent != null)
        {
            Transform inicial = FindChildByName(regioesParent, regiaoPadrao);
            if (inicial != null)
            {
                Renderer r = inicial.GetComponent<Renderer>();
                if (r != null)
                {
                    ultimaRegiaoSelecionada = r;
                    corOriginalUltimaRegiao = r.material.color;
                    escalaOriginalUltimaRegiao = inicial.localScale;

                    Color clareada = Color.Lerp(corOriginalUltimaRegiao, Color.white, fatorClareamento);
                    r.material.color = clareada;

                    Vector3 s = escalaOriginalUltimaRegiao;
                    s.z *= fatorEscala;
                    inicial.localScale = s;

                    if (nomeRegiaoPainel != null)
                        nomeRegiaoPainel.text = ResolverNomeExibicao(regiaoPadrao);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, regiaoLayer))
            {
                Renderer candidato = GetClosestChildRenderer(regioesParent, hit.point);

                if (candidato == null)
                    return;

                string candidatoName = candidato.gameObject.name;
                if (!NomeEhValida(candidatoName))
                    return;

                if (ultimaRegiaoSelecionada != null)
                {
                    ultimaRegiaoSelecionada.material.color = corOriginalUltimaRegiao;
                    ultimaRegiaoSelecionada.transform.localScale = escalaOriginalUltimaRegiao;
                }

                corOriginalUltimaRegiao = candidato.material.color;
                escalaOriginalUltimaRegiao = candidato.transform.localScale;

                Color clareada = Color.Lerp(corOriginalUltimaRegiao, Color.white, fatorClareamento);
                candidato.material.color = clareada;

                Vector3 novaEscala = escalaOriginalUltimaRegiao;
                novaEscala.z *= fatorEscala;
                candidato.transform.localScale = novaEscala;

                if (nomeRegiaoPainel != null)
                    nomeRegiaoPainel.text = ResolverNomeExibicao(candidatoName);

                ultimaRegiaoSelecionada = candidato;
            }
        }
    }

    // ---------- NOMES DE EXIBIÇÃO ----------

    string ResolverNomeExibicao(string nomeInterno)
    {
        if (string.IsNullOrWhiteSpace(nomeInterno))
            return string.Empty;

        string key = NormalizarChave(nomeInterno);

        // 1) Se estiver no dicionário, usa o "bonito"
        if (nomeParaExibicao != null && nomeParaExibicao.TryGetValue(key, out var exibicao))
            return exibicao;

        // 2) Fallback “bonitinho”: troca _ e - por espaço, remove (Clone) e faz Title Case simples
        string limpinho = Regex.Replace(RemoverClone(nomeInterno), @"[_\-]+", " ").Trim();
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(limpinho.ToLower());
    }

    string NormalizarChave(string s)
    {
        return Regex.Replace(RemoverClone(s).Trim(), @"\s+", " ").ToLower();
    }

    string RemoverClone(string s)
    {
        return Regex.Replace(s ?? "", @"\s*\(Clone\)$", "", RegexOptions.IgnoreCase);
    }

    // ---------- RESTO DO SEU CÓDIGO ----------

    Renderer GetClosestChildRenderer(Transform parent, Vector3 point)
    {
        if (parent == null) return null;

        Renderer[] rends = parent.GetComponentsInChildren<Renderer>(false);
        Renderer best = null;
        float bestDist = float.MaxValue;

        foreach (Renderer r in rends)
        {
            if (r.transform == parent) continue;

            float d = Vector3.Distance(r.transform.position, point);
            if (d < bestDist)
            {
                bestDist = d;
                best = r;
            }
        }

        return (best != null && bestDist <= distanciaMaximaClique) ? best : null;
    }

    bool NomeEhValida(string nome)
    {
        if (nomesRegioes == null || nomesRegioes.Length == 0) return true;
        string nomeC = nome.Trim().ToLower();

        foreach (string alvo in nomesRegioes)
        {
            string alvoC = alvo.Trim().ToLower();
            if (nomeC == alvoC || CompareNamesLoose(nomeC, alvoC))
                return true;
        }
        return false;
    }

    bool CompareNamesLoose(string a, string b)
    {
        string[] tA = Regex.Split(a.ToLower(), @"\W+").Where(s => s.Length > 0).ToArray();
        string[] tB = Regex.Split(b.ToLower(), @"\W+").Where(s => s.Length > 0).ToArray();
        Array.Sort(tA); Array.Sort(tB);
        return tA.SequenceEqual(tB);
    }

    Transform FindChildByName(Transform parent, string nome)
    {
        if (parent == null) return null;
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == nome) return t;
        return null;
    }

    void CarregarFase()
    {
        // Continua carregando a cena pelo NOME INTERNO do objeto selecionado,
        // independente do nome exibido no painel.
        if (ultimaRegiaoSelecionada != null)
            SceneManager.LoadScene(ultimaRegiaoSelecionada.name);
    }
}
