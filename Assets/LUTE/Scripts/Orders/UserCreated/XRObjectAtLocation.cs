using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "PlaceObjectAtLocation",
              "Place an object at the specified location")]
[AddComponentMenu("")]
public class XRObjectAtLocation : Order
{
    [Tooltip("Where this item will be placed on the map")]
    [SerializeField] private LocationVariable _objectLocation;

    [Tooltip("The object to place at the location")]
    [SerializeField] private GameObject _objectToPlace;

    [Tooltip("The name of the object to place at the location")]
    [SerializeField] private string _objectName;

    private GameObject _xrObject;
    private ARRaycastManager _arRaycastManager;
    private List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();
    private bool _locationInitialized = false;

    private const float EarthRadius = 6378137f; // Earth's radius in meters

    public override void OnEnter()
    {
        _xrObject = XRManager.Instance.GetXRObject();

        if (_xrObject == null)
        {
            Debug.LogError("XR Object not initialized.");
            Continue();
            return;
        }

        StartCoroutine(InitializeLocationServices());
    }

    private IEnumerator InitializeLocationServices()
    {
        // Check if location services are enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services are not enabled by the user.");
            Continue();
            yield break;
        }

        // Start location services
        Input.location.Start();

        // Wait for location services to initialize
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Check if initialization timed out
        if (maxWait < 1)
        {
            Debug.LogError("Location services initialization timed out.");
            Continue();
            yield break;
        }

        // Check if connection failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Failed to initialize location services.");
            Continue();
            yield break;
        }

        _locationInitialized = true;
        Debug.Log($"Location initialized: {Input.location.lastData.latitude}, {Input.location.lastData.longitude}");

        // Proceed to check proximity to target location
        StartCoroutine(CheckProximityToTargetLocation());
    }

    private IEnumerator CheckProximityToTargetLocation()
    {
        // Wait until the device is within the target radius
        while (true)
        {
            if (IsWithinTargetRadius())
            {
                Debug.Log("Device is within target radius.");
                InitializePlaneDetection();
                yield break;
            }
            else
            {
                Debug.Log("Device is not within target radius. Waiting...");
                yield return new WaitForSeconds(5); // Wait for 5 seconds before checking again
            }
        }
    }

    private bool IsWithinTargetRadius()
    {
        Vector2d targetLatLon = _objectLocation.Value.LatLongString();
        Vector2d deviceLatLon = new Vector2d(Input.location.lastData.latitude, Input.location.lastData.longitude);

        float distance = GetDistanceInMeters(targetLatLon, deviceLatLon);
        float radiusInMeters = 1.0f; // You can adjust the radius as needed

        return distance <= radiusInMeters;
    }

    private float GetDistanceInMeters(Vector2d point1, Vector2d point2)
    {
        // Haversine formula
        double lat1Rad = point1.x * Mathf.Deg2Rad;
        double lat2Rad = point2.x * Mathf.Deg2Rad;
        double deltaLat = (point2.x - point1.x) * Mathf.Deg2Rad;
        double deltaLon = (point2.y - point1.y) * Mathf.Deg2Rad;

        double a = Mathf.Sin((float)(deltaLat / 2)) * Mathf.Sin((float)(deltaLat / 2)) +
                   Mathf.Cos((float)lat1Rad) * Mathf.Cos((float)lat2Rad) *
                   Mathf.Sin((float)(deltaLon / 2)) * Mathf.Sin((float)(deltaLon / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        double distance = EarthRadius * c;

        return (float)distance;
    }

    private void InitializePlaneDetection()
    {
        var planeManager = _xrObject.GetComponentInChildren<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not found in XR Object.");
            Continue();
            return;
        }

        _arRaycastManager = _xrObject.GetComponentInChildren<ARRaycastManager>();
        if (_arRaycastManager == null)
        {
            Debug.LogError("ARRaycastManager not found in XR Object.");
            Continue();
            return;
        }

        // Subscribe to planesChanged event
        planeManager.planesChanged += OnPlanesChanged;
        Debug.Log("Plane detection initialized.");
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added != null && args.added.Count > 0)
        {
            // Unsubscribe after first plane detection
            var planeManager = _xrObject.GetComponentInChildren<ARPlaneManager>();
            if (planeManager != null)
            {
                planeManager.planesChanged -= OnPlanesChanged;
            }

            // Perform raycast to place object
            TryPlaceObject();
        }
    }

    private void TryPlaceObject()
    {
        // Perform a raycast from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (_arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            // Instantiate the object at the hit position
            GameObject obj = Instantiate(_objectToPlace, hitPose.position, hitPose.rotation);
            obj.name = _objectName;

            XRObjectManager.Instance.AddObject(_objectName, obj);

            Debug.Log($"Object '{_objectName}' placed at {hitPose.position}");

            Continue();
        }
        else
        {
            Debug.Log("Raycast did not hit any planes.");
            
        }
    }

    public override string GetSummary()
    {
        return "Places an object at a specified location";
    }
}
