using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntVar
{
    [SerializeField] protected string key;
    [SerializeField] protected int value;

    public string Key { get { return key; } set { key = value; } }
    public int Value { get { return value; } set { this.value = value; } }
}

/// Serializable container for encoding the state of variables.
[System.Serializable]
public class EngineData 
{
    [SerializeField] protected string engineName;
    [SerializeField] protected List<IntVar> intVars = new List<IntVar>();

    public string EngineName { get { return engineName; } set { engineName = value; } }

    public List<IntVar> IntVars { get { return intVars; } set { intVars = value; } }

    public static EngineData Encode(BasicFlowEngine engine)
    {
        var engineData = new EngineData();
        engineData.EngineName = engine.name;

        for(int i = 0; i <engine.Variables.Count; i++)
        {
            var v = engine.Variables[i];

            var intVariable = v as IntegerVariable;
            if (intVariable != null)
            {
                var d = new IntVar();
                d.Key = intVariable.Key;
                d.Value = intVariable.Value;
                engineData.IntVars.Add(d);
            }
        }

        return engineData;
    }

    public static void Decode(EngineData engineData)
    {

        var go = GameObject.Find(engineData.EngineName);
        if (go == null)
        {
            Debug.LogError("Failed to find engine with name: " + engineData.EngineName);
            return;
        }

        var engine = go.GetComponent<BasicFlowEngine>();
        if (engine == null)
        {
            Debug.LogError("Failed to find engine component in game object: " + engineData.EngineName);
            return;
        }

        for (int i = 0; i < engineData.IntVars.Count; i++)
        {
            var intVar = engineData.IntVars[i];
            engine.SetIntegerVariable(intVar.Key, intVar.Value);
        }
    }
}
