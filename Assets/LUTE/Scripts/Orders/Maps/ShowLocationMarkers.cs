using Mapbox.Examples;
using UnityEngine;

[OrderInfo("Map",
             "Reveal Locations",
             "Reveals a series of location markers based on the locations provided - can be hidden again using the 'Hide Location Marker(s)' Order")]
[AddComponentMenu("")]
public class ShowLocationMarkers : Order
{
    [Tooltip("The locations of the markers to reveal.")]
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
        Invoke("ShowLocations", 0.45f);

        Continue();
    }

    private void ShowLocations()
    {
        foreach (LocationVariable location in locations)
        {
            if (location != null)
                map.ShowLocationMarker(location);
        }
    }

    public override string GetSummary()
    {
        if (locations != null)
            return "Shows location markers at: " + locations.Length + " locations";

        return "Error: No locations provided.";
    }
}
