using System;
using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomPropertyDrawer(typeof(LUTECustomPropAttribute))]
    public class LUTECustomPropDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldType = fieldInfo.FieldType;

            // Call your desired method to process the field value and type
            CreateDropdown(fieldType, property);

            // Draw the default property field
            EditorGUI.PropertyField(position, property, label, true);
        }

        private static void CreateDropdown(Type type, SerializedProperty property)
        {
            VariableScriptGenerator generator = new VariableScriptGenerator();

            generator.TargetType = type;

            if (generator.ExistingGeneratedClass == null)
            {
                try
                {
                    generator.Generate();
                    EditorUtility.DisplayProgressBar("Generating " + type.Name, "Importing Scripts", 0);
                    AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
                generator = new VariableScriptGenerator();
                EditorUtility.ClearProgressBar();
            }

            // draw dropdown based on new or exisiting variable type using flow engine to get vars

            // The dropdown will use the variable keys and when set it will set the value of the field to value of variable
            // If we choose <none> then the value is null
            // Wheter or not we should replace the field entirely is up to you or if we add the dropdown and have the OG field

            // set value of property based on the type ensuring variable value type matches
        }
    }
}