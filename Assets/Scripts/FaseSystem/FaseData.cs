using UnityEngine;
using System.Collections.Generic;

// O atributo [CreateAssetMenu] nos permite criar "arquivos de dados" de fase dentro do Editor da Unity
[CreateAssetMenu(fileName = "Nova Fase", menuName = "Jogo/Nova Fase")]
public class FaseData : ScriptableObject
{
    [Header("Informações da Fase")]
    public string nomeDaFase;

    [Tooltip("O arquivo .ink compilado (.json) para esta fase específica.")]
    public TextAsset historiaInk;

    [Header("Observações")]
    [Tooltip("Liste os nomes das variáveis para cada observação nesta fase. Ex: 'agua', 'nitrogenio', 'praga'")]
    public List<string> nomesDasObservacoes;
}