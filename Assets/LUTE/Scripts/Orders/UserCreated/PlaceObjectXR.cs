using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit;

[OrderInfo("XR", "PlaceObjectOnPlane", "")]
[AddComponentMenu("")]
public class PlaceObjectXR : Order
{
    [Tooltip("The 3D object to place when clicked")]
    [SerializeField] private GameObject m_PrefabToPlace;

    [SerializeField] private string m_ObjectName;

    [SerializeField]
    [Tooltip("The Scriptable Object Asset that contains the ARRaycastHit event.")]
    private ARRaycastHitEventAsset raycastHitEvent;

    [SerializeField]
    public bool automaticallyPlaceObject = true;

    [SerializeField]
    public bool rotateable = true;
    [SerializeField]
    public bool scaleable = true;
    [SerializeField]
    public bool moveable = true;

    [SerializeField]
    public PlaneAlignment planeAlignment;

    private GameObject m_SpawnedObject;
    private ARPlaneManager planeManager;

    private void OnObjectSpawned(GameObject obj)
    {
        Debug.Log("Object spawned");

        var xrManager = XRManager.Instance;
        if (xrManager == null)
        {
            Debug.LogError("XRManager instance is null.");
            return;
        }

        var arObjectInstance = xrManager.GetXRObject();
        if (arObjectInstance == null)
        {
            Debug.LogError("XR object is not initialized.");
            return;
        }

        var objectSpawner = arObjectInstance.GetComponentInChildren<ObjectSpawner>();
        if (objectSpawner == null)
        {
            Debug.LogError("ObjectSpawner not found in XR object.");
            return;
        }

        objectSpawner.objectSpawned -= OnObjectSpawned;

        XRGrabInteractable grabInteractable = obj.GetComponentInChildren<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.trackPosition = moveable;
            grabInteractable.trackRotation = rotateable;
            grabInteractable.trackScale = scaleable;
        }
        else
        {
            Debug.LogWarning("XRGrabInteractable component not found on spawned object.");
        }

        objectSpawner.objectPrefabs.Remove(m_PrefabToPlace);
        ObjectSpawner.IsCurrentlyPlacingObject = false;

        XRObjectManager.Instance.AddObject(m_ObjectName, obj);

        Continue();
    }

    public override void OnEnter()
    {
        var xrManager = XRManager.Instance;
        if (xrManager == null)
        {
            Debug.LogError("XRManager instance is null.");
            Continue();
            return;
        }

        GameObject arObjectInstance = xrManager.GetXRObject();
        if (arObjectInstance == null)
        {
            Debug.LogError("XR object is not initialized.");
            Continue();
            return;
        }

        planeManager = arObjectInstance.GetComponentInChildren<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not found in XR object.");
            Continue();
            return;
        }

        Debug.Log(planeManager.gameObject);

        ObjectSpawner objectSpawner = arObjectInstance.GetComponentInChildren<ObjectSpawner>();
        if (objectSpawner == null)
        {
            Debug.LogError("ObjectSpawner not found in XR object.");
            Continue();
            return;
        }

        objectSpawner.objectSpawned += OnObjectSpawned;

        if (!objectSpawner.objectPrefabs.Contains(m_PrefabToPlace))
        {
            objectSpawner.objectPrefabs.Add(m_PrefabToPlace);
        }

        ObjectSpawner.IsCurrentlyPlacingObject = true;

        if (m_PrefabToPlace == null)
        {
            Debug.LogWarning($"{nameof(PlaceObjectXR)} component on {name} has null m_PrefabToPlace and will have no effect in this scene.", this);
            Continue();
            return;
        }

        if (automaticallyPlaceObject)
        {
            planeManager.planesChanged += OnPlaneDetected;
        }
        else
        {
            // If not automatic, set up event handling as needed
        }
    }

    private void OnPlaneDetected(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            if (plane.alignment == planeAlignment)
            {
                GameObject go = Instantiate(m_PrefabToPlace, plane.transform.position, plane.transform.rotation);
                go.transform.parent = plane.transform;

                XRObjectManager.Instance.AddObject(m_ObjectName, go);

                // Unsubscribe to prevent multiple placements
                planeManager.planesChanged -= OnPlaneDetected;

                Continue();
                break;
            }
        }
    }

    public override string GetSummary()
    {
        return "Places an object on a detected plane, automatically or manually";
    }
}



//Object Manager class that manages all the AR objects that are placed in the scene
//it's static so that it can be accessed from anywhere in the scene
public class XRObjectManager : MonoBehaviour
{
    private static XRObjectManager _instance;

    public static XRObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindObjectOfType<XRObjectManager>();
                if (_instance == null)
                {
                    // Create a new GameObject and attach XRObjectManager
                    GameObject xrObjectManagerObject = new GameObject("XRObjectManager");
                    _instance = xrObjectManagerObject.AddComponent<XRObjectManager>();
                    // Optionally, make it persist across scenes
                    // DontDestroyOnLoad(xrObjectManagerObject);
                }
            }
            return _instance;
        }
    }

    // Dictionary to store the objects, mapping name to GameObject
    private Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();

    public void AddObject(string name, GameObject obj)
    {
        if (_objects.ContainsKey(name))
        {
            Debug.LogWarning($"Object with name {name} already exists in XRObjectManager. Overwriting the existing object.");
            _objects[name] = obj;
        }
        else
        {
            _objects.Add(name, obj);
        }
    }

    public void RemoveObject(string name)
    {
        if (_objects.ContainsKey(name))
        {
            _objects.Remove(name);
        }
        else
        {
            Debug.LogWarning($"Object with name {name} does not exist in XRObjectManager.");
        }
    }

    public GameObject GetObject(string name)
    {
        if (_objects.ContainsKey(name))
        {
            return _objects[name];
        }
        else
        {
            Debug.LogError($"Object with name {name} not found in XRObjectManager.");
            return null;
        }
    }
}