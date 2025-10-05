using UnityEngine;
using System.Collections.Generic; // Necess�rio para usar List

public class BookManager : MonoBehaviour
{
    // Arraste todos os seus GameObjects de p�gina aqui pelo Inspector
    public List<GameObject> pages;

    void Start()
    {

        // Garante que apenas a primeira p�gina (�ndice) esteja vis�vel ao iniciar
        ShowPage(0);
    }

    // Uma fun��o p�blica que pode ser chamada pelos bot�es
    public void ShowPage(int pageIndex)
    {
        // Primeiro, esconde todas as p�ginas
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(false);
        }

        // Depois, mostra apenas a p�gina desejada, se o �ndice for v�lido
        if (pageIndex >= 0 && pageIndex < pages.Count)
        {
            pages[pageIndex].SetActive(true);
        }
    }
}