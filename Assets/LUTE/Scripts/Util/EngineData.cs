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

[System.Serializable]
public class BoolVar
{
    [SerializeField] protected string key;
    [SerializeField] protected bool value;

    public string Key { get { return key; } set { key = value; } }
    public bool Value { get { return value; } set { this.value = value; } }
}

[System.Serializable]
public class FloatVar
{
    [SerializeField] protected string key;
    [SerializeField] protected float value;

    public string Key { get { return key; } set { key = value; } }
    public float Value { get { return value; } set { this.value = value; } }
}

[System.Serializable]
public class StringVar
{
    [SerializeField] protected string key;
    [SerializeField] protected string value;

    public string Key { get { return key; } set { key = value; } }
    public string Value { get { return value; } set { this.value = value; } }
}

/// Serializable container for encoding the state of variables.
[System.Serializable]
public class EngineData 
{
    [SerializeField] protected string engineName;
    [SerializeField] protected List<IntVar> intVars = new List<IntVar>();
    [SerializeField] protected List<BoolVar> boolVars = new List<BoolVar>();
    [SerializeField] protected List<FloatVar> floatVars = new List<FloatVar>();
    [SerializeField] protected List<StringVar> stringVars = new List<StringVar>();

    public string EngineName { get { return engineName; } set { engineName = value; } }

    public List<IntVar> IntVars { get { return intVars; } set { intVars = value; } }
    public List<BoolVar> BoolVars { get { return boolVars; } set { boolVars = value; } }
    public List<FloatVar> FloatVars { get { return floatVars; } set { floatVars = value; } }
    public List<StringVar> StringVars { get { return stringVars; } set { stringVars = value; } }

    public static EngineData Encode(BasicFlowEngine engine)
    {
        var engineData = new EngineData();
        engineData.EngineName = engine.name;

        for(int i = 0; i < engine.Variables.Count; i++)
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

            var boolVariable = v as BooleanVariable;
            if (boolVariable != null)
            {
                var d = new BoolVar();
                d.Key = boolVariable.Key;
                d.Value = boolVariable.Value;
                engineData.BoolVars.Add(d);
            }

            var floatVariable = v as FloatVariable;
            if (floatVariable != null)
            {
                var d = new FloatVar();
                d.Key = floatVariable.Key;
                d.Value = floatVariable.Value;
                engineData.FloatVars.Add(d);
            }

            var stringVariable = v as StringVariable;
            if (stringVariable != null)
            {
                var d = new StringVar();
                d.Key = stringVariable.Key;
                d.Value = stringVariable.Value;
                engineData.StringVars.Add(d);
            }
        }

        return engineData;
    }

    public static void Decode(EngineData engineData)
    {
        var go = GameObject.Find(engineData.EngineName);
        if (go == null)
        {
            Debug.Log("Failed to find engine with name: " + engineData.EngineName);
            return;
        }

        var engine = go.GetComponent<BasicFlowEngine>();
        if (engine == null)
        {
            Debug.Log("Failed to find engine component in game object: " + engineData.EngineName);
            return;
        }
        for (int i = 0; i < engineData.IntVars.Count; i++)
        {
            var intVar = engineData.IntVars[i];
            engine.SetIntegerVariable(intVar.Key, intVar.Value);
        }
        for (int i = 0; i < engineData.BoolVars.Count; i++)
        {
            var boolVar = engineData.BoolVars[i];
            engine.SetBooleanVariable(boolVar.Key, boolVar.Value);
        }
        for (int i = 0; i < engineData.FloatVars.Count; i++)
        {
            var floatVar = engineData.FloatVars[i];
            engine.SetFloatVariable(floatVar.Key, floatVar.Value);
        }
        for (int i = 0; i < engineData.StringVars.Count; i++)
        {
            var stringVar = engineData.StringVars[i];
            engine.SetStringVariable(stringVar.Key, stringVar.Value);
        }
    }
}
