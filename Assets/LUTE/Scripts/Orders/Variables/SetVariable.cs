using UnityEngine;

[OrderInfo("Variable",
            "Set Variable",
            "Sets a Integer (and more) variable to a new value using a simple arithmetic operation. The value can be a constant or reference another variable of the same type.")]
public class SetVariable : Order, ISerializationCallbackReceiver
{
    [SerializeField] protected AnyVariableAndDataPair variable = new AnyVariableAndDataPair();

    [Tooltip("The type of operation to perform on the variable")]
    [SerializeField] protected SetOperator setOperator;

    protected virtual void SetOperation()
    {
        if (variable.variable == null)
        {
            Debug.LogError("No variable selected");
            return;
        }
        variable.SetOp(setOperator);
    }

    public virtual SetOperator _SetOperator
    {
        get
        {
            return setOperator;
        }
    }

    public override void OnEnter()
    {
        SetOperation();

        Continue();
    }

    public override string GetSummary()
    {
        if (variable.variable == null)
        {
            return "Error: No variable selected";
        }

        string desc = variable.variable.Key;
        desc += " " + VariableUtil.GetSetOperatorDescription(setOperator) + " ";
        desc += variable.GetDataDescription();

        return desc;
    }

    public override bool HasReference(Variable variable)
    {
        return false;
    }

    [Tooltip("Variable to use in expression.")]
    [VariableProperty(AllVariableTypes.VariableAny.Any)]
    [SerializeField] protected Variable var;

    [Tooltip("Integer value to compare against")]
    [SerializeField] protected IntegerData integerData;

    public void OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (var == null)
            return;
        else
            variable.variable = var;

        if (variable.GetType() == typeof(IntegerVariable) && !integerData.Equals(new IntegerData()))
        {
            variable.data.integerData = integerData;
            integerData = new IntegerData();
        }

        variable = null;
    }
}
