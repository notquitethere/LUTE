using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

[CustomEditor(typeof(ToggleXR))]
public class ToggleXREditor : OrderEditor
{
    protected SerializedProperty toggle;

    protected SerializedProperty planeVisualiser;
    protected SerializedProperty planeDetectionMode;
    protected SerializedProperty pointCloudVisualiser;

    public override void OnEnable()
    {
        base.OnEnable();

        toggle = serializedObject.FindProperty("toggle");
        
        planeVisualiser = serializedObject.FindProperty("planeVisualiser");

        //if the planeVisualiser property is null, then set it to the default plane visualiser which is "AR Feathered Plane"
        if (planeVisualiser.objectReferenceValue == null)
        {
            planeVisualiser.objectReferenceValue = Resources.Load("Prefabs/AR Feathered Plane");
        }

        planeDetectionMode = serializedObject.FindProperty("planeDetectionMode");

        //set defaulr plane detection mode to horizontal
        planeDetectionMode.enumValueIndex = 1;

        pointCloudVisualiser = serializedObject.FindProperty("pointCloudVisualiser");

        //if the pointCloudVisualiser property is null, then set it to the default point cloud visualiser which is "AR Point Cloud"
        if (pointCloudVisualiser.objectReferenceValue == null)
        {
            pointCloudVisualiser.objectReferenceValue = Resources.Load("Prefabs/AR Point Cloud Debug Visualizer");
        }
    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        ToggleXR t = target as ToggleXR;
        var engine = (BasicFlowEngine)t.GetEngine();

        EditorGUILayout.PropertyField(toggle);

       //if toggle is on, show everything else
       if (toggle.boolValue)
        {
            EditorGUILayout.PropertyField(planeVisualiser);
            EditorGUILayout.PropertyField(planeDetectionMode);
            EditorGUILayout.PropertyField(pointCloudVisualiser);
        }


        serializedObject.ApplyModifiedProperties();
    }

}
