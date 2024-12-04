using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARHelper : MonoBehaviour
{

    public static GameObject xrObject;
    private static GameObject spawnedObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public static bool initiliaseXR()
    {
        if(xrObject == null)
        {
            xrObject = Resources.Load<GameObject>("XR");
        }

        //if the xrObject is not in the scene, then add it
        if(GameObject.Find("XR") == null)
        {
            spawnedObject = GameObject.Instantiate(xrObject);
            return true;
        }
        return false;
    }

    public static bool toggleXR()
    {
        //if the xrObject is in the scene, then remove it
        if(spawnedObject.activeSelf)
        {
            spawnedObject.SetActive(false);
            return false;
        }
        else
        {
            spawnedObject.SetActive(true);
            return true;
        }
 
    }

    public static bool setXRActive(bool active)
    {
        spawnedObject.SetActive(active);
        return active;
    }

    public static GameObject getXRObject()
    {
        return spawnedObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
