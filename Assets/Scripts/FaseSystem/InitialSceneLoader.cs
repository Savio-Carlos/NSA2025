using UnityEngine;
using UnityEngine.SceneManagement;

// Este script inicializa o jogo e carrega a primeira fase automaticamente.
public class InitialSceneLoader : MonoBehaviour
{
    [SerializeField] private float delayBeforeLoading = 3f; // Tempo para ver sua cena FilterShow
    [SerializeField] private string firstLevelSceneName = "Norte Nordeste"; // O nome da sua primeira fase

    void Start()
    {
        Invoke("LoadFirstLevel", delayBeforeLoading);
    }

    void LoadFirstLevel()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }
}