using System.Linq;
using MoreMountains.Tools;
using UnityEditor;

[CustomEditor(typeof(Achievement))]
public class AchievementEditor : OrderEditor
{
    protected SerializedProperty achievementProp;
    protected SerializedProperty progressProp;
    protected SerializedProperty amountProp;

    protected int achievementIndex = 0;

    public override void OnEnable()
    {
        base.OnEnable();
        achievementProp = serializedObject.FindProperty("achievementID");
        progressProp = serializedObject.FindProperty("progress");
        amountProp = serializedObject.FindProperty("amount");
    }

    public override void OnInspectorGUI()
    {
        DrawOrderGUI();
    }

    public override void DrawOrderGUI()
    {
        Achievement t = target as Achievement;
        var engine = (BasicFlowEngine)t.GetEngine();

        var achievements = engine.GetComponentInChildren<AchievementRules>().AchievementList.Achievements;

        if (achievements == null)
        {
            return;
        }

        if(achievements.Count == 0)
        {
            return;
        }

        for (int i = 0; i < achievements.Count; i++)
        {
            if (achievements[i].AchievementID == achievementProp.stringValue)
            {
                achievementIndex = i;
            }
        }

        achievementIndex = EditorGUILayout.Popup("Achievement", achievementIndex, achievements.Select(x => x.AchievementID).ToArray());
        achievementProp.stringValue = achievements[achievementIndex].AchievementID;

        EditorGUILayout.PropertyField(progressProp);
        EditorGUILayout.PropertyField(amountProp);

        serializedObject.ApplyModifiedProperties();
    }
}
