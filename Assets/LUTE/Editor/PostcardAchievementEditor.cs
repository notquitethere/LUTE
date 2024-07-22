using UnityEditor;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [CustomPropertyDrawer(typeof(PostcardAchievement))]
    public class PostcardAchievementEditor : PropertyDrawer
    {
        private const string ExcludedPropertyName = "triggerNode";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;
            float yOffset = 0;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (ShouldSkipProperty(iterator))
                {
                    // Then, find the BasicFlowEngine in the scene
                    BasicFlowEngine engine = UnityEngine.Object.FindObjectOfType<BasicFlowEngine>();
                    if (engine != null)
                    {
                        // Find all the nodes on the engine
                        var nodes = engine.GetComponents<Node>();
                        // Create a dropdown with all the nodes
                        string[] nodeNames = new string[nodes.Length];
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            nodeNames[i] = nodes[i]._NodeName;
                        }
                        int index = 0;
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            if (nodes[i]._NodeName == property.FindPropertyRelative("triggerNode").stringValue)
                            {
                                index = i;
                                break;
                            }
                        }
                        index = EditorGUI.Popup(new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight), "Trigger Node", index, nodeNames);
                        property.FindPropertyRelative("triggerNode").stringValue = nodes[index]._NodeName;
                        yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    continue;
                }

                float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                Rect propertyRect = new Rect(position.x, position.y + yOffset, position.width, propertyHeight);

                EditorGUI.PropertyField(propertyRect, iterator, true);
                yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.EndProperty();
        }

        private bool ShouldSkipProperty(SerializedProperty property)
        {
            // Skip based on name
            if (property.name == ExcludedPropertyName)
            {
                return true;
            }

            return false;
        }

        private void DrawNodeDrawer()
        {

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight * 5; // Start with the height of a single line (with room for the foldout)

            SerializedProperty iterator = property.Copy();
            bool hasNext = iterator.NextVisible(true); // Move to the first visible property

            while (hasNext)
            {
                totalHeight += EditorGUI.GetPropertyHeight(iterator, GUIContent.none, true); // Add the height of each property
                hasNext = iterator.NextVisible(false); // Move to the next visible property
            }

            return totalHeight;
        }
    }
}
