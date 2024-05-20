using UnityEditor;
using MoreMountains.InventoryEngine;
using System.Linq;

[CustomEditor(typeof(UsableItem))]

public class UsableItemEditor : OrderEditor
{
    protected SerializedProperty feedbackProp;
    protected SerializedProperty showCardProp;
    protected SerializedProperty locationProp;
    protected SerializedProperty itemProp;

    protected int locationVarIndex = 0;
    protected int itemIndex = 0;

    public override void OnEnable()
    {
        base.OnEnable();
        feedbackProp = serializedObject.FindProperty("useFeedback");
        showCardProp = serializedObject.FindProperty("showCard");
        locationProp = serializedObject.FindProperty("itemLocation");
        itemProp = serializedObject.FindProperty("item");
    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        UsableItem t = target as UsableItem;
        var engine = (BasicFlowEngine)t.GetEngine();

        EditorGUILayout.PropertyField(feedbackProp);
        EditorGUILayout.PropertyField(showCardProp);

        var locationVars = engine.GetComponents<LocationVariable>();
        for (int i = 0; i < locationVars.Length; i++)
        {
            if (locationVars[i] == locationProp.objectReferenceValue as LocationVariable)
            {
                locationVarIndex = i;
            }
        }

        locationVarIndex = EditorGUILayout.Popup("Location", locationVarIndex, locationVars.Select(x => x.Key).ToArray());
        if (locationVars.Length > 0)
            locationProp.objectReferenceValue = locationVars[locationVarIndex];

        var items = ContainerCardEditor.GetAllInstances<InventoryItem>();
        for (int j = 0; j < items.Length; j++)
        {
            if (items[j] == itemProp.objectReferenceValue as InventoryItem)
            {
                itemIndex = j;
            }
        }

        itemIndex = EditorGUILayout.Popup("Item", itemIndex, items.Where(x => x.IsUsable).Select(x => x.ItemName).ToArray());
        itemProp.objectReferenceValue = items[itemIndex];

        serializedObject.ApplyModifiedProperties();
    }
}