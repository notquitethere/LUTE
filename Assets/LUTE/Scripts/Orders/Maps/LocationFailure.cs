using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("Map",
                 "Location Failure",
                 "Determines location automatically or provided by designer/player and updates the inaccesible location with an accesible one")]
    [AddComponentMenu("")]
    public class LocationFailure : Order
    {
        [Tooltip("The location that is inaccesible - if left empty the nearest location to the player is chosen.")]
        [SerializeField] protected LocationVariable location;

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

        public override void OnEnter()
        {
            var engine = GetEngine();
            if (engine == null)
            {
                Continue();
                return;
            }
            var locationFailureHandler = engine.GetComponent<LocationFailureHandler>();
            if (locationFailureHandler == null)
            {
                Continue();
                return;
            }
            if (location != null)
            {
                // If location is provided by designer/player
                // Do the location failure handling based on this location
                var loc2d = Conversions.StringToLatLon(location.Value);
                if (loc2d != Vector2d.zero)
                {
                    locationFailureHandler.HandleFailure(loc2d);
                    //LocationFailureHandler.HandleFailure(loc2d);
                }
                Continue();
                return;
            }
            var map = engine.GetMap();
            if (map == null)
            {
                Continue();
                return;
            }
            var tracker = map.TrackerPos();
            if (tracker == null)
            {
                Continue();
                return;
            }
            if (LocationProvider == null)
            {
                Continue();
                return;
            }

            var trackerPos = tracker;
            var deviceLoc = LocationProvider.CurrentLocation.LatitudeLongitude;

            // Find the closest location variable to either tracker pos or device loc

            Vector2d closestLocation = Vector2d.zero;

            if (engine.DemoMapMode)
            {
                // Using demo map mode so use dummy
                if (trackerPos != null)
                {
                    closestLocation = FindClosestLocation(trackerPos, engine);
                }
            }
            else
            {
                // Use the actual location of the device
                if (deviceLoc != null)
                {
                    closestLocation = FindClosestLocation(deviceLoc, engine);
                }
            }
            // Do the location failure handling based on the closest location
            if (closestLocation != Vector2d.zero)
            {
                locationFailureHandler.HandleFailure(closestLocation);

                //LocationFailureHandler.HandleFailure(closestLocation);
            }
            Continue();
        }

        private Vector2d FindClosestLocation(Vector2d location, BasicFlowEngine engine)
        {
            if (engine == null)
            {
                return Vector2d.zero;
            }

            var locations = engine.GetComponents<LocationVariable>();
            if (locations == null || locations.Length == 0)
            {
                return Vector2d.zero;
            }

            LocationVariable closestLocation = null;
            double closestDistance = double.MaxValue;

            foreach (var loc in locations)
            {
                if (string.IsNullOrEmpty(loc.Value))
                {
                    Debug.LogWarning($"LocationVariable {loc.name} has no value");
                    continue;
                }

                Vector2d locPosition;
                try
                {
                    locPosition = Conversions.StringToLatLon(loc.Value);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to convert location value '{loc.Value}' to Vector2d: {e.Message}");
                    continue;
                }

                double distance = Vector2d.Distance(location, locPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLocation = loc;
                }
            }

            var loc2d = Conversions.StringToLatLon(closestLocation.Value);
            return loc2d;
        }

        public override string GetSummary()
        {
            if (location != null)
                return "Location Failure at: " + location?.Key;

            return "    Location Failure at automatic location provided by device.";
        }
    }
}
