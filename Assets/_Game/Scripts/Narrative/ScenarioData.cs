using UnityEngine;
using System;
using System.Collections.Generic;

public enum DialogueAdvanceMode
{
    Auto,
    WaitForPlayer
}

public enum SpeakerFocusMode
{
    [InspectorName("Auto (Theo tên người nói)")]
    AutoByName,
    [InspectorName("Nhân vật 1")]
    Character1,
    [InspectorName("Nhân vật 2")]
    Character2,
    [InspectorName("Cả hai")]
    Both,
    [InspectorName("Không ai (Lời dẫn/Mờ cả hai)")]
    None
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
    public SpeakerFocusMode speakerFocus = SpeakerFocusMode.AutoByName;
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
    [Tooltip("Tên file video trong StreamingAssets/Scenario (không cần .mp4). Ưu tiên hơn videoClip")]
    public string videoFile;
    public UnityEngine.Video.VideoClip videoClip; // cũ: không phát được trên WebGL, dùng videoFile
    
    [Header("Character 1 Expression / Pose")]
    public Sprite characterExpression; // Biểu cảm (VD: e1, e2, e3, e4)
    public Sprite characterPose;       // Tư thế (VD: pose 1, pose 2)
    public Sprite characterHair;       // Mái tóc (VD: mái.png)

    [Header("Character 2 Expression / Pose (Optional)")]
    public Sprite character2Expression;
    public Sprite character2Pose;
    public Sprite character2Hair;

    public Sprite choiceBgSprite;       // Ảnh nền tùy chỉnh cho nút lựa chọn này (Optional)

    public TargetType targetType = TargetType.Situation;
    public string targetGuid; // Trỏ tới Scene tiếp theo
    public List<ImpactData> impacts = new List<ImpactData>();

    [Header("Vật phẩm thưởng (Optional)")]
    [Tooltip("Lựa chọn tốt có thể thưởng 1 vật phẩm dùng trong trận cùng ngày")]
    public LifestyleItem rewardItem = LifestyleItem.None;

    [Header("Kiến thức y khoa (Optional)")]
    [Tooltip("Thẻ kiến thức hiện ra sau khi chọn, giải thích tác động của lựa chọn lên sức khỏe")]
    [TextArea(2, 6)]
    public string knowledgeTip;
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
    [Tooltip("Tên file video trong StreamingAssets/Scenario (không cần .mp4). Ưu tiên hơn dialogueVideo")]
    public string dialogueVideoFile;
    public UnityEngine.Video.VideoClip dialogueVideo; // cũ: không phát được trên WebGL, dùng dialogueVideoFile
    public bool loopDialogueVideo = false;
    public UnityEngine.Video.VideoClip choiceVideo;
    public bool loopChoiceVideo = true;

    [Header("Character 1 (Default / Slot 1)")]
    public string character1Name;      // Tên nhận diện NV1 (để Auto-focus khi nói)
    public Sprite characterPose;       // Tư thế nhân vật 1
    public Sprite characterExpression; // Biểu cảm khuôn mặt nhân vật 1
    public Sprite characterHair;       // Mái tóc nhân vật 1

    [Header("Character 2 (Optional / Slot 2)")]
    public bool hasSecondCharacter = false; // Tình huống này có 2 nhân vật hay không
    public string character2Name;      // Tên nhận diện NV2 (để Auto-focus khi nói)
    public Sprite character2Pose;      // Tư thế nhân vật 2
    public Sprite character2Expression;// Biểu cảm khuôn mặt nhân vật 2
    public Sprite character2Hair;      // Mái tóc nhân vật 2

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
