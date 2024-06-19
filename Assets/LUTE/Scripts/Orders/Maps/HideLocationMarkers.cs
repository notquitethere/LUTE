using Mapbox.Examples;
using UnityEngine;

[OrderInfo("Map",
             "Hide Locations",
             "Hides a series of location markers based on the locations provided - can be revaled again using the 'Reveal Location Marker(s)' Order")]
[AddComponentMenu("")]
public class HideLocationMarkers : Order
{
    [Tooltip("The locations of the markers to hide.")]
    [SerializeField] protected LocationVariable[] locations;

    private SpawnOnMap map;

    public override void OnEnter()
    {
        var engine = GetEngine();

        if (engine == null)
        {
            Continue();
            return;
        }

        map = engine.GetMap();

        if (map == null)
        {
            Continue();
            return;
        }

        if (locations == null || locations.Length <= 0)
        {
            Continue();
            return;
        }

        //Wait a moment before hiding the locations to ensure the map has been loaded correctly
        Invoke("HideLocations", 0.45f);

        Continue();
    }

    private void HideLocations()
    {
        foreach (LocationVariable location in locations)
        {
            if (location != null)
                map.HideLocationMarker(location);
        }
    }

    public override string GetSummary()
    {
        if (locations != null)
            return "Hides location markers at: " + locations.Length + " locations";

        return "Error: No locations provided.";
    }
}
