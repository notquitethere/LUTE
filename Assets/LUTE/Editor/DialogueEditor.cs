using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : OrderEditor
{
    public static bool showTagHelp;
    public static string DrawTagHelpLabel()
    {
        string tagHelpText = "";
        tagHelpText = "" +
                       "\t{b} Bold Text {/b}\n" +
                       "\t{i} Italic Text {/i}\n" +
                       "\t{color=red} Color Text (color){/color}\n" +
                       "\t{size=30} Text size {/size}\n" +
                       "\n" +
                       "\t{s}, {s=60} Writing speed (chars per sec){/s}\n" +
                       "\t{w}, {w=0.5} Wait (seconds)\n" +
                       "\t{wi} Wait for input\n" +
                       "\t{wc} Wait for input and clear\n" +
                       "\t{wvo} Wait for voice over line to complete\n" +
                       "\t{wp}, {wp=0.5} Wait on punctuation (seconds){/wp}\n" +
                       "\t{c} Clear\n" +
                       "\t{x} Exit, advance to the next command without waiting for input\n" +
                       "\n" +
                       "\t{vpunch=10,0.5} Vertically punch screen (intensity,time)\n" +
                       "\t{hpunch=10,0.5} Horizontally punch screen (intensity,time)\n" +
                       "\t{punch=10,0.5} Punch screen (intensity,time)\n" +
                       "\t{flash=0.5} Flash screen (duration)\n" +
                       "\n" +
                       "\t{audio=AudioObjectName} Play Audio Once\n" +
                       "\t{audioloop=AudioObjectName} Play Audio Loop\n" +
                       "\t{audiopause=AudioObjectName} Pause Audio\n" +
                       "\t{audiostop=AudioObjectName} Stop Audio\n" +
                       "\n" +
                       "\t{m=MessageName} Broadcast message\n" +
                       "\t{$VarName} Substitute variable\n" +
                       "\n" +
                       "\t-------- Text Mesh Pro Tags --------\n" +
                       "\t<align=\"right\"> Right </align> <align=\"center\"> Center </align> <align=\"left\"> Left </align>\n" +
                       "\t<color=\"red\"> Red </color> <color=#005500> Dark Green </color>\n" +
                       "\t<alpha=#88> 88 </alpha>\n" +
                       "\t<i> Italic text </i>\n" +
                       "\t<b> Bold text </b>\n" +
                       "\t<cspace=1em> Character spacing </cspace>\n" +
                       "\t<font=\"FontName\"> Change font </font>\n" +
                       "\t<font=\"FontName\" material=\"MaterialName\"> Change font and material </font>\n" +
                       "\t<indent=15%> Indentation </indent>\n" +
                       "\t<line-height=100%> Line height </line-height>\n" +
                       "\t<line-indent=15%> Line indentation </line-indent>\n" +
                       "\t{link=id}link text{/link} <link=id>link text</link>\n" +
                       "\t<lowercase> Lowercase </lowercase>\n" +
                       "\t<uppercase> Uppercase </uppercase>\n" +
                       "\t<smallcaps> Smallcaps </smallcaps>\n" +
                       "\t<margin=5em> Margin </margin>\n" +
                       "\t<mark=#ffff00aa> Mark (Highlight) </mark>\n" +
                       "\t<mspace=2.75em> Monospace </mspace>\n" +
                       "\t<noparse> <b> </noparse>\n" +
                       "\t<nobr> Non-breaking spaces </nobr>\n" +
                       "\t<page> Page break\n" +
                       "\t<size=50%> Font size </size>\n" +
                       "\t<space=5em> Horizontal space\n" +
                       "\t<space=5em> Horizontal space\n" +
                       "\t<sprite=\"AssetName\" index=0> Sprite\n" +
                       "\t<s> Strikethrough </s>\n" +
                       "\t<u> Underline </u>\n" +
                       "\t<style=\"StyleName\"> Styles </style>\n" +
                       "\t<sub> Subscript </sub>\n" +
                       "\t<sup> Superscript </sup>\n" +
                       "\t<voffset=1em> Vertical offset </voffset>\n" +
                       "\t<width=60%> Text width </width>\n" +
                       "\n" +
                       "\n" + // extra space is to fix the vertical sizing the help box at default inspector width
                       "\n";


        float pixelHeight = EditorStyles.miniLabel.CalcHeight(new GUIContent(tagHelpText), EditorGUIUtility.currentViewWidth);
        EditorGUILayout.SelectableLabel(tagHelpText, GUI.skin.GetStyle("HelpBox"), GUILayout.MinHeight(pixelHeight + 50));

        return tagHelpText;
    }

    public Texture2D blackTex;

    protected SerializedProperty characterProp;
    protected SerializedProperty typingSpeedProp;
    protected SerializedProperty portraitProp;
    protected SerializedProperty storyTextProp;
    protected SerializedProperty voiceOverClipProp;
    protected SerializedProperty showAlwaysProp;
    protected SerializedProperty showCountProp;
    protected SerializedProperty extendPreviousProp;
    protected SerializedProperty fadeWhenDoneProp;
    protected SerializedProperty waitForClickProp;
    protected SerializedProperty stopVoiceoverProp;
    protected SerializedProperty waitForVOProp;
    protected SerializedProperty allowClickAnywhereProp;
    protected SerializedProperty useButtonProp;

    public override void OnEnable()
    {
        base.OnEnable();

        characterProp = serializedObject.FindProperty("character");
        portraitProp = serializedObject.FindProperty("characterPortrait");
        storyTextProp = serializedObject.FindProperty("storyText");
        voiceOverClipProp = serializedObject.FindProperty("voiceOverClip");
        showAlwaysProp = serializedObject.FindProperty("showAlways");
        showCountProp = serializedObject.FindProperty("showCount");
        extendPreviousProp = serializedObject.FindProperty("extendPrevious");
        fadeWhenDoneProp = serializedObject.FindProperty("fadeWhenDone");
        waitForClickProp = serializedObject.FindProperty("waitForClick");
        stopVoiceoverProp = serializedObject.FindProperty("stopVoiceover");
        waitForVOProp = serializedObject.FindProperty("waitForVO");
        typingSpeedProp = serializedObject.FindProperty("typingSpeed");
        allowClickAnywhereProp = serializedObject.FindProperty("allowClickAnywhere");
        useButtonProp = serializedObject.FindProperty("useButtonToProgress");

        // if (blackTex == null)
        // {
        //     blackTex = CustomGUI.CreateBlackTexture();
        // }
    }

    protected virtual void OnDisable()
    {
        DestroyImmediate(blackTex);
    }

    public override void DrawOrderGUI()
    {
        serializedObject.Update();

        bool showPortraits = false;
        OrderEditor.ObjectField<Character>(characterProp,
                                            new GUIContent("Character", "Character that is speaking"),
                                            new GUIContent("<None>"),
                                            Character.ActiveCharacters);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(" ");
        characterProp.objectReferenceValue = (Character)EditorGUILayout.ObjectField(characterProp.objectReferenceValue, typeof(Character), true);
        EditorGUILayout.EndHorizontal();

        Dialogue t = target as Dialogue;

        // Only show portrait selection if...
        if (t._Character != null &&              // Character is selected
            t._Character.Portraits != null &&    // Character has a portraits field
            t._Character.Portraits.Count > 0)   // Selected Character has at least 1 portrait
        {
            showPortraits = true;
        }

        if (showPortraits)
        {
            OrderEditor.ObjectField<Sprite>(portraitProp,
                                              new GUIContent("Portrait", "Portrait representing speaking character"),
                                              new GUIContent("<None>"),
                                              t._Character.Portraits);
        }
        else
        {
            if (!t.ExtendPrevious)
            {
                t.Portrait = null;
            }
        }

        EditorGUILayout.PropertyField(storyTextProp);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("Tag Help", "View available rich text tags"), new GUIStyle(EditorStyles.miniButton)))
        {
            showTagHelp = !showTagHelp;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(typingSpeedProp);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(extendPreviousProp);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();


        if (showTagHelp)
        {
            DrawTagHelpLabel();
        }
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(voiceOverClipProp,
                                      new GUIContent("Voice Over Clip", "Voice over audio to play when the text is displayed"));

        EditorGUILayout.PropertyField(showAlwaysProp);

        if (showAlwaysProp.boolValue == false)
        {
            EditorGUILayout.PropertyField(showCountProp);
        }

        GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
        centeredLabel.alignment = TextAnchor.MiddleCenter;
        GUIStyle leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
        leftButton.fontSize = 10;
        leftButton.font = EditorStyles.toolbarButton.font;
        GUIStyle rightButton = new GUIStyle(EditorStyles.miniButtonRight);
        rightButton.fontSize = 10;
        rightButton.font = EditorStyles.toolbarButton.font;

        EditorGUILayout.PropertyField(fadeWhenDoneProp);
        EditorGUILayout.PropertyField(waitForClickProp);
        EditorGUILayout.PropertyField(stopVoiceoverProp);
        EditorGUILayout.PropertyField(waitForVOProp);
        if (useButtonProp.boolValue == false)
            EditorGUILayout.PropertyField(allowClickAnywhereProp);
        if (allowClickAnywhereProp.boolValue == false)
            EditorGUILayout.PropertyField(useButtonProp);

        if (showPortraits && t.Portrait != null)
        {
            Texture2D characterTexture = t.Portrait.texture;
            float aspect = (float)characterTexture.width / (float)characterTexture.height;
            Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), GUILayout.ExpandWidth(true));
            if (characterTexture != null)
            {
                GUI.DrawTexture(previewRect, characterTexture, ScaleMode.ScaleToFit, true, aspect);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}