using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ScenarioData))]
public class ScenarioEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ScenarioData data = (ScenarioData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🔧 Scenario Editor Tool", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Quản lý các tình huống, câu hỏi và tác động của nó trong ngày.", MessageType.Info);
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Open Scenario Graph Editor", GUILayout.Height(40)))
        {
            ScenarioGraphWindow.OpenGraphWindow(data);
        }
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate/Migrate Scenario GUIDs", GUILayout.Height(30)))
        {
            MigrateGUIDs(data);
        }
        if (GUILayout.Button("Migrate Legacy Dialogues", GUILayout.Height(30)))
        {
            MigrateLegacyDialogues(data);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("entryGuid"), new GUIContent("Entry GUID (Read Only)"));
        EditorGUILayout.Space();

        SerializedProperty situationsProp = serializedObject.FindProperty("situations");

        for (int i = 0; i < situationsProp.arraySize; i++)
        {
            SerializedProperty sitProp = situationsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            string sitId = sitProp.FindPropertyRelative("situationId").stringValue;
            string timeStr = sitProp.FindPropertyRelative("timeText").stringValue;
            string headerText = !string.IsNullOrEmpty(sitId)
                ? $"Situation {i + 1}: {sitId}" + (!string.IsNullOrEmpty(timeStr) ? $" [{timeStr}]" : "")
                : $"Situation {i + 1}: {timeStr}";

            sitProp.isExpanded = EditorGUILayout.Foldout(sitProp.isExpanded, headerText, true);

            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("▲", GUILayout.Width(24)))
            {
                situationsProp.MoveArrayElement(i, i - 1);
                break;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(i == situationsProp.arraySize - 1);
            if (GUILayout.Button("▼", GUILayout.Width(24)))
            {
                situationsProp.MoveArrayElement(i, i + 1);
                break;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                situationsProp.DeleteArrayElementAtIndex(i);
                break; // Break to avoid index out of bounds
            }
            EditorGUILayout.EndHorizontal();

            if (sitProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("guid"), new GUIContent("GUID"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("situationId"), new GUIContent("Situation ID"));
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("timeText"), new GUIContent("Time Text (e.g. 07:30)"));
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("speakerName"), new GUIContent("Default Speaker Name (Optional)"));

                EditorGUILayout.LabelField("Background / Media", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                
                SerializedProperty mediaTypeProp = sitProp.FindPropertyRelative("mediaType");
                EditorGUILayout.PropertyField(mediaTypeProp, new GUIContent("Media Type"));

                if (mediaTypeProp.enumValueIndex == (int)BackgroundMediaType.Image)
                {
                    EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("illustration"), new GUIContent("Illustration Image"));
                }
                else
                {
                    EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("dialogueVideo"), new GUIContent("Dialogue Video"));
                    if (sitProp.FindPropertyRelative("dialogueVideo").objectReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("loopDialogueVideo"), new GUIContent("Loop Dialogue Video"));
                    }
                    EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("choiceVideo"), new GUIContent("Choice Video"));
                    if (sitProp.FindPropertyRelative("choiceVideo").objectReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("loopChoiceVideo"), new GUIContent("Loop Choice Video"));
                    }
                }
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("descriptionBgSprite"), new GUIContent("Description Background (Optional)"));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Character Sprites (Situation Default)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("characterPose"), new GUIContent("Pose (Body)"));
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("characterExpression"), new GUIContent("Expression (Face/Eyes)"));
                EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("characterHair"), new GUIContent("Hair (Optional)"));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Dialogues", EditorStyles.boldLabel);
                
                SerializedProperty dialogueLinesProp = sitProp.FindPropertyRelative("dialogueLines");
                for (int d = 0; d < dialogueLinesProp.arraySize; d++)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    SerializedProperty lineProp = dialogueLinesProp.GetArrayElementAtIndex(d);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Line {d + 1}", EditorStyles.boldLabel);

                    EditorGUI.BeginDisabledGroup(d == 0);
                    if (GUILayout.Button("▲", GUILayout.Width(24)))
                    {
                        dialogueLinesProp.MoveArrayElement(d, d - 1);
                        break;
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(d == dialogueLinesProp.arraySize - 1);
                    if (GUILayout.Button("▼", GUILayout.Width(24)))
                    {
                        dialogueLinesProp.MoveArrayElement(d, d + 1);
                        break;
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("X", GUILayout.Width(22)))
                    {
                        dialogueLinesProp.DeleteArrayElementAtIndex(d);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(lineProp.FindPropertyRelative("speakerName"), new GUIContent("Speaker Name (Who is speaking)"));
                    EditorGUILayout.PropertyField(lineProp.FindPropertyRelative("text"), new GUIContent("Text"));
                    
                    SerializedProperty advanceModeProp = lineProp.FindPropertyRelative("advanceMode");
                    EditorGUILayout.PropertyField(advanceModeProp, new GUIContent("Advance Mode"));
                    
                    if (advanceModeProp.enumValueIndex == (int)DialogueAdvanceMode.Auto)
                    {
                        EditorGUILayout.PropertyField(lineProp.FindPropertyRelative("displayDuration"), new GUIContent("Display Duration"));
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                
                if (GUILayout.Button("+ Add Dialogue Line"))
                {
                    dialogueLinesProp.arraySize++;
                }

                EditorGUILayout.Space();
                
                SerializedProperty choicesProp = sitProp.FindPropertyRelative("choices");
                if (choicesProp.arraySize == 0)
                {
                    EditorGUILayout.LabelField("Auto Transition (0 Choices)", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("autoTransitionDelay"), new GUIContent("Delay (s)"));
                    EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("autoTargetType"), new GUIContent("Transition Type"));
                    
                    if (sitProp.FindPropertyRelative("autoTargetType").enumValueIndex == (int)TargetType.Situation)
                    {
                        EditorGUILayout.PropertyField(sitProp.FindPropertyRelative("autoTargetGuid"), new GUIContent("Target GUID"));
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
                    for (int j = 0; j < choicesProp.arraySize; j++)
                    {
                        SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(j);
                        
                        EditorGUILayout.BeginVertical("helpbox");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("choiceText"), new GUIContent($"Choice {j + 1}"));

                        EditorGUI.BeginDisabledGroup(j == 0);
                        if (GUILayout.Button("▲", GUILayout.Width(24)))
                        {
                            choicesProp.MoveArrayElement(j, j - 1);
                            break;
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(j == choicesProp.arraySize - 1);
                        if (GUILayout.Button("▼", GUILayout.Width(24)))
                        {
                            choicesProp.MoveArrayElement(j, j + 1);
                            break;
                        }
                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("X", GUILayout.Width(22)))
                        {
                            choicesProp.DeleteArrayElementAtIndex(j);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("delayAfterChoice"), new GUIContent("Animation Delay (s)"));
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("videoClip"), new GUIContent("Video (Optional)"));
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("choiceBgSprite"), new GUIContent("Choice Background (Optional)"));
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("targetType"), new GUIContent("Target Type"));
                        
                        if (choiceProp.FindPropertyRelative("targetType").enumValueIndex == (int)TargetType.Situation)
                        {
                            EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("targetGuid"), new GUIContent("Target GUID"));
                        }

                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("Character Expression On Choice", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("characterPose"), new GUIContent("Choice Pose"));
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("characterExpression"), new GUIContent("Choice Expression"));
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("characterHair"), new GUIContent("Choice Hair (Optional)"));

                        SerializedProperty impactsProp = choiceProp.FindPropertyRelative("impacts");
                        for (int k = 0; k < impactsProp.arraySize; k++)
                        {
                            SerializedProperty impactProp = impactsProp.GetArrayElementAtIndex(k);
                            EditorGUILayout.BeginHorizontal();
                            
                            SerializedProperty targetStatProp = impactProp.FindPropertyRelative("targetStat");
                            StatImpactType currentType = (StatImpactType)Mathf.Clamp(targetStatProp.enumValueIndex, 0, System.Enum.GetValues(typeof(StatImpactType)).Length - 1);
                            StatImpactType newType = (StatImpactType)EditorGUILayout.EnumPopup(currentType, GUILayout.Width(180));
                            targetStatProp.enumValueIndex = (int)newType;

                            EditorGUILayout.PropertyField(impactProp.FindPropertyRelative("value"), GUIContent.none);
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                impactsProp.DeleteArrayElementAtIndex(k);
                                break;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        if (GUILayout.Button("+ Add Impact", GUILayout.Width(100)))
                        {
                            impactsProp.arraySize++;
                        }
                        
                        EditorGUILayout.EndVertical();
                    }
                }

                if (GUILayout.Button("+ Add Choice (A, B, C...)", GUILayout.Height(25)))
                {
                    choicesProp.arraySize++;
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("+ Add New Situation", GUILayout.Height(30)))
        {
            situationsProp.arraySize++;
            SerializedProperty newSit = situationsProp.GetArrayElementAtIndex(situationsProp.arraySize - 1);
            
            // Xoá dữ liệu rác do Unity copy từ phần tử cuối cùng
            newSit.FindPropertyRelative("guid").stringValue = System.Guid.NewGuid().ToString();
            newSit.FindPropertyRelative("situationId").stringValue = "Situation_" + (situationsProp.arraySize - 1);
            newSit.FindPropertyRelative("timeText").stringValue = "00:00";
            newSit.FindPropertyRelative("description").stringValue = "";
            newSit.FindPropertyRelative("dialogues").arraySize = 0;
            newSit.FindPropertyRelative("dialogueLines").arraySize = 0;
            newSit.FindPropertyRelative("hasMigratedDialogues").boolValue = true;
            newSit.FindPropertyRelative("mediaType").enumValueIndex = (int)BackgroundMediaType.Video;
            newSit.FindPropertyRelative("illustration").objectReferenceValue = null;
            newSit.FindPropertyRelative("dialogueVideo").objectReferenceValue = null;
            newSit.FindPropertyRelative("loopDialogueVideo").boolValue = false;
            newSit.FindPropertyRelative("choiceVideo").objectReferenceValue = null;
            newSit.FindPropertyRelative("loopChoiceVideo").boolValue = true;
            newSit.FindPropertyRelative("characterPose").objectReferenceValue = null;
            newSit.FindPropertyRelative("characterExpression").objectReferenceValue = null;
            newSit.FindPropertyRelative("characterHair").objectReferenceValue = null;
            newSit.FindPropertyRelative("autoTargetGuid").stringValue = "";
            newSit.FindPropertyRelative("autoTargetType").enumValueIndex = (int)TargetType.Situation;
            newSit.FindPropertyRelative("autoTransitionDelay").floatValue = 3.0f;
            newSit.FindPropertyRelative("choices").arraySize = 0;
            newSit.FindPropertyRelative("editorPosition").vector2Value = Vector2.zero;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void MigrateGUIDs(ScenarioData data)
    {
        bool changed = false;
        var seenGuids = new HashSet<string>();

        foreach (var sit in data.situations)
        {
            if (string.IsNullOrEmpty(sit.guid) || seenGuids.Contains(sit.guid))
            {
                sit.guid = System.Guid.NewGuid().ToString();
                changed = true;
            }
            seenGuids.Add(sit.guid);

            if (string.IsNullOrEmpty(sit.situationId))
            {
                sit.situationId = "Situation_" + data.situations.IndexOf(sit);
                changed = true;
            }
        }
        
        if (data.situations.Count > 0 && string.IsNullOrEmpty(data.entryGuid))
        {
            data.entryGuid = data.situations[0].guid;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log("Scenario GUIDs migrated successfully.");
        }
        else
        {
            Debug.Log("No missing GUIDs found. Migration skipped.");
        }
    }

    private void MigrateLegacyDialogues(ScenarioData data)
    {
        bool changed = false;
        Undo.RecordObject(data, "Migrate Dialogues");

        foreach (var sit in data.situations)
        {
            if (!sit.hasMigratedDialogues)
            {
                if (sit.dialogueLines == null) sit.dialogueLines = new List<DialogueLineData>();
                
                if (sit.dialogues != null && sit.dialogues.Count > 0)
                {
                    foreach(var s in sit.dialogues)
                    {
                        sit.dialogueLines.Add(new DialogueLineData { text = s, advanceMode = DialogueAdvanceMode.Auto, displayDuration = 3f });
                    }
                    sit.dialogues.Clear();
                }
                else if (!string.IsNullOrEmpty(sit.description))
                {
                    sit.dialogueLines.Add(new DialogueLineData { text = sit.description, advanceMode = DialogueAdvanceMode.Auto, displayDuration = 3f });
                    sit.description = "";
                }

                sit.hasMigratedDialogues = true;
                changed = true;
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log("Legacy Dialogues migrated successfully.");
        }
        else
        {
            Debug.Log("No dialogues to migrate. Migration skipped.");
        }
    }
}
