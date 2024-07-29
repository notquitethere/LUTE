using UnityEngine;

[OrderInfo("Map",
             "Hide Location",
             "Hides a location marker based on the location provided - can be shown again using the 'Show Location Marker' Order")]
[AddComponentMenu("")]
public class HideLocationMarker : Order
{
    [Tooltip("The location of the marker to hide.")]
    [SerializeField] protected LocationVariable location;
    public override void OnEnter()
    {
        if (location == null)
        {
            Continue();
            return;
        }

        HideLocation();

        Continue();
    }

    private void HideLocation()
    {
        var engine = GetEngine();

        if (engine == null)
        {
            Continue();
            return;
        }

        var map = engine.GetMap();

        if (map == null)
        {
            Continue();
            return;
        }
        map.HideLocationMarker(location);
    }

    public override string GetSummary()
    {
        if (location != null)
            return "Hides location marker at: " + location?.Key;

        return "Error: No location provided.";
    }
}
