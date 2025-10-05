using UnityEngine;
using TMPro;

public class DialogueScreen : MonoBehaviour
{
    [TextArea(4, 12)] public string texto;
    public GameObject rootUI;
    public TypewriterTMP typewriter;
    public TMP_Text hintPressSpace;
    public KeyCode advanceKey = KeyCode.Space;

    private bool readyToClose;
    private int _id;

    void Awake()
    {
        _id = GetInstanceID();
        Debug.Log($"[DialogueScreen#{_id}] Awake");
        if (rootUI != null) rootUI.SetActive(false);
        if (hintPressSpace) hintPressSpace.gameObject.SetActive(false);
    }

    public void Show()
    {
        Debug.Log($"[DialogueScreen#{_id}] Show()");
        if (rootUI == null) { Debug.LogError($"[DialogueScreen#{_id}] rootUI NULL"); return; }
        if (typewriter == null) { Debug.LogError($"[DialogueScreen#{_id}] typewriter NULL"); return; }

        rootUI.SetActive(true);
        Debug.Log($"[DialogueScreen#{_id}] rootUI ativo? {rootUI.activeInHierarchy}");

        readyToClose = false;
        if (hintPressSpace) hintPressSpace.gameObject.SetActive(false);

        string finalText = string.IsNullOrEmpty(texto) ? typewriter.name + " (texto do TMP)" : texto;
        Debug.Log($"[DialogueScreen#{_id}] Enviando texto len={finalText.Length} -> \"{finalText}\"");

        // passa o texto, mas se estiver vazio usa null pra não zerar
        typewriter.Play(string.IsNullOrEmpty(texto) ? null : texto);
    }

    void Update()
    {
        if (rootUI == null || !rootUI.activeSelf || typewriter == null) return;

        if (typewriter.Finished && !readyToClose)
        {
            readyToClose = true;
            Debug.Log($"[DialogueScreen#{_id}] Typewriter terminou");
            if (hintPressSpace) hintPressSpace.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(advanceKey))
        {
            Debug.Log($"[DialogueScreen#{_id}] advanceKey; IsPrinting={typewriter.IsPrinting} Finished={typewriter.Finished}");
            if (typewriter.IsPrinting) typewriter.SkipToEnd();
            else if (readyToClose) CloseAndContinue();
        }
    }

    private void CloseAndContinue()
    {
        Debug.Log($"[DialogueScreen#{_id}] CloseAndContinue()");
        rootUI.SetActive(false);
        // próxima ação…
    }
}
