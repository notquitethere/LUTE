using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MenuChoice))]
public class MenuChoiceEditor : OrderEditor
{
    protected SerializedProperty textProp;
    protected SerializedProperty descriptionProp;
    protected SerializedProperty targetNodeProp;
    protected SerializedProperty hideIfVisitedProp;
    protected SerializedProperty interactableProp;
    protected SerializedProperty setMenuDialogProp;
    protected SerializedProperty hideThisOptionProp;
    protected SerializedProperty closeMenuOnSelectProp;

    public override void OnEnable()
    {
        base.OnEnable();

        textProp = serializedObject.FindProperty("text");
        descriptionProp = serializedObject.FindProperty("description");
        targetNodeProp = serializedObject.FindProperty("targetNode");
        hideIfVisitedProp = serializedObject.FindProperty("hideIfVisited");
        interactableProp = serializedObject.FindProperty("interactable");
        setMenuDialogProp = serializedObject.FindProperty("setMenuDialogue");
        hideThisOptionProp = serializedObject.FindProperty("hideThisOption");
        closeMenuOnSelectProp = serializedObject.FindProperty("closeMenuOnSelect");
    }

    public override void DrawOrderGUI()
    {
        MenuChoice t = target as MenuChoice;
        var engine = (BasicFlowEngine)t.GetEngine();
        if (engine == null)
        {
            return;
        }

        serializedObject.Update();

        EditorGUILayout.PropertyField(textProp);

        EditorGUILayout.PropertyField(descriptionProp);

        EditorGUILayout.BeginHorizontal();
        NodeEditor.NodeField(targetNodeProp,
                               new GUIContent("Target Node", "Node to call when option is selected"),
                               new GUIContent("<None>"),
                               engine);
        const int popupWidth = 17;
        if (targetNodeProp.objectReferenceValue == null && GUILayout.Button("+", GUILayout.MaxWidth(popupWidth)))
        {
            engine.SelectedNode = t.ParentNode;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(hideIfVisitedProp);
        EditorGUILayout.PropertyField(interactableProp);
        EditorGUILayout.PropertyField(setMenuDialogProp);
        EditorGUILayout.PropertyField(hideThisOptionProp);

        var orders = t.ParentNode.OrderList;
        if (orders.Count > 0)
        {
            foreach (Order order in orders)
            {
                if (order is PopupMenu)
                {
                    EditorGUILayout.PropertyField(closeMenuOnSelectProp);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

