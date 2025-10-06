using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Ink.Runtime;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Atributos do dialogo")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _portraitGO; 
    [SerializeField] private GameObject _UIBlocker; 
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _speakerNameText;

    [Header("Escolhas UI")]
    [SerializeField] private GameObject _choicesParent;
    [SerializeField] private DialogueChoiceButton[] _choiceButtons; 

  // make sure this is at the top

private void Update()
{
    // Only react if the dialogue panel is on screen
    if (_dialoguePanel == null || !_dialoguePanel.activeInHierarchy) return;

    if (Input.GetKeyDown(KeyCode.Space))
    {
        // If choices are visible, "click" the selected (or first) choice
        if (_choicesParent != null && _choicesParent.activeInHierarchy)
        {
            GameObject sel = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (sel != null && sel.TryGetComponent<Button>(out var selBtn))
            {
                selBtn.onClick.Invoke();
            }
            else
            {
                // Fallback: click the first active choice button
                foreach (var b in _choiceButtons)
                {
                    if (b != null && b.gameObject.activeInHierarchy &&
                        b.TryGetComponent<Button>(out var btn))
                    {
                        btn.onClick.Invoke();
                        break;
                    }
                }
            }
        }
        // Otherwise, behave like your Continue button
        else if (_continueButton != null &&
                 _continueButton.gameObject.activeInHierarchy &&
                 _continueButton.interactable)
        {
            _continueButton.onClick.Invoke();
        }
    }
}


    private void OnEnable()
    {
        DialogueManager.instance.onDialogueStarted += ShowDialoguePanel;
        DialogueManager.instance.onDialogueFinished += HideDialoguePanel;
        DialogueManager.instance.onDisplayDialogue += DisplayDialogue;

        // Se voc� implementar um sistema de sele��o de escolha
        // GameEvents.instance.DialogueEvents.onUpdateChoiceIndex += SelectChoice; // Evento para saber qual escolha foi selecionada
    }

    private void OnDisable()
    {
        DialogueManager.instance.onDialogueStarted -= ShowDialoguePanel;
        DialogueManager.instance.onDialogueFinished -= HideDialoguePanel;
        DialogueManager.instance.onDisplayDialogue -= DisplayDialogue;
        // GameEvents.instance.DialogueEvents.onUpdateChoiceIndex -= SelectChoice;
    }

    private void Awake()
    {
        _dialoguePanel.SetActive(false); // Esconde o painel no in�cio
        if (_portraitGO) _portraitGO.SetActive(false); 
        if (_UIBlocker) _UIBlocker.SetActive(true);
         _continueButton.onClick.AddListener(DialogueManager.instance.ContinueDialogue);
        _choicesParent.SetActive(false); // Esconde os pais das escolhas
        ResetPanel();
    }

    private void ShowDialoguePanel()
    {
        if (_portraitGO)
        {
            _portraitGO.SetActive(true);               // liga junto
            _portraitGO.transform.SetAsFirstSibling(); // garante que fique atrás da caixa
        }
        if (_UIBlocker)
        {
            _UIBlocker.SetActive(false); // desliga junto
        }
        _dialoguePanel.SetActive(true);
        ResetPanel();
    }

    private void HideDialoguePanel()
    {
        _dialoguePanel.SetActive(false);
        if (_portraitGO) _portraitGO.SetActive(false);
        if (_UIBlocker) _UIBlocker.SetActive(true);
        
        ResetPanel();
    }

    public void DisplayDialogue(string speakerName, string line, List<Choice> currentChoices)
    {
        if (!string.IsNullOrEmpty(speakerName))
        {
            _speakerNameText.gameObject.SetActive(true);
            _speakerNameText.text = speakerName;
        }
        else
        {
            _speakerNameText.gameObject.SetActive(false);
        }

        _dialogueText.text = line; 
        _continueButton.gameObject.SetActive(currentChoices.Count == 0);

        // Limpa e desativa todos os bot�es de escolha
        for (int i = 0; i < _choiceButtons.Length; i++)
        {
            _choiceButtons[i].gameObject.SetActive(false);
        }

        if (currentChoices.Count > 0)
        {
            _choicesParent.SetActive(true);

            // Exibe os bot�es de escolha na ordem inversa do Ink para uma UI "de baixo para cima"
            int choiceButtonIndex = currentChoices.Count - 1;
            for (int i = 0; i < currentChoices.Count; i++)
            {
                DialogueChoiceButton button = _choiceButtons[choiceButtonIndex];
                button.gameObject.SetActive(true);
                button.SetText(currentChoices[i].text);
                button.SetChoiceIndex(currentChoices[i].index);

                if (i == 0) // Seleciona o primeiro bot�o por padr�o
                {
                    button.SelectChoiceButton();
                }
                choiceButtonIndex--;
            }
        }
        else
        {
            _choicesParent.SetActive(false);
        }
    }

    private void ResetPanel()
    {
        _dialogueText.text = "";
        _speakerNameText.text = ""; // Limpa o nome no reset
        _speakerNameText.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);
        _choicesParent.SetActive(false);
    }
}