using UnityEngine;
using UnityEngine.Events;

[OrderInfo("XR",
              "InteractWithXRObject",
              "Wait for a specific XR Object to be interacted with")]
[AddComponentMenu("")]
public class InteractWithXRObject : Order
{
    [SerializeField] private string _objectName;

    [SerializeField] private InteractorEvent _interactorEvent;

    [Tooltip("Event to call when the object is interacted with")]
    [SerializeField] private UnityEvent _onInteraction;

    private GameObject _xrObject;

    public override void OnEnter()
    {
        // Get the XR object from XRObjectManager
        _xrObject = XRObjectManager.Instance.GetObject(_objectName);

        if (_xrObject == null)
        {
            Debug.LogError($"XR Object with name '{_objectName}' not found.");
            Continue();
            return;
        }

        InteractorEvent interactorEv = _interactorEvent ?? _xrObject.GetComponent<InteractorEvent>();

        if (interactorEv == null)
        {
            interactorEv = _xrObject.AddComponent<InteractorEvent>();
        }

        if (interactorEv.onInteracted == null)
        {
            interactorEv.onInteracted = new UnityEvent();
        }

        interactorEv.onInteracted.AddListener(OnInteract);
    }

    public void OnInteract()
    {
        Debug.Log("Interacted with XR Object");

        if (_xrObject != null)
        {
            var interactorEv = _xrObject.GetComponent<InteractorEvent>();
            if (interactorEv != null)
            {
                interactorEv.onInteracted.RemoveListener(OnInteract);
            }
        }

        _onInteraction.Invoke();
        Continue();
    }

    public override string GetSummary()
    {
        return $"Waits for interaction with XR Object '{_objectName}'.";
    }
}
