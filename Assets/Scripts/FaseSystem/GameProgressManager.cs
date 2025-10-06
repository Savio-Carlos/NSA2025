using UnityEngine;
using System.Collections.Generic;

// Este script será o guardião do progresso geral do jogador.
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager instance { get; private set; }

    // Usamos um HashSet para armazenar os nomes das fases concluídas.
    // É rápido e evita duplicatas.
    private HashSet<string> fasesConcluidas = new HashSet<string>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Método para marcar uma fase como concluída com sucesso
    public void MarcarFaseComoConcluida(string nomeDaFase)
    {
        if (!fasesConcluidas.Contains(nomeDaFase))
        {
            fasesConcluidas.Add(nomeDaFase);
            Debug.Log($"<color=gold>PROGRESSO GLOBAL SALVO: Fase '{nomeDaFase}' concluída com sucesso!</color>");
            // Aqui é onde você salvaria em um arquivo no futuro (PlayerPrefs, JSON, etc.)
        }
    }

    // Método para checar se uma fase já foi concluída
    public bool ChecarFaseConcluida(string nomeDaFase)
    {
        return fasesConcluidas.Contains(nomeDaFase);
    }
}