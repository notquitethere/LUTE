// This needs to be created in similar way to dialogue boxes
// If one already exists can we just swap out the object with children?

// Reusing the same object will require knowing:
// -- Object info, mesh of the object, sprite of the hidden object
// -- But would require us to randomly place the hidden object on every intitialise
// -- Otherwise we must delete current object then spawn it again

// ---- ensure that the object info is not on this class but in another one

namespace LoGaCulture.LUTE
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ObjectSpinner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        [SerializeField] protected float rotationSpeed = 5f;
        [SerializeField] protected float frictionCoefficient = 0.95f; // Controls how quickly the spin slows down

        private Vector3 angularVelocity;

        void Update()
        {
            if (angularVelocity.magnitude > 0.01f)
            {
                // Apply momentum-based rotation
                transform.Rotate(Vector3.right, angularVelocity.x * Time.deltaTime, Space.World);
                transform.Rotate(Vector3.up, angularVelocity.y * Time.deltaTime, Space.World);
                // Apply friction
                angularVelocity *= frictionCoefficient;
            }
            else
            {
                angularVelocity = Vector3.zero;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            angularVelocity = Vector3.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float rotX = -eventData.delta.y * rotationSpeed * Time.deltaTime;
            float rotY = eventData.delta.x * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.right, rotX, Space.World);
            transform.Rotate(Vector3.up, rotY, Space.World);

            // Calculate angular velocity
            angularVelocity = new Vector3(rotX, rotY, 0) / Time.deltaTime;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // The angular velocity is already set, so we don't need to do anything here
        }
    }
}
