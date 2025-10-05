using UnityEngine;
using System;
using Ink.Runtime;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{
    private Story _currentStory;
    private bool _dialogueIsPlaying;
    private InkDialogueVariables _inkDialogueVariables;

    public static DialogueManager instance { get; private set; }

    public event Action<string, string, List<Choice>> onDisplayDialogue;
    public event Action onDialogueStarted;
    public event Action onDialogueFinished;

    private void Awake()
    {
        if (instance != null) { Debug.LogError("Mais de um DialogueManager na scene!"); }
        instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("DialogueManager.OnEnable EXECUTADO.");
        GameEventsManager.instance.DialogueEvents.onEnterDialogue += EnterDialogue;

        if (FaseManager.instance != null)
        {
            // Se inscreve nos eventos do FaseManager
            FaseManager.instance.OnFaseIniciada += HandleFaseIniciada; // <-- INSCRIÇÃO NO NOVO EVENTO
            FaseManager.instance.OnFaseStateChanged += HandleFaseStateChanged;
            FaseManager.instance.OnObservacaoRegistrada += HandleObservacaoRegistrada;
        }
    }

    private void OnDisable()
    {
        GameEventsManager.instance.DialogueEvents.onEnterDialogue -= EnterDialogue;
        if (FaseManager.instance != null)
        {
            // Cancela a inscrição de TODOS os eventos
            FaseManager.instance.OnFaseIniciada -= HandleFaseIniciada;
            FaseManager.instance.OnFaseStateChanged -= HandleFaseStateChanged;
            FaseManager.instance.OnObservacaoRegistrada -= HandleObservacaoRegistrada;
        }
    }

    // NOVO MÉTODO: Chamado apenas UMA VEZ no início da fase
    private void HandleFaseIniciada(FaseData dataDaFase)
    {
        // Cria uma história temporária apenas para inicializar o dicionário de variáveis
        Story tempStory = new Story(dataDaFase.historiaInk.text);
        _inkDialogueVariables = new InkDialogueVariables(tempStory);
        Debug.Log("Gerenciador de variáveis do diálogo INICIALIZADO para a nova fase.");
    }

    public void EnterDialogue(string knotName)
    {
        Debug.Log($"EnterDialogue chamado com o nó: {knotName}");
        if (_dialogueIsPlaying) return;

        // Verificação de segurança
        if (_inkDialogueVariables == null)
        {
            Debug.LogError("O diálogo foi iniciado, mas o Gerenciador de Variáveis é NULO! A fase começou corretamente via GameController?");
            return;
        }

        TextAsset historiaDaFase = FaseManager.instance.faseAtual.historiaInk;
        Debug.Log($"História da fase '{historiaDaFase.name}' carregada com sucesso!");

        _dialogueIsPlaying = true;
        _currentStory = new Story(historiaDaFase.text);

        // AGORA A LÓGICA ESTÁ CORRETA: Sincroniza o estado atual do jogo (que está no _inkDialogueVariables) para dentro da nova história
        _inkDialogueVariables.SyncVariablesToStory(_currentStory);

        if (!string.IsNullOrEmpty(knotName))
        {
            _currentStory.ChoosePathString(knotName);
        }

        onDialogueStarted?.Invoke();
        ContinueStory();
    }

    // O resto do seu script (ExitDialogue, ContinueStory, etc.) permanece o mesmo...

    private void HandleFaseStateChanged(FaseState novoEstado)
    {
        if (_inkDialogueVariables == null) return; // Segurança extra
        string nomeVariavelInk = "fase_status";
        _inkDialogueVariables.UpdateVariableState(nomeVariavelInk, new StringValue(novoEstado.ToString()));
    }

    private void HandleObservacaoRegistrada(string nomeObservacao, bool valor)
    {
        Debug.Log($"<color=green>DialogueManager RECEBEU o evento para: {nomeObservacao}</color>");
        if (_inkDialogueVariables == null) return; // Segurança extra
        string nomeVariavelInk = $"encontrou_{nomeObservacao.ToLower()}";
        _inkDialogueVariables.UpdateVariableState(nomeVariavelInk, new BoolValue(valor));
    }

    // ... Continue com o resto dos seus métodos (MakeChoice, ContinueDialogue, IsLineBlank)
    private void ExitDialogue()
    {
        _dialogueIsPlaying = false;
        onDialogueFinished?.Invoke();
        _currentStory = null;

        if (FaseManager.instance.estadoAtual == FaseState.Inicio)
        {
            Debug.Log("<color=cyan>Diálogo de início concluído! Mudando estado da fase para Andamento.</color>");
            FaseManager.instance.MudarEstado(FaseState.Andamento);
        }
    }

    private void ContinueStory()
    {
        if (_currentStory.canContinue)
        {
            string raw = _currentStory.Continue();                 // 1) pega a linha
            string text = CleanInkLine(raw);                       // 2) LIMPA AQUI ✅

            string speakerName = ParseSpeakerTag(_currentStory.currentTags);

            if (IsLineBlank(text) && !_currentStory.canContinue && _currentStory.currentChoices.Count == 0)
            {
                ExitDialogue();
            }
            else
            {
                onDisplayDialogue?.Invoke(speakerName, text, _currentStory.currentChoices); // 3) envia já limpo ✅
            }
        }
        else if (_currentStory.currentChoices.Count > 0)
        {
            Debug.Log("Esperando escolha...");
        }
        else
        {
            ExitDialogue();
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        if (!_dialogueIsPlaying) return;
        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }

    public void ContinueDialogue()
    {
        if (_dialogueIsPlaying && _currentStory != null && _currentStory.currentChoices.Count == 0)
        {
            ContinueStory();
        }
    }

    private bool IsLineBlank(string line)
    {
        line = CleanInkLine(line);
        return string.IsNullOrWhiteSpace(line);
    }

    private static string CleanInkLine(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Replace("\r\n", "\n");                // normaliza
        s = Regex.Replace(s, @"\n{3,}", "\n\n");    // 3+ quebras => 2
        return s.TrimEnd('\n', '\r', ' ');           // tira quebra no fim
    }

    private static string ParseSpeakerTag(List<string> tags)
    {
        if (tags == null) return "";
        foreach (var t in tags)
        {
            var tag = t.Trim();
            if (tag.StartsWith("speaker", StringComparison.OrdinalIgnoreCase))
            {
                var idx = tag.IndexOf(':');
                if (idx >= 0) return tag.Substring(idx + 1).Trim();
            }
        }
        return "";
    }
}