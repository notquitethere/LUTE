using UnityEngine;

namespace LoGaCulture.LUTE
{
    public class DateTimeFieldTest : MonoBehaviour
    {
        [SerializeField] protected UDateTime dateTime;
        [SerializeField] protected UDateTimeData dateTimeVariable;

        protected virtual void Update()
        {
            Debug.Log(dateTimeVariable.Value.dateTime + " and the type is: " + dateTimeVariable.Value.dateTime.GetType());
        }
    }
}