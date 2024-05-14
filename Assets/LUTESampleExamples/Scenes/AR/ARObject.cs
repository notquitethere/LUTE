using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARObject : MonoBehaviour
{

    private LocationVariable objectLocation;
    private GameObject objectToPlace;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static ARObject CreateARObject(LocationVariable location, GameObject objectToPlace, string name)
    {

        GameObject go = null;

        go = Instantiate(objectToPlace) as GameObject;
        go.name = name;

        ARObject arObject = go.AddComponent<ARObject>();
        arObject.objectLocation = location;
        arObject.objectToPlace = objectToPlace;

        return arObject;

    }

    private bool CheckLocation()
    {
        return objectLocation.Evaluate(ComparisonOperator.Equals, null);
    }

}
