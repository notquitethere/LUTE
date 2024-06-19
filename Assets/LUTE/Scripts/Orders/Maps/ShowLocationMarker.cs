using UnityEngine;

[OrderInfo("Map",
             "Reveal Location",
             "Reveals a location marker based on the location provided - can be hidden again using the 'Hide Location Marker' Order")]
[AddComponentMenu("")]
public class ShowLocationMarker : Order
{
    [Tooltip("The location of the marker to show.")]
    [SerializeField] protected LocationVariable location;
    public override void OnEnter()
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

        if (location == null)
        {
            Continue();
            return;
        }

        map.ShowLocationMarker(location);
        Continue();
    }

    public override string GetSummary()
    {
        if (location != null)
            return "Shows location marker at: " + location?.Key;

        return "Error: No location provided.";
    }
}
