using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;

[VariableInfo("", "Location")]
[AddComponentMenu("")]
[System.Serializable]
public class LocationVariable : BaseVariable<string>
{
    [SerializeField]
    public Sprite locationSprite;
    public Color locationColor = Color.white;
    public bool showLocationName = true;

    ILocationProvider _locationProvider;
    ILocationProvider LocationProvider
    {
        get
        {
            if (_locationProvider == null)
            {
                _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
            }

            return _locationProvider;
        }
    }

    public override bool Evaluate(ComparisonOperator comparisonOperator, string value)
    {
        bool condition = false;
        Vector2 location = Vector2.zero;
        switch (comparisonOperator)
        {
            case ComparisonOperator.Equals:
                condition = IsWithinRadius();
                break;
            case ComparisonOperator.NotEquals:
                condition = !IsWithinRadius();
                break;
            default:
                condition = base.Evaluate(comparisonOperator, value);
                break;
        }

        return condition;
    }

    private bool IsWithinRadius()
    {
        var engine = GetEngine();
        var map = engine.GetMap();
        var tracker = map.TrackerPos();
        var trackerPos = tracker;
        var radius = 0.00025f;

        Vector2d vecVal = Conversions.StringToLatLon(Value);
        var deviceLoc = LocationProvider.CurrentLocation.LatitudeLongitude;

        //If engine is not in demo mode then use the real device location
        if (!engine.DemoMapMode)
        {
            if (deviceLoc != null)
            {
                var distance = Vector2d.Distance(vecVal, deviceLoc);
                return distance <= radius;
            }
            else
                return false;
        }
        //Otherwise we use the tracker position in the hierarchy if it exists
        else
        {
            radius = 0.00035f;
            if (trackerPos != null)
            {
                var distance = Vector2d.Distance(vecVal, trackerPos);
                return distance <= radius;
            }
            else
                return false;
        }
    }

    public override bool SupportsArithmetic(SetOperator setOperator)
    {
        return true;
    }

    public override void Apply(SetOperator setOperator, string value)
    {
        switch (setOperator)
        {
            // case SetOperator.Negate:
            //     Value = Value * -1;
            //     break;
            // case SetOperator.Add:
            //     Value += value;
            //     break;
            // case SetOperator.Subtract:
            //     Value -= value;
            //     break;
            //             case SetOperator.Multiply:
            // #if UNITY_2019_2_OR_NEWER
            //                 Value *= value;
            // #else
            //                 var tmpM = Value;
            //                 tmpM.Scale(value);
            //                 Value = tmpM;
            // #endif
            //                 break;
            //             case SetOperator.Divide:
            // #if UNITY_2019_2_OR_NEWER
            //                 Value /= value;
            // #else
            //                 var tmpD = Value;
            //                 tmpD.Scale(new Vector2(1.0f / value.x, 1.0f / value.y));
            //                 Value = tmpD;
            // #endif
            //                 break;
            default:
                base.Apply(setOperator, value);
                break;
        }
    }
}

/// Container for a Vector2 variable reference or constant value.
[System.Serializable]
public struct LocationData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(LocationVariable))]
    public LocationVariable locationRef;

    [SerializeField]
    public string locationVal;

    public LocationData(string v)
    {
        locationVal = v;
        locationRef = null;
    }

    public static implicit operator string(LocationData stringData)
    {
        return stringData.Value;
    }

    public string Value
    {
        get { return (locationRef == null) ? locationVal : locationRef.Value; }
        set { if (locationRef == null) { locationVal = value; } else { locationRef.Value = value; } }
    }

    public string GetDescription()
    {
        if (locationRef == null)
        {
            return locationVal.ToString();
        }
        else
        {
            return locationRef.Key;
        }
    }
}
