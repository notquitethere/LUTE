using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "PlaceObjectAtLocation",
              "Place an object at the speciied location")]
[AddComponentMenu("")]
public class XRObjectAtLocation : Order
{


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


    [Tooltip("Where this item will be placed on the map")]
    [SerializeField] protected LocationVariable objectLocation;

    [Tooltip("The object to place at the location")]
    [SerializeField] protected GameObject objectToPlace;

    //[SerializeField] protected bool placeOnPlaceDetected = true;

    //name of the object to place
    [Tooltip("The name of the object to place at the location")]
    [SerializeField] protected string objectName;


    private GameObject xrObject;

    bool locationInit = false;

   IEnumerator start()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.LogError("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.LogError("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            Debug.LogError("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            locationInit = true;

            //Debug.Log("Trying to use mapbox to get location");

            //Debug.Log("Is this null" + LocationProvider);

            //var deviceLoc = LocationProvider.CurrentLocation.LatitudeLongitude;

            //Debug.LogError("Device Location based on mapbox: " + deviceLoc.x + " " + deviceLoc.y);

        }


    }


    public override void OnEnter()
    {

       xrObject = XRHelper.getXRScript().gameObject;


       StartCoroutine(start());

       
        //get the AR plane manager
        var planeManager = xrObject.GetComponentInChildren<ARPlaneManager>();

        Debug.Log(planeManager.gameObject);

        planeManager.planesChanged += OnPlaneDetected;

        //if (raycastHitEvent != null)
        //{
        //    if (automaticallyPlaceObject)
        //    {
               
        //    }
        //    else
        //    {
        //        raycastHitEvent.eventRaised += MoveObject;
        //        raycastHitEvent.eventRaised += PlaceObjectAt;
        //    }
        //}




        //versions.GeoToWorldPosition(objectLocation., out Vector3 position);
        //this code gets executed as the order is called
        //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
        //Continue();
    }

    private float GetLongitudeDegreeDistance(float latitude)
    {
        return 111319.9f * Mathf.Cos(latitude * (Mathf.PI / 180));
    }

    Vector2 GamePosToGPS(Vector3 pos)
    {
        // Real GPS Position - This will be the world origin.
        var gpsLat = Input.location.lastData.latitude;
        var gpsLon = Input.location.lastData.longitude;

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        float degreesLongitudeInMetersAtEquator = 111319.9f;

        // GPS position converted into unity coordinates
        var latOffset = pos.x / degreesLatitudeInMeters;
        var lonOffset = pos.z / GetLongitudeDegreeDistance(gpsLat);

        // Real world position of object. Need to update with something near your own location.
        float latitude = gpsLat + latOffset;
        float longitude = gpsLon + lonOffset;

        return new Vector2(latitude, longitude);

    }

    void SpawnObject()
    {
        // Real world position of object. Need to update with something near your own location.
        float latitude = -27.469093f;
        float longitude = 153.023394f;

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        float degreesLongitudeInMetersAtEquator = 111319.9f;

        // Real GPS Position - This will be the world origin.
        var gpsLat = Input.location.lastData.latitude;
        var gpsLon = Input.location.lastData.longitude;
        // GPS position converted into unity coordinates
        var latOffset = (latitude - gpsLat) * degreesLatitudeInMeters;
        var lonOffset = (longitude - gpsLon) * GetLongitudeDegreeDistance(latitude);

        // Create object at coordinates
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.transform.position = new Vector3(latOffset, 0, lonOffset);
        obj.transform.localScale = new Vector3(4, 4, 4);
    }

    private void OnPlaneDetected(ARPlanesChangedEventArgs args)
    {

        Debug.Log("Plane detected");

        foreach (var plane in args.added)
        {
            
            if(plane.alignment == PlaneAlignment.HorizontalUp)
            {

                Debug.Log("Horizontal plane detected");

                //do a raycast from the camera to the plane and get the position of the hit
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;

                Debug.Log("Raycasting");
                
                //once hit, get the position of the hit
                if (Physics.Raycast(ray, out hit))
                {
                   //convert the position to real world coordinates
                    var position = hit.point;

                    Debug.Log("Hit point: " + position);

                    //get the latitude and longitude of the position
                    var latLon = GamePosToGPS(position);

                    Debug.Log("LatLon: " + latLon);

                    //check if the object is within a radius of 1 meter from the location
                    if (Vector2.Distance(new Vector2(latLon.x, latLon.y), new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude)) <= 1)
                    {

                        Debug.Log("Object within 1 meter of location");

                        // Instantiate the game object
                        var obj = Instantiate(objectToPlace, position, Quaternion.identity);

                        XRObjectManager.AddObject(objectName, obj);

                        Continue();

                    }

                    

                }


            }

        }

    }

    private void Update()
    {
        
    }

    public override string GetSummary()
  {
 //you can use this to return a summary of the order which is displayed in the inspector of the order
      return "Places an object at a specified location";
  }
}