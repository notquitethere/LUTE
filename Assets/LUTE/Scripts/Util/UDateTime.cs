#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

[System.Serializable]
public class UDateTime : ISerializationCallbackReceiver
{
    [HideInInspector] public DateTime dateTime;

    [HideInInspector][SerializeField] private string _dateTime;

    public static implicit operator DateTime(UDateTime udt)
    {
        return udt.dateTime;
    }

    public static implicit operator UDateTime(DateTime dt)
    {
        return new UDateTime() { dateTime = dt };
    }

    public void OnAfterDeserialize()
    {
        DateTime.TryParse(_dateTime, out dateTime);
    }

    public void OnBeforeSerialize()
    {
        _dateTime = dateTime.ToString();
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UDateTime))]
public class UDateTimeDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Get the UDateTime object from the property
        SerializedProperty dateTimeProp = property.FindPropertyRelative("_dateTime");
        DateTime dateTime;
        DateTime.TryParse(dateTimeProp.stringValue, out dateTime);

        // Rects for hour and minute dropdowns
        float fieldWidth = position.width / 2 - 5;
        Rect hourRect = new Rect(position.x, position.y, fieldWidth, position.height);
        Rect minuteRect = new Rect(position.x + fieldWidth + 10, position.y, fieldWidth, position.height);

        // Hour and minute arrays for dropdown
        int[] hours = new int[24];
        int[] minutes = new int[60];
        for (int i = 0; i < 24; i++) hours[i] = i;
        for (int i = 0; i < 60; i++) minutes[i] = i;

        // Create dropdowns for hours and minutes
        int selectedHour = dateTime.Hour;
        int selectedMinute = dateTime.Minute;

        selectedHour = EditorGUI.IntPopup(hourRect, selectedHour, Array.ConvertAll(hours, h => h.ToString()), hours);
        selectedMinute = EditorGUI.IntPopup(minuteRect, selectedMinute, Array.ConvertAll(minutes, m => m.ToString()), minutes);

        // Update the DateTime object with the selected values
        dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, selectedHour, selectedMinute, 0);

        // Write the updated dateTime back to the serialized property
        dateTimeProp.stringValue = dateTime.ToString();

        EditorGUI.EndProperty();
    }
}
#endif
