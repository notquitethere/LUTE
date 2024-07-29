using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomEditor(typeof(LocationFailureHandler))]
    public class LocationFailureHandlerEditor : Editor
    {
        private ReorderableList failureMethodsList;
        private SerializedProperty failureMethodsProp;
        private string[] availableMethods;
        private int locationVarIndex;

        private void OnEnable()
        {
            failureMethodsProp = serializedObject.FindProperty("failureMethods");
            failureMethodsList = new ReorderableList(serializedObject, failureMethodsProp, true, true, true, true);

            failureMethodsList.drawElementCallback = DrawFailureMethodElement;
            failureMethodsList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Failure Methods");
            failureMethodsList.elementHeightCallback = GetFailureMethodHeight;

            availableMethods = LocationFailureHandler.GetAvailableMethods();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            failureMethodsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFailureMethodElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = failureMethodsProp.GetArrayElementAtIndex(index);
            var queriedLocationProp = element.FindPropertyRelative("queriedLocation");
            var updateLocationTextProp = element.FindPropertyRelative("updateLocationText");
            var backupLocationsProp = element.FindPropertyRelative("backupLocations");
            var prioritizedMethodsProp = element.FindPropertyRelative("priorityMethods");

            // Draw the queried location and backup locations as a dropdown based on locations derived from the engine
            var engine = BasicFlowEngine.CachedEngines[0];
            var locations = engine.GetComponents<LocationVariable>();
            var locationKeys = locations.Select(x => x.Key).ToArray();

            // Find the index of the currently selected location
            int locationVarIndex = Array.FindIndex(locations, x => x == (queriedLocationProp.objectReferenceValue as LocationVariable));
            if (locationVarIndex == -1) locationVarIndex = 0; // Default to first item if not found

            // Create the dropdown
            EditorGUI.BeginChangeCheck();
            locationVarIndex = EditorGUI.Popup(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                queriedLocationProp.displayName,
                locationVarIndex,
                locationKeys
            );

            // Update the property if the selection changed
            if (EditorGUI.EndChangeCheck() && locations.Length > 0)
            {
                queriedLocationProp.objectReferenceValue = locations[locationVarIndex];
            }

            // Move the rect down for the next control
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Update Location Text
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), updateLocationTextProp);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Create backup locations list excluding the queried location
            var backupLocations = locations.Where(x => x != queriedLocationProp.objectReferenceValue).ToArray();
            var backupLocationKeys = backupLocations.Select(x => x.Key).ToArray();

            // Backup Locations Dropdown
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Backup Locations");
            rect.y += EditorGUIUtility.singleLineHeight;

            for (int i = 0; i < backupLocationsProp.arraySize; i++)
            {
                var backupLocationProp = backupLocationsProp.GetArrayElementAtIndex(i);
                int backupLocationIndex = Array.FindIndex(backupLocations, x => x == (backupLocationProp.objectReferenceValue as LocationVariable));
                if (backupLocationIndex == -1) backupLocationIndex = 0;

                EditorGUI.BeginChangeCheck();
                backupLocationIndex = EditorGUI.Popup(
                    new Rect(rect.x, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight),
                    backupLocationIndex,
                    backupLocationKeys
                );

                if (EditorGUI.EndChangeCheck() && backupLocations.Length > 0)
                {
                    backupLocationProp.objectReferenceValue = backupLocations[backupLocationIndex];
                }

                if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight), "-"))
                {
                    backupLocationsProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // Add button for new backup location
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Add Backup Location"))
            {
                backupLocationsProp.InsertArrayElementAtIndex(backupLocationsProp.arraySize);
            }

            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw the prioritized methods list
            var prioritizedMethodsList = new ReorderableList(prioritizedMethodsProp.serializedObject, prioritizedMethodsProp, true, true, true, true);
            prioritizedMethodsList.drawHeaderCallback = (Rect r) => EditorGUI.LabelField(r, "Prioritized Methods");

            prioritizedMethodsList.drawElementCallback = (Rect r, int i, bool active, bool focused) =>
            {
                var methodProp = prioritizedMethodsProp.GetArrayElementAtIndex(i);
                EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), methodProp, GUIContent.none);
            };

            prioritizedMethodsList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                var menu = new GenericMenu();
                foreach (var method in availableMethods)
                {
                    if (!prioritizedMethodsProp.arrayContains(method))
                    {
                        menu.AddItem(new GUIContent(method), false, () =>
                        {
                            prioritizedMethodsProp.InsertArrayElementAtIndex(prioritizedMethodsProp.arraySize);
                            prioritizedMethodsProp.GetArrayElementAtIndex(prioritizedMethodsProp.arraySize - 1).stringValue = method;
                            prioritizedMethodsProp.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }
                menu.ShowAsContext();
            };

            prioritizedMethodsList.DoList(new Rect(rect.x, rect.y, rect.width, 100)); // Adjust height as needed
        }

        private float GetFailureMethodHeight(int index)
        {
            var element = failureMethodsProp.GetArrayElementAtIndex(index);
            var backupLocationsProp = element.FindPropertyRelative("backupLocations");
            var prioritizedMethodsProp = element.FindPropertyRelative("priorityMethods");

            float height = EditorGUIUtility.singleLineHeight * 2; // queriedLocation + backupLocations
            height += EditorGUI.GetPropertyHeight(backupLocationsProp);
            height += 100; // Height for prioritized methods list (adjust as needed)
            height += EditorGUIUtility.standardVerticalSpacing * 3;

            return height;
        }
    }

    public static class SerializedPropertyExtensions
    {
        public static bool arrayContains(this SerializedProperty property, string value)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).stringValue == value)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
