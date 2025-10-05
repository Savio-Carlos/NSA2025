using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogueEvents
{
    public event Action<string> onEnterDialogue;

    public void enterDialogue(string knotName)
    {
        Debug.Log($"EVENTO SENDO DISPARADO com o nó: {knotName}"); // Log #1

        if (onEnterDialogue != null)
        {
            onEnterDialogue(knotName);
        }
        else
        {
            Debug.LogWarning("AVISO: Ninguém está inscrito (ouvindo) o evento onEnterDialogue!"); // Log #2
        }
    }
}
