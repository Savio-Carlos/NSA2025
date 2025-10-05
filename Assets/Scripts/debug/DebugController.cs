using UnityEngine;

// Este script é apenas para fins de teste e pode ser removido ou desativado na versão final do jogo.
public class DebugController : MonoBehaviour
{
    // Este método será chamado pelo botão de teste da vegetação.
    public void RegistrarObservacaoVegetacao()
    {
        if (FaseManager.instance != null)
        {
            Debug.Log("--- DEBUG: Registrando observação 'vegetacao' ---");
            FaseManager.instance.RegistrarObservacao("vegetacao");
        }
    }

    // Este método será chamado pelo botão de teste do fogo.
    public void RegistrarObservacaoFogo()
    {
        if (FaseManager.instance != null)
        {
            Debug.Log("--- DEBUG: Registrando observação 'fogo' ---");
            FaseManager.instance.RegistrarObservacao("fogo");
        }
    }

    // BÔNUS: Um botão para forçar a mudança de estado após o diálogo inicial
    public void MudarEstadoParaAndamento()
    {
        if (FaseManager.instance != null)
        {
            Debug.Log("--- DEBUG: Mudando estado para 'Andamento' ---");
            // Use o nome exato do seu enum FaseState
            FaseManager.instance.MudarEstado(FaseState.Andamento);
        }
    }
}