using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueChoiceButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private TextMeshProUGUI _choiceText;
    private Button _button;
    private int _choiceIndex = -1;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    public void SetText(string text)
    {
        _choiceText.text = text;
    }

    public void SetChoiceIndex(int index)
    {
        _choiceIndex = index;
    }

    public void SelectChoiceButton()
    {
        _button.Select(); // Seleciona o botão
    }

    private void HandleClick()
    {
        if (_choiceIndex != -1)
        {
            DialogueManager.instance.MakeChoice(_choiceIndex); // Informa ao DialogueManager a escolha
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Disparar um evento para o DialogueManager saber qual escolha foi selecionada,
        // caso você tenha lógica de navegação de teclado/gamepad
        // GameEvents.instance.DialogueEvents.UpdateChoiceIndex(_choiceIndex);
    }
}