using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public DialogueEvents DialogueEvents;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Mais de um game GameEvents na scene");
        }
        instance = this;

        DialogueEvents = new DialogueEvents();
    }
}
