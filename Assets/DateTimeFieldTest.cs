using UnityEngine;

namespace LoGaCulture.LUTE
{
    public class DateTimeFieldTest : MonoBehaviour
    {
        [SerializeField] protected UDateTime dateTime;
        [SerializeField] protected UTime time;
        [SerializeField] protected UDate date;
        [SerializeField] protected Season season;
        [SerializeField] protected TimeOfDay timeOfDay;
        [SerializeField] protected DaylightCycle daylightCycle;

        [VariableProperty(typeof(UDateTimeVariable))]
        [SerializeField] protected UDateTimeVariable test;
    }
}