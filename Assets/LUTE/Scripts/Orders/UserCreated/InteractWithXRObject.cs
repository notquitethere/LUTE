using UnityEngine;
using UnityEngine.Events;

[OrderInfo("XR",
              "InteractWithXRObject",
              "Wait for a specific XR Object to be interacted with")]
[AddComponentMenu("")]
public class InteractWithXRObject : Order
{

    [SerializeField] protected string objectName;

    [SerializeField] protected InteractorEvent interactorEvent;

    [Tooltip("Event to call when the object is interacted with")]
    [SerializeField] protected UnityEvent onInteraction;


    private GameObject xrObject;

    public override void OnEnter()
    {

        xrObject = XRObjectManager.GetObject(objectName);

        if (interactorEvent == null)
        {

            var interactorEv = xrObject.AddComponent<InteractorEvent>();

            if(interactorEv.onInteracted == null)
            {
                interactorEv.onInteracted = new UnityEvent();
            }

            interactorEv.onInteracted.AddListener(OnInteract);
        }
        else
        {
            interactorEvent.onInteracted.AddListener(OnInteract);
        }
      //this code gets executed as the order is called
      //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
      //Continue();
    }

    public void OnInteract()
    {
        Debug.Log("Interacted with XR Object");
        xrObject.GetComponent<InteractorEvent>().onInteracted.RemoveListener(OnInteract);
        onInteraction.Invoke();
        Continue();
    }

    private void Update()
    {
 
    }

    public override string GetSummary()
  {
 //you can use this to return a summary of the order which is displayed in the inspector of the order
      return "";
  }
}