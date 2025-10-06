using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public AudioSource source;  // assign in Inspector (same GameObject)
    public AudioClip initialTrack;
    [Range(0f, 1f)] public float initialVolume = 0.8f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);              // prevent duplicates on new scenes
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);        // persist across scenes

        if (source == null) source = GetComponent<AudioSource>();
        source.loop = true;
        source.volume = initialVolume;

        if (initialTrack != null && !source.isPlaying)
        {
            source.clip = initialTrack;
            source.Play();
        }
    }

    public void SetVolume(float v) => source.volume = Mathf.Clamp01(v);
    public void Pause() => source.Pause();
    public void Resume() => source.UnPause();

    public void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        source.clip = clip;
        source.Play();
    }
}