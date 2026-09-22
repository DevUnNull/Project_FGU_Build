using UnityEngine;
using System;
using System.Collections.Generic;

public enum DialogueAdvanceMode
{
    Auto,
    WaitForPlayer
}

[System.Serializable]
public class DialogueLineData
{
    [TextArea(2, 5)]
    public string text;
    public DialogueAdvanceMode advanceMode = DialogueAdvanceMode.Auto;
    [Min(0f)]
    public float displayDuration = 3f;
}

public enum StatImpactType
{
    AtpRecovery,
    MucosaHp,
    CellDamage,
    CellAttackSpeed,
    GastricAcid,
    ToxinObstacles
}

public enum TargetType
{
    Situation,
    EndScene
}

[System.Serializable]
public class ImpactData
{
    public StatImpactType targetStat;
    public float value;
}

[System.Serializable]
public class ChoiceData
{
    public string choiceText;
    public float delayAfterChoice = 2.0f;
    public UnityEngine.Video.VideoClip videoClip;
    public TargetType targetType = TargetType.Situation;
    public string targetGuid; // Trỏ tới Scene tiếp theo
    public List<ImpactData> impacts = new List<ImpactData>();
}

[System.Serializable]
public class SituationData
{
    public string guid; // Mã duy nhất
    public string situationId; // Tên hiển thị (VD: "S01")
    public Vector2 editorPosition; // Dành cho GraphView sau này
    
    public string timeText;
    
    [Header("Background / Media")]
    public Sprite illustration;
    public UnityEngine.Video.VideoClip dialogueVideo;
    public bool loopDialogueVideo = false;
    public UnityEngine.Video.VideoClip choiceVideo;
    public bool loopChoiceVideo = true;

    [Header("Dialogues")]
    [HideInInspector]
    public string description; // Keeping for backward compatibility / migration
    [HideInInspector]
    public List<string> dialogues = new List<string>(); // Legacy
    
    public bool hasMigratedDialogues = false;
    public List<DialogueLineData> dialogueLines = new List<DialogueLineData>();
    
    [Header("Auto Transition (0 Choices)")]
    public float autoTransitionDelay = 3.0f;
    public TargetType autoTargetType = TargetType.Situation;
    public string autoTargetGuid;

    [Header("Branching")]
    public List<ChoiceData> choices = new List<ChoiceData>();
}

[CreateAssetMenu(fileName = "NewScenario", menuName = "Visual Novels/Scenario Data")]
public class ScenarioData : ScriptableObject
{
    public string entryGuid; // Node bắt đầu
    public List<SituationData> situations = new List<SituationData>();
}
