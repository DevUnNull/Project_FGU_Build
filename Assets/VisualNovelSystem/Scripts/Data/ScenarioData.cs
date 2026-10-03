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
    public string speakerName;
    [TextArea(2, 5)]
    public string text;
    public DialogueAdvanceMode advanceMode = DialogueAdvanceMode.Auto;
    [Min(0f)]
    public float displayDuration = 3f;
}

public enum StatImpactType
{
    [InspectorName("Immunity (Kháng thể)")]
    Immunity,
    [InspectorName("Energy (Thể lực)")]
    Energy,
    [InspectorName("Toxicity (Độc tố)")]
    Toxicity,
    [InspectorName("Hydration (Nước)")]
    Hydration,
    [InspectorName("Recovery (Hồi phục)")]
    Recovery
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
    
    [Header("Character Expression / Pose")]
    public Sprite characterExpression; // Biểu cảm (VD: e1, e2, e3, e4)
    public Sprite characterPose;       // Tư thế (VD: pose 1, pose 2)
    public Sprite characterHair;       // Mái tóc (VD: mái.png)

    public Sprite choiceBgSprite;       // Ảnh nền tùy chỉnh cho nút lựa chọn này (Optional)

    public TargetType targetType = TargetType.Situation;
    public string targetGuid; // Trỏ tới Scene tiếp theo
    public List<ImpactData> impacts = new List<ImpactData>();
}

public enum BackgroundMediaType
{
    Image,
    Video
}

[System.Serializable]
public class SituationData
{
    public string guid; // Mã duy nhất
    public string situationId; // Tên hiển thị (VD: "S01")
    public Vector2 editorPosition; // Dành cho GraphView sau này
    
    public string timeText;
    public string speakerName; // Tên nhân vật xưng danh mặc định cho tình huống (Optional)
    
    [Header("Background / Media")]
    public BackgroundMediaType mediaType = BackgroundMediaType.Video;
    public Sprite illustration;
    public Sprite descriptionBgSprite; // Ảnh nền tùy chỉnh cho khung thoại Description (Optional)
    public UnityEngine.Video.VideoClip dialogueVideo;
    public bool loopDialogueVideo = false;
    public UnityEngine.Video.VideoClip choiceVideo;
    public bool loopChoiceVideo = true;

    [Header("Character Default Sprites")]
    public Sprite characterExpression; // Biểu cảm khuôn mặt
    public Sprite characterPose;       // Tư thế nhân vật
    public Sprite characterHair;       // Mái tóc

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
