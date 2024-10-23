using UnityEngine;

// Standard comparison operators.
public enum ComparisonOperator
{
    /// <summary> == mathematical operator.</summary>
    Equals,
    /// <summary> != mathematical operator.</summary>
    NotEquals,
    /// <summary> < mathematical operator.</summary>
    LessThan,
    /// <summary> > mathematical operator.</summary>
    GreaterThan,
    /// <summary> <= mathematical operator.</summary>
    LessThanOrEquals,
    /// <summary> >= mathematical operator.</summary>
    GreaterThanOrEquals
}

// Mathematical operations that can be performed on variables.
public enum SetOperator
{
    /// <summary> = operator. </summary>
    Assign,
    /// <summary> =! operator. </summary>
    Negate,
    /// <summary> += operator. </summary>
    Add,
    /// <summary> -= operator. </summary>
    Subtract,
    /// <summary> *= operator. </summary>
    Multiply,
    /// <summary> /= operator. </summary>
    Divide
}

public enum VariableScope
{
    /// <summary> Can only be accessed by commands in the same Engine. </summary>
    Private,
    /// <summary> Can be accessed from any command in any Engine. </summary>
    Public,
    /// <summary> Creates and/or references a global variable of that name, all variables of this name and scope share the same underlying variable and exist for the duration of the instance of Unity.</summary>
    Global,
}

// Helper class for variable info attribute - used to display variable info in the inspector
// Sealed class means it cannot be inherited from
public sealed class VariableInfoAttribute : System.Attribute
{
    //Note do not use "isPreviewedOnly:true", it causes the script to fail to load without errors shown
    public VariableInfoAttribute(string category, string variableType, int order = 0, bool isPreviewedOnly = false)
    {
        this.Category = category;
        this.VariableType = variableType;
        this.Order = order;
        this.IsPreviewedOnly = isPreviewedOnly;
    }

    public string Category { get; set; }
    public string VariableType { get; set; }
    public int Order { get; set; }
    public bool IsPreviewedOnly { get; set; }
}

// Attribute class for variable properties - used to display variable properties in the inspector
public sealed class VariablePropertyAttribute : PropertyAttribute
{
    public VariablePropertyAttribute(params System.Type[] variableTypes)
    {
        this.VariableTypes = variableTypes;
    }

    public VariablePropertyAttribute(AllVariableTypes.VariableAny any)
    {
        VariableTypes = AllVariableTypes.AllLogaVarTypes;
    }

    public VariablePropertyAttribute(string defaultText, params System.Type[] variableTypes)
    {
        this.defaultText = defaultText;
        this.VariableTypes = variableTypes;
    }

    public string defaultText = "<None>";
    public string compatibleVariableName = string.Empty;

    public System.Type[] VariableTypes { get; set; }
}

[RequireComponent(typeof(BasicFlowEngine))]
[System.Serializable]
public abstract class Variable : MonoBehaviour
{
    [SerializeField] protected VariableScope scope;
    [SerializeField] protected string key = "";

    public virtual VariableScope Scope
    {
        get { return scope; }
        set { scope = value; }
    }

    public virtual string Key
    {
        get { return key; }
        set { key = value; }
    }

    // If the engine is reset then this callback is used to reset the variable to its default value
    public abstract void OnReset();

    // Used by SetVariable, child classes required to declare and implement operators
    public abstract void Apply(SetOperator setOperator, object value);

    // Used by ifs, while etc. - child classes required to declare and implement comparisons
    public abstract bool Evaluate(ComparisonOperator comparisonOperator, object value);

    // Does the underlying type provide support for +-*/ operations?
    public virtual bool SupportsArithmetic(SetOperator setOperator) { return false; }

    // Does the underlying type provide support for ==/!=/< etc. operations?
    public virtual bool SupportsComparison() { return false; }

    // Referenced value of type defined within child classes; intended for editor code
    public abstract object GetValue();

    // Variables are required to be on an engine object so this method is used as a helper
    public virtual BasicFlowEngine GetEngine() { return GetComponent<BasicFlowEngine>(); }
}

// Base class for all variable types - provides a common interface for accessing variable properties which is inherented from above
public abstract class BaseVariable<T> : Variable
{
    //cache mechanism for global static variables
    private BaseVariable<T> _globalStaticRef;
    private BaseVariable<T> globalStaticRef
    {
        get
        {
            if (_globalStaticRef != null)
            {
                return _globalStaticRef;
            }
            else if (Application.isPlaying)
            {
                return _globalStaticRef = LogaManager.Instance.GlobalVariables.GetOrAddVariable(Key, value, this.GetType());
            }
            else
            {
                return null;
            }
        }
    }

    [SerializeField] protected T value;

    public virtual T Value
    {
        get
        {
            if (scope != VariableScope.Global || !Application.isPlaying)
            {
                return this.value;
            }
            else
            {
                return globalStaticRef.value;
            }
        }
        set
        {
            if (scope != VariableScope.Global || !Application.isPlaying)
            {
                this.value = value;
            }
            else
            {
                globalStaticRef.Value = value;
            }
        }
    }

    public override object GetValue()
    {
        return value;
    }

    protected T startValue;

    public override void OnReset()
    {
        Value = startValue;
    }

    public override string ToString()
    {
        if (Value != null)
            return Value.ToString();
        else
            return "null";
    }

    protected virtual void Start()
    {
        //cache the start value to use later on
        startValue = Value;
    }

    // Apply to get from base system.object to T
    public override void Apply(SetOperator setOperator, object value)
    {
        if (value is T || value == null)
            Apply(setOperator, (T)value);
        else if (value is BaseVariable<T>)
        {
            var bv = value as BaseVariable<T>;
            Apply(setOperator, bv.value);
        }
        else
        {
            Debug.LogError("Cannot do Apply on variable, as object type: " + value.GetType().Name + " is incompatible with " + typeof(T).Name);
        }
    }

    public virtual void Apply(SetOperator setOperator, T value)
    {
        switch (setOperator)
        {
            case SetOperator.Assign:
                Value = value;
                break;
            default:
                Debug.LogError("Cannot do Apply on variable, as operator: " + setOperator.ToString() + " is not supported");
                break;
        }
    }

    // Evaluate to get from base system.object to T
    public override bool Evaluate(ComparisonOperator comparisonOperator, object value)
    {
        if (value is T || value == null)
        {
            return Evaluate(comparisonOperator, (T)value);
        }
        else if (value is BaseVariable<T>)
        {
            var vbg = value as BaseVariable<T>;
            return Evaluate(comparisonOperator, vbg.Value);
        }
        else
        {
            Debug.LogError("Cannot do Evaluate on variable, as object type: " + value.GetType().Name + " is incompatible with " + typeof(T).Name);
        }

        return false;
    }

    public virtual bool Evaluate(ComparisonOperator comparisonOperator, T value)
    {
        bool condition = false;
        switch (comparisonOperator)
        {
            case ComparisonOperator.Equals:
                condition = Equals(Value, value);
                break;
            case ComparisonOperator.NotEquals:
                condition = !Equals(Value, value);
                break;
            default:
                Debug.LogError("Cannot do Evaluate on variable, as operator: " + comparisonOperator.ToString() + " is not supported");
                break;
        }
        return condition;
    }

    public override bool SupportsArithmetic(SetOperator setOperator)
    {
        return setOperator == SetOperator.Assign || base.SupportsArithmetic(setOperator);
    }
}