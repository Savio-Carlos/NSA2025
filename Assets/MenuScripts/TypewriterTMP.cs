using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterTMP : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private TMP_Text textUI;

    [TextArea(3,10)]
    [SerializeField] private string fullText; // se vazio, usa o texto já no TMP

    [Header("Comportamento")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float punctuationPause = 0.12f;
    [SerializeField] private string punctuation = ".,;:!?";

    [Header("SFX por caractere (opcional)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip[] bleeps;
    [SerializeField] private Vector2 pitchRandom = new Vector2(0.95f, 1.05f);

    [Header("Input")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    public bool IsPrinting { get; private set; }
    public bool Finished { get; private set; }

    private Coroutine routine;
    private int _id;

    void Reset() { textUI = GetComponent<TMP_Text>(); }

    void Awake()
    {
        _id = GetInstanceID();
        Debug.Log($"[TypewriterTMP#{_id}] Awake");

        if (textUI == null) textUI = GetComponent<TMP_Text>();
        if (textUI == null)
        {
            Debug.LogError($"[TypewriterTMP#{_id}] Sem TMP_Text! Arraste um TMP_Text.");
            enabled = false; return;
        }

        if (string.IsNullOrEmpty(fullText)) fullText = textUI.text;

        textUI.text = fullText;
        textUI.maxVisibleCharacters = 0;
        textUI.richText = true; // garante RichText ON por padrão

        // força atualização completa do layout/mesh
        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate(true, true);

        Debug.Log($"[TypewriterTMP#{_id}] Setup: playOnAwake={playOnAwake}, cps={charsPerSecond}, textLen={fullText?.Length ?? 0}, TMPactive={textUI.isActiveAndEnabled}, richText={textUI.richText}");

        if (playOnAwake) Play();
    }

    public void Play(string newText = null)
    {
        Debug.Log($"[TypewriterTMP#{_id}] Play(newText? {newText != null})");
        if (newText != null) fullText = newText;

        StopTyping();

        textUI.text = fullText;
        textUI.maxVisibleCharacters = 0;

        // LOG do texto bruto
        Debug.Log($"[TypewriterTMP#{_id}] TEXT=\"{textUI.text}\" activeInHierarchy={textUI.gameObject.activeInHierarchy}");

        // força atualização antes de medir
        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate(true, true);

        int totalChars = textUI.textInfo.characterCount;
        Debug.Log($"[TypewriterTMP#{_id}] charsAfterForce={totalChars} (spaces={textUI.textInfo.spaceCount})");

        if (totalChars <= 0)
        {
            Debug.LogWarning($"[TypewriterTMP#{_id}] 0 chars visíveis. Texto pode ser só tags/whitespace ou TMP não atualizou?");
            Finished = true; IsPrinting = false; return;
        }

        Finished = false;
        routine = StartCoroutine(TypeRoutine());
    }

    public void SkipToEnd()
    {
        if (!IsPrinting) return;
        Debug.Log($"[TypewriterTMP#{_id}] SkipToEnd");
        StopTyping();
        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate(true, true);
        textUI.maxVisibleCharacters = textUI.textInfo.characterCount;
        Finished = true;
    }

    private void StopTyping()
    {
        if (routine != null)
        {
            Debug.Log($"[TypewriterTMP#{_id}] StopTyping");
            StopCoroutine(routine);
        }
        routine = null;
        IsPrinting = false;
    }

    private IEnumerator TypeRoutine()
    {
        IsPrinting = true;

        Canvas.ForceUpdateCanvases();
        textUI.ForceMeshUpdate(true, true);
        int totalChars = textUI.textInfo.characterCount;
        int visible = 0;

        float baseDelay = (charsPerSecond <= 0f) ? 0f : 1f / charsPerSecond;
        Debug.Log($"[TypewriterTMP#{_id}] TypeRoutine START total={totalChars} baseDelay={baseDelay}");

        while (visible < totalChars)
        {
            if (Input.GetKeyDown(skipKey))
            {
                Debug.Log($"[TypewriterTMP#{_id}] skipKey");
                SkipToEnd(); yield break;
            }

            visible++;
            textUI.maxVisibleCharacters = visible;
            PlayBleepForIndex(visible - 1);

            char c = GetRawCharAtVisibleIndex(visible - 1);
            float delay = baseDelay + (punctuation.IndexOf(c) >= 0 ? punctuationPause : 0f);

            // usar tempo não escalado
            float t = 0f;
            while (t < delay)
            {
                if (Input.GetKeyDown(skipKey)) { SkipToEnd(); yield break; }
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            if (visible % 10 == 0 || visible == totalChars)
                Debug.Log($"[TypewriterTMP#{_id}] progress {visible}/{totalChars}");
        }

        IsPrinting = false;
        Finished = true;
        Debug.Log($"[TypewriterTMP#{_id}] TypeRoutine END");
    }

    private char GetRawCharAtVisibleIndex(int i)
    {
        if (i >= 0 && i < textUI.textInfo.characterCount)
            return textUI.textInfo.characterInfo[i].character;
        return '\0';
    }

    private void PlayBleepForIndex(int i)
    {
        if (sfxSource == null || bleeps == null || bleeps.Length == 0) return;
        sfxSource.pitch = Random.Range(pitchRandom.x, pitchRandom.y);
        var clip = bleeps[i % bleeps.Length];
        sfxSource.PlayOneShot(clip);
    }
}
