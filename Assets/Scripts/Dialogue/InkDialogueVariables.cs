using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkDialogueVariables
{
    private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

    public InkDialogueVariables(Story story)
    {
        foreach (string name in story.variablesState)
        {
            var raw = GetRawValue(story, name);
            _variables[name] = raw;
            Debug.Log($"Variável Ink inicializada: {name} = {raw}");
        }
    }

    public void UpdateVariableState(string name, Value value)
    {
        if (string.IsNullOrEmpty(name) || value == null) return;
        var raw = value.valueObject;
        if (_variables.ContainsKey(name))
        {
            _variables[name] = raw;
        }
        else
        {
            _variables.Add(name, raw);
            Debug.LogWarning($"Variável Ink '{name}' não existia no snapshot inicial. Adicionando dinamicamente.");
        }
    }

    public void SyncVariablesToStory(Story story)
    {
        Debug.Log("<color=lightblue>--- SINCRONIZANDO VARIÁVEIS DO C# PARA O INK ---</color>");

        foreach (KeyValuePair<string, object> variable in _variables)
        {
            // Este é o log mais importante de todos.
            Debug.Log($"Sincronizando: {variable.Key} = {variable.Value} (Tipo: {variable.Value.GetType()})");
            story.variablesState[variable.Key] = variable.Value;
        }
        Debug.Log("<color=lightblue>--- SINCRONIZAÇÃO CONCLUÍDA ---</color>");
    }

    private static object GetRawValue(Story story, string name)
    {
        var obj = story.variablesState.GetVariableWithName(name);
        if (obj is Value v)
        {
            return v.valueObject;
        }
        return obj;
    }
}