using UnityEngine;
using UnityEngine.SceneManagement;
public class CenasG : MonoBehaviour
{
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    // Função opcional para carregar uma cena pelo seu índice de build
    public void CarregarCenaPorIndice(int indiceDaCena)
    {
        SceneManager.LoadScene(indiceDaCena);
    }

    // Função opcional para fechar o jogo
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo..."); // Esta mensagem aparecerá no console da Unity
        Application.Quit();
    }
}
