using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FaseManager : MonoBehaviour
{
    public static FaseManager instance { get; private set; }

    public FaseData faseAtual { get; private set; }
    public FaseState estadoAtual { get; private set; }

    public event Action<FaseState> OnFaseStateChanged;
    public event Action<string, bool> OnObservacaoRegistrada;
    private Dictionary<string, bool> observacoesConcluidas;

    public event Action<FaseData> OnFaseIniciada;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            Debug.LogError("Mais de um FaseManager na cena!");
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // O estado inicial pode ser definido aqui, mas a fase em si
        // deve ser iniciada por um gerenciador de jogo ao carregar o nível.
        // MudarEstado(FaseState.Inicio);
    }

    public void IniciarFase(FaseData dataDaFase)
    {
        faseAtual = dataDaFase;
        observacoesConcluidas = new Dictionary<string, bool>();

        foreach (string obs in faseAtual.nomesDasObservacoes)
        {
            observacoesConcluidas[obs.ToLower()] = false;
        }
        
        // Dispara o evento para que outros scripts (como o DialogueManager) saibam que a fase começou
        OnFaseIniciada?.Invoke(faseAtual); // <-- ADICIONE ESTA LINHA

        Debug.Log($"Fase '{faseAtual.nomeDaFase}' iniciada com {observacoesConcluidas.Count} observações.");
        MudarEstado(FaseState.Inicio);
    }

    public void RegistrarObservacao(string nomeObservacao)
    {
        // LOG #1: Este log deve aparecer SEMPRE que o botão for clicado.
        Debug.Log($"[FaseManager] Tentativa de registrar observação recebida: '{nomeObservacao}'");

        string obsKey = nomeObservacao.ToLower();

        if (observacoesConcluidas.ContainsKey(obsKey) && !observacoesConcluidas[obsKey])
        {
            // LOG #2: Este log SÓ VAI APARECER se a observação for válida e registrada com sucesso.
            Debug.Log($"[FaseManager] Observação '{obsKey}' é VÁLIDA. Registrando e disparando evento!");

            observacoesConcluidas[obsKey] = true;
            OnObservacaoRegistrada?.Invoke(obsKey, true); // O evento é disparado aqui!


        }
        else
        {
            // LOG #3: Se a observação não for válida (typo?) ou já tiver sido feita, este log aparecerá.
            Debug.LogWarning($"[FaseManager] A observação '{obsKey}' não pôde ser registrada. Ela existe na FaseData? Já foi concluída?");
        }
    }

    public bool TodasObservacoesFeitas()
    {
        if (observacoesConcluidas == null || observacoesConcluidas.Count == 0) return false;
        return observacoesConcluidas.All(kvp => kvp.Value);
    }
    public bool ChecarObservacao(string nomeObservacao)
    {
        string obsKey = nomeObservacao.ToLower();

        if (observacoesConcluidas.ContainsKey(obsKey))
        {
            return observacoesConcluidas[obsKey];
        }

        Debug.LogWarning($"Tentativa de checar uma observação ('{nomeObservacao}') que não existe na lista da FaseData atual.");
        return false;
    }
    public void MudarEstado(FaseState novoEstado)
    {
        if (estadoAtual == novoEstado) return;

        estadoAtual = novoEstado;
        Debug.Log("Novo estado da fase: " + novoEstado);
        OnFaseStateChanged?.Invoke(novoEstado);
    }
}