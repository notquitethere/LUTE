
//using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "Toggle XR",
              "Toggles the XR object on or off depending on its current state")]
[AddComponentMenu("")]
public class ToggleXR : Order
{

    [SerializeField]
    public bool toggle = true;

    [SerializeField]
    public PlaneDetectionMode planeDetectionMode = PlaneDetectionMode.Horizontal;


    [SerializeField]
    public GameObject planeVisualiser;

    [SerializeField]
    public GameObject pointCloudVisualiser;

    public override void OnEnter()
    {

        //if the plane visualiser is not null, set it to the plane visualiser of the XR object
        if (planeVisualiser != null)
        {
            var planeManager = XRHelper.getXRScript().gameObject.GetComponentInChildren<ARPlaneManager>();
            planeManager.planePrefab = planeVisualiser;
        }

        //if the point cloud visualiser is not null, set it to the point cloud visualiser of the XR object
        if (pointCloudVisualiser != null)
        {
            var pointCloudManager = XRHelper.getXRScript().gameObject.GetComponentInChildren<ARPointCloudManager>();
            pointCloudManager.pointCloudPrefab = pointCloudVisualiser;
        }
            
       
        Continue();
    }

    public override string GetSummary()
    {
        return "Toggles the XR camera on or off depending on the chosen setting";
    }
}