using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ScenarioPlayer : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Kéo file ScenarioData (ví dụ: Day1_v1) chứa dữ liệu kịch bản vào đây.")]
    public ScenarioData currentScenario;

    [Header("UI References")]
    [Tooltip("Kéo Component TextMeshProUGUI hiển thị thời gian (ví dụ: TimeText) vào đây.")]
    public TextMeshProUGUI timeTextUI;

    [Tooltip("Kéo Component TextMeshProUGUI hiển thị câu thoại / mô tả tình huống (ví dụ: DescText) vào đây.")]
    public TextMeshProUGUI descriptionTextUI;

    [Tooltip("Kéo Component TextMeshProUGUI hiển thị tên người nói (ô màu hồng ở trên khung thoại) vào đây.")]
    public TextMeshProUGUI speakerNameTextUI;

    [Tooltip("Kéo GameObject Khung thoại Description (chứa Image nền và Text inside) vào đây.")]
    public GameObject descriptionPanel;

    [Tooltip("Kéo Component Image làm khung nền đệm phía sau đoạn chat/lời thoại vào đây.")]
    public Image descriptionBgImage;

    [Tooltip("Kéo Sprite khung nền mặc định cho lời thoại vào đây (nếu tình huống không cài ảnh nền riêng sẽ dùng ảnh này).")]
    public Sprite defaultDescriptionBgSprite;

    [Tooltip("Kéo Sprite khung nền mặc định cho các nút lựa chọn vào đây (nếu nút lựa chọn không cài ảnh nền riêng sẽ dùng ảnh này).")]
    public Sprite defaultChoiceBgSprite;

    [Tooltip("Kéo Component Image hiển thị ảnh minh họa nền tĩnh (IllustrationImage) vào đây.")]
    public Image illustrationImage;

    [Tooltip("Kéo Component VideoPlayer phát video nền (Background2) vào đây.")]
    public UnityEngine.Video.VideoPlayer bgVideoPlayer;

    [Header("Character 1 UI References")]
    [Tooltip("Kéo GameObject cha chứa nhân vật 1 (ví dụ Charter) vào đây.")]
    public GameObject character1Root;

    [Tooltip("Kéo Component Image hiển thị tư thế/thân người nhân vật 1 (MainCharter) vào đây.")]
    public Image characterPoseImage;

    [Tooltip("Kéo Component Image hiển thị biểu cảm khuôn mặt nhân vật 1 (Expention) vào đây.")]
    public Image characterExpressionImage;

    [Tooltip("Kéo Component Image hiển thị kiểu tóc nhân vật 1 (nếu có) vào đây.")]
    public Image characterHairImage;

    [Header("Character 2 UI References")]
    [Tooltip("Kéo GameObject cha chứa nhân vật 2 (ví dụ Charter_Left / Charter_2) vào đây (tự động ẩn khi cảnh chỉ có 1 nhân vật).")]
    public GameObject character2Root;

    [Tooltip("Kéo Component Image hiển thị tư thế/thân người nhân vật 2 vào đây.")]
    public Image character2PoseImage;

    [Tooltip("Kéo Component Image hiển thị biểu cảm khuôn mặt nhân vật 2 vào đây.")]
    public Image character2ExpressionImage;

    [Tooltip("Kéo Component Image hiển thị kiểu tóc nhân vật 2 (nếu có) vào đây.")]
    public Image character2HairImage;

    [Header("Character Highlight & Dimming")]
    [Tooltip("Màu của nhân vật khi đang nói (Sáng rõ)")]
    public Color activeSpeakerColor = Color.white;

    [Tooltip("Màu của nhân vật khi không nói (Tối đi, ví dụ xám 45%)")]
    public Color inactiveSpeakerColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Tooltip("Tốc độ chuyển đổi màu sáng/tối")]
    public float dimTransitionSpeed = 8f;

    [Tooltip("Kéo Object/RectTransform cha chứa danh sách các nút lựa chọn (ChoicesContainer có Horizontal Layout Group) vào đây.")]
    public Transform choicesContainer; // Nơi chứa các nút (Dùng Horizontal Layout Group)

    [Tooltip("Kéo Prefab nút bấm lựa chọn (ChoiceButton) vào đây để sinh các câu trả lời khi chơi.")]
    public GameObject choiceButtonPrefab; // Prefab của một nút chọn

    [Header("Stat Impact Popup Settings")]
    [Tooltip("Kéo StatImpactPopupPrefab vào đây để khi chọn phương án sẽ bay popup từ đầu nhân vật.")]
    public GameObject statImpactPopupPrefab;

    [Tooltip("Vị trí gốc trên đầu nhân vật để bay popup (nếu trống sẽ tự lấy vị trí của CharacterPoseImage/Canvas).")]
    public Transform popupSpawnTransform;

    [Header("Stat Icons & Colors")]
    public Sprite immunityIcon;
    public Sprite energyIcon;
    public Sprite toxicityIcon;
    public Sprite hydrationIcon;
    public Sprite recoveryIcon;

    public Color positiveStatColor = new Color(0.2f, 1f, 0.3f, 1f); // Xanh lá
    public Color negativeStatColor = new Color(1f, 0.25f, 0.25f, 1f); // Đỏ

    public Action<StatImpactType, float> onChoiceImpact;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (statImpactPopupPrefab == null)
        {
            statImpactPopupPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Narrative/StatImpactPopupPrefab.prefab");
        }
        if (character1Root == null)
        {
            Transform charter = GameObject.Find("Charter")?.transform ?? GameObject.Find("MainCharter")?.transform;
            if (charter != null) character1Root = charter.gameObject;
        }
        if (character2Root == null)
        {
            Transform charter2 = GameObject.Find("Charter_Left")?.transform ?? GameObject.Find("Charter_2")?.transform;
            if (charter2 != null) character2Root = charter2.gameObject;
        }
        if (popupSpawnTransform == null)
        {
            Transform charter = GameObject.Find("MainCharter")?.transform ?? GameObject.Find("Charter")?.transform;
            if (charter != null) popupSpawnTransform = charter;
        }
        if (immunityIcon == null)
            immunityIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Armour/Shield.png");
        if (energyIcon == null)
            energyIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Bolt/Bolt - Yellow.png");
        if (toxicityIcon == null)
            toxicityIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Skull/Skull.png");
        if (hydrationIcon == null)
            hydrationIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Potion/Potion - Blue.png");
        if (recoveryIcon == null)
            recoveryIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Heart - Health/Heart - Red.png");
    }
#endif

    private int currentSituationIndex = 0;
    private List<GameObject> activeButtons = new List<GameObject>();
    private Dictionary<string, SituationData> situationLookup = new Dictionary<string, SituationData>();
    private Coroutine playSituationRoutine;
    private Coroutine videoTransitionRoutine;
    private Coroutine dimmingRoutine;

    private UnityEngine.Video.VideoPlayer activeVideoPlayer;
    private UnityEngine.Video.VideoPlayer standbyVideoPlayer;

    private bool advanceConsumedThisFrame = false;

    private void UpdateCharacter1Display(Sprite pose, Sprite expression, Sprite hair)
    {
        if (characterPoseImage != null && pose != null)
        {
            characterPoseImage.sprite = pose;
            characterPoseImage.gameObject.SetActive(true);
        }

        if (characterExpressionImage != null && expression != null)
        {
            characterExpressionImage.sprite = expression;
            characterExpressionImage.gameObject.SetActive(true);
        }

        if (characterHairImage != null && hair != null)
        {
            characterHairImage.sprite = hair;
            characterHairImage.gameObject.SetActive(true);
        }
    }

    private void UpdateCharacter2Display(Sprite pose, Sprite expression, Sprite hair)
    {
        if (character2PoseImage != null && pose != null)
        {
            character2PoseImage.sprite = pose;
            character2PoseImage.gameObject.SetActive(true);
        }

        if (character2ExpressionImage != null && expression != null)
        {
            character2ExpressionImage.sprite = expression;
            character2ExpressionImage.gameObject.SetActive(true);
        }

        if (character2HairImage != null && hair != null)
        {
            character2HairImage.sprite = hair;
            character2HairImage.gameObject.SetActive(true);
        }
    }

    private void UpdateCharacterDisplay(Sprite pose, Sprite expression, Sprite hair)
    {
        UpdateCharacter1Display(pose, expression, hair);
    }

    private void SetCharacter1ColorDirect(Color c)
    {
        if (characterPoseImage != null) characterPoseImage.color = c;
        if (characterExpressionImage != null) characterExpressionImage.color = c;
        if (characterHairImage != null) characterHairImage.color = c;
    }

    private void SetCharacter2ColorDirect(Color c)
    {
        if (character2PoseImage != null) character2PoseImage.color = c;
        if (character2ExpressionImage != null) character2ExpressionImage.color = c;
        if (character2HairImage != null) character2HairImage.color = c;
    }

    public void ApplySpeakerHighlight(SituationData sit, DialogueLineData line, bool immediate = false)
    {
        bool has2 = sit != null && sit.hasSecondCharacter;

        // Nếu chỉ có 1 nhân vật: NV1 luôn sáng (activeSpeakerColor), NV2 tự động ẩn
        if (!has2)
        {
            if (character2Root != null) character2Root.SetActive(false);
            if (character1Root != null) character1Root.SetActive(true);

            if (dimmingRoutine != null) StopCoroutine(dimmingRoutine);
            SetCharacter1ColorDirect(activeSpeakerColor);
            return;
        }

        // Nếu có 2 nhân vật: Bật cả 2 root
        if (character1Root != null) character1Root.SetActive(true);
        if (character2Root != null) character2Root.SetActive(true);

        Color targetColor1 = activeSpeakerColor;
        Color targetColor2 = activeSpeakerColor;

        string currentSpeaker = line != null ? (!string.IsNullOrEmpty(line.speakerName) ? line.speakerName : sit.speakerName) : sit.speakerName;
        SpeakerFocusMode focus = line != null ? line.speakerFocus : SpeakerFocusMode.AutoByName;

        switch (focus)
        {
            case SpeakerFocusMode.Character1:
                targetColor1 = activeSpeakerColor;
                targetColor2 = inactiveSpeakerColor;
                break;

            case SpeakerFocusMode.Character2:
                targetColor1 = inactiveSpeakerColor;
                targetColor2 = activeSpeakerColor;
                break;

            case SpeakerFocusMode.Both:
                targetColor1 = activeSpeakerColor;
                targetColor2 = activeSpeakerColor;
                break;

            case SpeakerFocusMode.None:
                targetColor1 = inactiveSpeakerColor;
                targetColor2 = inactiveSpeakerColor;
                break;

            case SpeakerFocusMode.AutoByName:
            default:
                if (!string.IsNullOrEmpty(currentSpeaker))
                {
                    bool isChar1 = !string.IsNullOrEmpty(sit.character1Name) && currentSpeaker.Trim().Equals(sit.character1Name.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool isChar2 = !string.IsNullOrEmpty(sit.character2Name) && currentSpeaker.Trim().Equals(sit.character2Name.Trim(), StringComparison.OrdinalIgnoreCase);

                    if (isChar1 && !isChar2)
                    {
                        targetColor1 = activeSpeakerColor;
                        targetColor2 = inactiveSpeakerColor;
                    }
                    else if (isChar2 && !isChar1)
                    {
                        targetColor1 = inactiveSpeakerColor;
                        targetColor2 = activeSpeakerColor;
                    }
                    else
                    {
                        // Nếu không khớp riêng biệt ai, cả 2 cùng sáng
                        targetColor1 = activeSpeakerColor;
                        targetColor2 = activeSpeakerColor;
                    }
                }
                else
                {
                    // Lời dẫn không có tên người nói
                    targetColor1 = activeSpeakerColor;
                    targetColor2 = activeSpeakerColor;
                }
                break;
        }

        if (immediate)
        {
            if (dimmingRoutine != null) StopCoroutine(dimmingRoutine);
            SetCharacter1ColorDirect(targetColor1);
            SetCharacter2ColorDirect(targetColor2);
        }
        else
        {
            if (dimmingRoutine != null) StopCoroutine(dimmingRoutine);
            dimmingRoutine = StartCoroutine(TransitionSpeakerColorsRoutine(targetColor1, targetColor2));
        }
    }

    private System.Collections.IEnumerator TransitionSpeakerColorsRoutine(Color target1, Color target2)
    {
        Color start1 = characterPoseImage != null ? characterPoseImage.color : activeSpeakerColor;
        Color start2 = character2PoseImage != null ? character2PoseImage.color : activeSpeakerColor;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * dimTransitionSpeed;
            Color cur1 = Color.Lerp(start1, target1, t);
            Color cur2 = Color.Lerp(start2, target2, t);

            SetCharacter1ColorDirect(cur1);
            SetCharacter2ColorDirect(cur2);

            yield return null;
        }

        SetCharacter1ColorDirect(target1);
        SetCharacter2ColorDirect(target2);
        dimmingRoutine = null;
    }

    private void Update()
    {
        advanceConsumedThisFrame = false;
    }

    private bool IsAdvanceDialogueRequested()
    {
        if (advanceConsumedThisFrame) return false;
        if (!Input.GetMouseButtonDown(0)) return false;
        // Bấm nút "Tua nhanh" không tính là bấm chuyển câu
        return fastForwardRect == null ||
               !RectTransformUtility.RectangleContainsScreenPoint(fastForwardRect, Input.mousePosition, null);
    }

    // ===== Nhịp đọc thoại =====
    // Câu tự chuyển được giữ lại đủ lâu để đọc (khoảng 15 ký tự/giây), bấm thì sang câu ngay.
    // Nút "Tua nhanh": chữ hiện ngay và mỗi câu chỉ dừng ngắn, dành cho người chơi lại.
    private const float ReadingCharsPerSecond = 15f;
    private const float ReadingExtraSeconds = 0.6f;
    private const float FastForwardHold = 0.35f;

    private bool fastForward;
    private RectTransform fastForwardRect;
    private TextMeshProUGUI fastForwardLabel;

    private void CreateFastForwardButton()
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("ScenarioControls", 200);
        canvas.transform.SetParent(transform, false);

        Button btn = RuntimeUI.CreatePillButton(canvas.transform, "Tua nhanh", new Vector2(240f, 76f), ToggleFastForward);
        fastForwardRect = (RectTransform)btn.transform;
        fastForwardRect.anchorMin = fastForwardRect.anchorMax = new Vector2(1f, 1f);
        fastForwardRect.anchoredPosition = new Vector2(-160f, -60f);
        fastForwardLabel = btn.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void ToggleFastForward()
    {
        fastForward = !fastForward;
        if (fastForwardLabel != null) fastForwardLabel.text = fastForward ? "Dừng tua" : "Tua nhanh";
    }

    /// <summary>Thời gian giữ câu sau khi gõ xong: không ngắn hơn thời gian cần để đọc hết câu.</summary>
    private float GetLineHoldTime(DialogueLineData line)
    {
        if (fastForward) return FastForwardHold;
        float typingTime = line.text.Length * TypewriterCharDelay + TypewriterBounceDuration;
        float readTime = line.text.Length / ReadingCharsPerSecond + ReadingExtraSeconds;
        return Mathf.Max(line.displayDuration, readTime - typingTime);
    }

    private void ConsumeAdvanceDialogueRequest()
    {
        advanceConsumedThisFrame = true;
    }

    private void InitVideoPlayers()
    {
        if (activeVideoPlayer != null && standbyVideoPlayer != null) return;

        if (bgVideoPlayer == null)
        {
            GameObject vpObj = new GameObject("AutoVideoPlayer");
            vpObj.transform.SetParent(this.transform);
            bgVideoPlayer = vpObj.AddComponent<UnityEngine.Video.VideoPlayer>();
            bgVideoPlayer.playOnAwake = false;
            bgVideoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
            if (Camera.main != null)
            {
                bgVideoPlayer.targetCamera = Camera.main;
            }
        }

        activeVideoPlayer = bgVideoPlayer;
        activeVideoPlayer.waitForFirstFrame = true;
        activeVideoPlayer.skipOnDrop = true;

        if (standbyVideoPlayer == null)
        {
            GameObject standbyObj = Instantiate(activeVideoPlayer.gameObject, activeVideoPlayer.transform.parent);
            standbyObj.name = activeVideoPlayer.gameObject.name + "_Standby";
            
            // Giữ vị trí Canvas Hierarchy ngay sau activeVideoPlayer (trước TimeText/DescText/ChoicesContainer)
            int activeIndex = activeVideoPlayer.transform.GetSiblingIndex();
            standbyObj.transform.SetSiblingIndex(activeIndex + 1);

            standbyVideoPlayer = standbyObj.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (standbyVideoPlayer == null)
            {
                standbyVideoPlayer = standbyObj.AddComponent<UnityEngine.Video.VideoPlayer>();
            }
            standbyVideoPlayer.playOnAwake = false;
            standbyVideoPlayer.waitForFirstFrame = true;
            standbyVideoPlayer.skipOnDrop = true;
            standbyVideoPlayer.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        InitVideoPlayers();
        CreateFastForwardButton();

        if (activeVideoPlayer != null)
        {
            activeVideoPlayer.gameObject.SetActive(false);
        }

        if (StomachDayData.Instance != null)
        {
            StomachDayData.Instance.ResetData();
        }

        // Chọn kịch bản theo ngày đang chơi (Resources/Days/Day{n}); không có cấu hình thì dùng kịch bản gán trong Inspector
        DayConfig dayConfig = GameProgress.CurrentDayConfig;
        if (dayConfig != null && dayConfig.scenario != null)
        {
            currentScenario = dayConfig.scenario;
        }

        if (currentScenario != null && currentScenario.situations.Count > 0)
        {
            situationLookup.Clear();
            foreach (var sit in currentScenario.situations)
            {
                if (!string.IsNullOrEmpty(sit.guid))
                {
                    if (!situationLookup.ContainsKey(sit.guid))
                    {
                        situationLookup.Add(sit.guid, sit);
                    }
                    else
                    {
                        Debug.LogError($"[Scenario Error] Duplicate GUID found: {sit.guid}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentScenario.entryGuid) && situationLookup.ContainsKey(currentScenario.entryGuid))
            {
                currentSituationIndex = currentScenario.situations.IndexOf(situationLookup[currentScenario.entryGuid]);
            }
            else
            {
                currentSituationIndex = 0;
            }

            LoadSituation(currentSituationIndex);
        }
        else
        {
            Debug.LogWarning("Chưa gán Scenario Data hoặc Data trống!");
        }
    }

    private void LoadSituation(int index)
    {
        if (index < currentScenario.situations.Count)
        {
            SituationData sit = currentScenario.situations[index];
            
            // Cập nhật Ảnh minh họa hoặc Video theo mediaType
            if (sit.mediaType == BackgroundMediaType.Image)
            {
                if (activeVideoPlayer != null)
                {
                    activeVideoPlayer.Stop();
                    activeVideoPlayer.gameObject.SetActive(false);
                }
                if (standbyVideoPlayer != null)
                {
                    standbyVideoPlayer.Stop();
                    standbyVideoPlayer.gameObject.SetActive(false);
                }

                if (illustrationImage != null)
                {
                    if (sit.illustration != null)
                    {
                        illustrationImage.sprite = sit.illustration;
                        illustrationImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        illustrationImage.gameObject.SetActive(false);
                    }
                }
            }
            else // Video
            {
                if (illustrationImage != null)
                {
                    illustrationImage.gameObject.SetActive(false);
                }
            }

            // Cập nhật và kích hoạt Khung thoại Description (Panel & Image)
            if (descriptionPanel != null)
            {
                descriptionPanel.SetActive(true);
                if (descriptionBgImage == null) descriptionBgImage = descriptionPanel.GetComponent<Image>() ?? descriptionPanel.GetComponentInChildren<Image>();
                if (descriptionTextUI == null) descriptionTextUI = descriptionPanel.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (descriptionBgImage != null)
            {
                Sprite bgSprite = sit.descriptionBgSprite != null ? sit.descriptionBgSprite : defaultDescriptionBgSprite;
                if (bgSprite != null)
                {
                    descriptionBgImage.sprite = bgSprite;
                }
                descriptionBgImage.gameObject.SetActive(true);
            }

            // Cập nhật Biểu cảm/Tư thế nhân vật mặc định của Tình huống
            UpdateCharacter1Display(sit.characterPose, sit.characterExpression, sit.characterHair);
            if (sit.hasSecondCharacter)
            {
                UpdateCharacter2Display(sit.character2Pose, sit.character2Expression, sit.character2Hair);
            }
            ApplySpeakerHighlight(sit, null, immediate: true);

            // Cập nhật Text
            if(timeTextUI != null) timeTextUI.text = sit.timeText;

            // Dọn dẹp nút cũ
            foreach (var btn in activeButtons)
            {
                Destroy(btn);
            }
            activeButtons.Clear();
            if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

            if (playSituationRoutine != null) StopCoroutine(playSituationRoutine);
            playSituationRoutine = StartCoroutine(PlaySituationCoroutine(sit));
        }
        else
        {
            // Hết kịch bản, chuyển sang Scene 2
            SceneVideoTransition.LoadScene("Scene2_Transition"); // hiệu ứng trượt UI
        }
    }

    private System.Collections.IEnumerator PlayVideoCoroutine(string videoFile, UnityEngine.Video.VideoClip legacyClip, bool loop)
    {
        if (!HasVideo(videoFile, legacyClip)) yield break;

        InitVideoPlayers();

        // 1. If active player is already playing this exact clip, just update looping and continue seamlessly!
        if (activeVideoPlayer != null && IsPlayingVideo(activeVideoPlayer, videoFile, legacyClip))
        {
            activeVideoPlayer.isLooping = loop;
            yield break;
        }

        // 2. Prepare new clip on standby player in the background
        if (standbyVideoPlayer == null) yield break;

        // Đảm bảo vị trí Canvas sibling luôn nằm ngay dưới activeVideoPlayer (trước TimeText/DescText/ChoicesContainer)
        if (activeVideoPlayer != null)
        {
            standbyVideoPlayer.transform.SetSiblingIndex(activeVideoPlayer.transform.GetSiblingIndex() + 1);
        }

        standbyVideoPlayer.gameObject.SetActive(true);
        AssignVideoSource(standbyVideoPlayer, videoFile, legacyClip);
        standbyVideoPlayer.isLooping = loop;
        standbyVideoPlayer.waitForFirstFrame = true;
        standbyVideoPlayer.Prepare();

        // Wait until standby player finishes preparing (web phải tải video qua mạng nên chờ lâu hơn)
#if UNITY_WEBGL && !UNITY_EDITOR
        float timeout = 15.0f;
#else
        float timeout = 3.0f;
#endif
        float elapsed = 0f;
        while (!standbyVideoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Play standby player once prepared
        standbyVideoPlayer.Play();

        // Overlap buffer: Keep old video playing for 0.2 seconds while standby video warms up to ensure zero stutter
        if (activeVideoPlayer != null && activeVideoPlayer.isPlaying)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            yield return null;
        }

        // 4. Stop old active player and hide it
        if (activeVideoPlayer != null && activeVideoPlayer != standbyVideoPlayer)
        {
            activeVideoPlayer.Stop();
            activeVideoPlayer.gameObject.SetActive(false);
        }

        // 5. Swap active and standby player references
        var temp = activeVideoPlayer;
        activeVideoPlayer = standbyVideoPlayer;
        standbyVideoPlayer = temp;
        bgVideoPlayer = activeVideoPlayer;
    }

    /// <summary>
    /// Video kịch bản nằm ở StreamingAssets/Scenario/{videoFile}.mp4 và phát qua URL (chạy được cả WebGL).
    /// legacyClip (trường VideoClip cũ) chỉ dùng khi chưa điền videoFile.
    /// </summary>
    private static bool HasVideo(string videoFile, UnityEngine.Video.VideoClip legacyClip)
    {
        return !string.IsNullOrEmpty(videoFile) || legacyClip != null;
    }

    private static void AssignVideoSource(UnityEngine.Video.VideoPlayer vp, string videoFile, UnityEngine.Video.VideoClip legacyClip)
    {
        if (string.IsNullOrEmpty(videoFile))
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            videoFile = legacyClip.name;
#else
            vp.source = UnityEngine.Video.VideoSource.VideoClip;
            vp.clip = legacyClip;
            return;
#endif
        }
        vp.source = UnityEngine.Video.VideoSource.Url;
        vp.url = ScenarioVideoUrl(videoFile);
    }

    public static string ScenarioVideoUrl(string videoFile)
    {
        if (!videoFile.EndsWith(".mp4", System.StringComparison.OrdinalIgnoreCase)) videoFile += ".mp4";
        return Application.streamingAssetsPath + "/Scenario/" + videoFile;
    }

    private static bool IsPlayingVideo(UnityEngine.Video.VideoPlayer vp, string videoFile, UnityEngine.Video.VideoClip legacyClip)
    {
        if (!vp.isPlaying) return false;
        if (vp.source == UnityEngine.Video.VideoSource.Url)
        {
            string file = !string.IsNullOrEmpty(videoFile) ? videoFile : (legacyClip != null ? legacyClip.name : null);
            return file != null && vp.url == ScenarioVideoUrl(file);
        }
        return string.IsNullOrEmpty(videoFile) && vp.clip == legacyClip;
    }

    private System.Collections.IEnumerator PlaySituationCoroutine(SituationData sit)
    {
        // 1. Play Dialogue Video (nếu mediaType là Video)
        if (sit.mediaType == BackgroundMediaType.Video && HasVideo(sit.dialogueVideoFile, sit.dialogueVideo))
        {
            yield return StartCoroutine(PlayVideoCoroutine(sit.dialogueVideoFile, sit.dialogueVideo, sit.loopDialogueVideo));
        }

        // 2. Play Dialogues
        List<DialogueLineData> lines = sit.dialogueLines;
        if (lines == null || lines.Count == 0)
        {
            // Fallback for old data if migration wasn't run
            lines = new List<DialogueLineData>();
            if (sit.dialogues != null && sit.dialogues.Count > 0)
            {
                foreach(var s in sit.dialogues)
                    lines.Add(new DialogueLineData { text = s, advanceMode = DialogueAdvanceMode.Auto, displayDuration = 3f });
            }
            else
            {
                lines.Add(new DialogueLineData { text = sit.description, advanceMode = DialogueAdvanceMode.Auto, displayDuration = 3f });
            }
        }

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.text)) continue;

            // Cập nhật Tên người nói (Speaker Name)
            string currentSpeaker = !string.IsNullOrEmpty(line.speakerName) ? line.speakerName : sit.speakerName;
            if (speakerNameTextUI != null)
            {
                if (!string.IsNullOrEmpty(currentSpeaker))
                {
                    speakerNameTextUI.text = currentSpeaker;
                    speakerNameTextUI.gameObject.SetActive(true);
                }
                else
                {
                    speakerNameTextUI.text = "";
                }
            }

            // Cập nhật Highlight / Dimming cho người nói (nếu có 2 người)
            ApplySpeakerHighlight(sit, line, immediate: false);

            PlayDescriptionText(line.text);
            if (fastForward) ForceFinishTypewriter(line.text);

            // STATE A: Đang gõ chữ (Typewriter)
            while (!isTypewriterFinished)
            {
                if (IsAdvanceDialogueRequested())
                {
                    ConsumeAdvanceDialogueRequest();
                    ForceFinishTypewriter(line.text);
                    break; // Thoát vòng lặp gõ, chữ hiện ra toàn bộ, NHƯNG KHÔNG CHUYỂN CÂU
                }
                yield return null;
            }

            // Đợi 1 frame phòng trường hợp click vừa kết thúc vòng lặp trên
            yield return null;

            // STATE B & C: Đã gõ xong
            if (line.advanceMode == DialogueAdvanceMode.Auto)
            {
                // Tự chuyển sau khi đủ thời gian đọc; người chơi bấm thì sang câu ngay
                float holdTime = GetLineHoldTime(line);
                float timer = 0f;
                while (timer < holdTime)
                {
                    if (IsAdvanceDialogueRequested())
                    {
                        ConsumeAdvanceDialogueRequest();
                        break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else if (line.advanceMode == DialogueAdvanceMode.WaitForPlayer)
            {
                while (!IsAdvanceDialogueRequested())
                {
                    yield return null;
                }
                ConsumeAdvanceDialogueRequest();
            }
        }

        // 3. Handle Branching / Auto Transition
        if (sit.choices.Count > 0)
        {
            if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);
            foreach (var choice in sit.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
                activeButtons.Add(btnObj);
                
                Image btnBg = btnObj.GetComponent<Image>() ?? btnObj.GetComponentInChildren<Image>();
                if (btnBg != null)
                {
                    Sprite bgSprite = choice.choiceBgSprite != null ? choice.choiceBgSprite : defaultChoiceBgSprite;
                    if (bgSprite != null)
                    {
                        btnBg.sprite = bgSprite;
                    }
                }

                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    // Tự động đồng bộ Font & Material nghệ thuật từ descriptionTextUI nếu btnText bị thiếu Font
                    if ((btnText.font == null || btnText.fontSharedMaterial == null) && descriptionTextUI != null)
                    {
                        btnText.font = descriptionTextUI.font;
                        btnText.fontSharedMaterial = descriptionTextUI.fontSharedMaterial;
                    }

                    btnText.text = choice.choiceText;
                    btnText.enableAutoSizing = true;
                    btnText.fontSizeMin = 13f;
                    btnText.fontSizeMax = 22f;
                    if (btnText.font != null)
                    {
                        btnText.font.TryAddCharacters(choice.choiceText);
                    }
                }

                Button btnComponent = btnObj.GetComponent<Button>();
                if (btnComponent != null)
                {
                    ChoiceData currentChoice = choice;
                    btnComponent.onClick.AddListener(() => OnChoiceSelected(currentChoice));
                    UIClickSounds.Attach(btnComponent);
                }
            }
        }
        else
        {
            // Auto Transition Logic
            StartCoroutine(AutoTransitionCoroutine(sit));
        }
    }

    private bool isProcessingChoice = false;

    private void OnChoiceSelected(ChoiceData choice)
    {
        if (isProcessingChoice) return;
        StartCoroutine(ProcessChoiceCoroutine(choice));
    }

    private System.Collections.IEnumerator AutoTransitionCoroutine(SituationData sit)
    {
        isProcessingChoice = true;
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        // Bắt đầu bật video sau trước khoảng 0.2s trước khi kết thúc delay
        float delay = sit.autoTransitionDelay;
        if (delay > 0.2f)
        {
            yield return new WaitForSeconds(delay - 0.2f);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        isProcessingChoice = false;
        ExecuteTransition(sit.autoTargetType, sit.autoTargetGuid);
    }

    private System.Collections.IEnumerator ProcessChoiceCoroutine(ChoiceData choice)
    {
        isProcessingChoice = true;

        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        // Cập nhật biểu cảm/tư thế nhân vật tương ứng với lựa chọn này (nếu có gán)
        UpdateCharacter1Display(choice.characterPose, choice.characterExpression, choice.characterHair);
        if (choice.character2Pose != null || choice.character2Expression != null || choice.character2Hair != null)
        {
            UpdateCharacter2Display(choice.character2Pose, choice.character2Expression, choice.character2Hair);
        }

        // Play animation video (if any)
        if (HasVideo(choice.videoFile, choice.videoClip))
        {
            yield return StartCoroutine(PlayVideoCoroutine(choice.videoFile, choice.videoClip, false));
        }

        if (choice.impacts != null && choice.impacts.Count > 0)
        {
            yield return StartCoroutine(SpawnStatImpactPopupsRoutine(choice.impacts));
        }

        // Thẻ "Kiến thức y khoa": chờ người chơi đọc & bấm "Đã hiểu"
        if (!string.IsNullOrEmpty(choice.knowledgeTip))
        {
            yield return new WaitForSeconds(0.6f); // cho popup chỉ số bay lên xong
            yield return KnowledgeCardUI.ShowAndWait(choice.knowledgeTip);
            NotebookProgress.Unlock(NotebookProgress.TipId(choice.knowledgeTip));
        }

        // Lựa chọn tốt có thưởng vật phẩm -> cất vào túi để dùng trong trận hôm nay
        if (choice.rewardItem != LifestyleItem.None)
        {
            if (StomachDayData.Instance != null) StomachDayData.Instance.AddItem(choice.rewardItem);
            ItemCatalog.Entry entry = ItemCatalog.Instance != null ? ItemCatalog.Instance.Get(choice.rewardItem) : null;
            if (entry != null) yield return ItemRewardUI.ShowAndWait(entry);
        }

        // Đợi theo delay sau lựa chọn (bật cảnh sau sớm 0.2s)
        float delay = choice.delayAfterChoice;
        if (delay > 0.2f)
        {
            yield return new WaitForSeconds(delay - 0.2f);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        isProcessingChoice = false;
        ExecuteTransition(choice.targetType, choice.targetGuid);
    }

    private System.Collections.IEnumerator SpawnStatImpactPopupsRoutine(List<ImpactData> impacts)
    {
        if (impacts == null || impacts.Count == 0) yield break;

        for (int i = 0; i < impacts.Count; i++)
        {
            var impact = impacts[i];

            // Tự động cập nhật trực tiếp chỉ số vào StomachDayData khi người chơi chọn
            ApplyImpactToStomachData(impact.targetStat, impact.value);

            if (onChoiceImpact != null)
            {
                onChoiceImpact.Invoke(impact.targetStat, impact.value);
            }

            // Mỗi popup tiếp theo sẽ dịch lên cao hơn 45px và xuất hiện cách nhau 0.18s
            float verticalOffsetStep = i * 45f;
            SpawnStatImpactPopup(impact.targetStat, impact.value, verticalOffsetStep);

            yield return new WaitForSeconds(0.18f);
        }
    }

    public static void ApplyImpactToStomachData(StatImpactType stat, float rawValue)
    {
        if (StomachDayData.Instance == null)
        {
            GameObject sddObj = new GameObject("StomachDayData");
            sddObj.AddComponent<StomachDayData>();
        }

        // Công thức quy đổi nằm trong DayStats (dùng chung với công cụ cân bằng)
        DayStats stats = StomachDayData.Instance.ToStats();
        stats.ApplyImpact(stat, rawValue);
        StomachDayData.Instance.SetStats(stats);

        Debug.Log($"[StomachDayData] Applied Impact ({stat}, {rawValue}) -> Immunity={StomachDayData.Instance.immunityMultiplier * 100:0}%, Energy={StomachDayData.Instance.energyMultiplier * 100:0}%, Hydration={StomachDayData.Instance.hydrationMultiplier * 100:0}%, Recovery={StomachDayData.Instance.recoveryMultiplier * 100:0}%, Toxicity={StomachDayData.Instance.toxicityLevel}");
    }

    public void SpawnStatImpactPopup(StatImpactType stat, float value, float extraVerticalOffset = 0f)
    {
        if (statImpactPopupPrefab == null) return;

        // Xác định vị trí spawn (Default: trên đầu characterPoseImage hoặc Canvas)
        Transform parentTransform = choicesContainer != null ? choicesContainer.parent : transform;
        Vector3 spawnWorldPos = Vector3.zero;

        if (popupSpawnTransform != null)
        {
            spawnWorldPos = popupSpawnTransform.position + new Vector3(0f, extraVerticalOffset, 0f);
        }
        else if (characterPoseImage != null)
        {
            spawnWorldPos = characterPoseImage.transform.position + new Vector3(0f, 160f + extraVerticalOffset, 0f);
        }
        else
        {
            spawnWorldPos = transform.position + new Vector3(0f, 200f + extraVerticalOffset, 0f);
        }

        GameObject popupObj = Instantiate(statImpactPopupPrefab, parentTransform);
        popupObj.transform.position = spawnWorldPos;

        // Lấy Icon & Tên hiển thị tương ứng với 5 chỉ số
        Sprite icon = null;
        string statDisplayName = "";

        switch (stat)
        {
            case StatImpactType.Immunity:
                icon = immunityIcon;
                statDisplayName = "Kháng thể";
                break;
            case StatImpactType.Energy:
                icon = energyIcon;
                statDisplayName = "Thể lực";
                break;
            case StatImpactType.Toxicity:
                icon = toxicityIcon;
                statDisplayName = "Độc tố";
                break;
            case StatImpactType.Hydration:
                icon = hydrationIcon;
                statDisplayName = "Nước";
                break;
            case StatImpactType.Recovery:
                icon = recoveryIcon;
                statDisplayName = "Hồi phục";
                break;
        }

        StatImpactPopup popupScript = popupObj.GetComponent<StatImpactPopup>();
        if (popupScript != null)
        {
            popupScript.Setup(icon, statDisplayName, value, positiveStatColor, negativeStatColor);
        }
    }

    private void ExecuteTransition(TargetType tType, string tGuid)
    {
        // Hỗ trợ tương thích ngược (Backward Compatibility) cho chuỗi "END" cũ
        if (tType == TargetType.EndScene || tGuid == "END")
        {
            SceneVideoTransition.LoadScene("Scene2_Transition"); // hiệu ứng trượt UI
            return;
        }

        if (!string.IsNullOrEmpty(tGuid))
        {
            if (situationLookup.ContainsKey(tGuid))
            {
                SituationData nextSit = situationLookup[tGuid];
                currentSituationIndex = currentScenario.situations.IndexOf(nextSit);
                
                if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);
                LoadSituation(currentSituationIndex);
            }
            else
            {
                Debug.LogError($"[Scenario Error] Cannot find target Situation GUID {tGuid}. Transition aborted.");
                if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);
            }
        }
        else
        {
            currentSituationIndex++;
            if (currentSituationIndex < currentScenario.situations.Count)
            {
                if (choicesContainer != null) choicesContainer.gameObject.SetActive(true);
                LoadSituation(currentSituationIndex);
            }
            else
            {
                SceneVideoTransition.LoadScene("Scene2_Transition"); // hiệu ứng trượt UI
            }
        }
    }

    private const float TypewriterCharDelay = 0.02f;
    private const float TypewriterBounceDuration = 0.3f;

    private Coroutine typeRoutine;
    private bool isTypewriterFinished = false;

    private void PlayDescriptionText(string text)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        isTypewriterFinished = false;
        if (descriptionTextUI != null)
        {
            typeRoutine = StartCoroutine(TypewriterBouncy(descriptionTextUI, text));
        }
        else
        {
            isTypewriterFinished = true;
        }
    }

    private void ForceFinishTypewriter(string fullText)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        if (descriptionTextUI != null)
        {
            descriptionTextUI.text = fullText;
            descriptionTextUI.ForceMeshUpdate();

            TMP_TextInfo textInfo = descriptionTextUI.textInfo;
            for (int i = 0; i < textInfo.materialCount; i++)
            {
                if (textInfo.meshInfo[i].mesh != null)
                {
                    descriptionTextUI.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }
            }
        }
        isTypewriterFinished = true;
    }

    private System.Collections.IEnumerator TypewriterBouncy(TextMeshProUGUI tmpText, string fullText)
    {
        tmpText.text = fullText;
        if (tmpText.font != null)
        {
            tmpText.font.TryAddCharacters(fullText);
        }
        tmpText.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpText.textInfo;
        int characterCount = textInfo.characterCount;
        
        List<Vector3[]> originalVertices = new List<Vector3[]>();
        for (int i = 0; i < textInfo.materialCount; i++)
        {
            Vector3[] orig = new Vector3[textInfo.meshInfo[i].vertices.Length];
            System.Array.Copy(textInfo.meshInfo[i].vertices, orig, orig.Length);
            originalVertices.Add(orig);
        }

        float[] charScales = new float[characterCount];
        float[] charTimes = new float[characterCount];
        for(int i = 0; i < characterCount; i++) charScales[i] = 0;

        float delayBetweenChars = TypewriterCharDelay; // Tốc độ xuất hiện từng chữ
        float bounceDuration = TypewriterBounceDuration; // Thời gian nảy của mỗi chữ
        float totalDuration = characterCount * delayBetweenChars + bounceDuration;
        float elapsed = 0;
        int typedChars = 0;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // Tiếng gõ chữ khi có ký tự mới hiện ra (bỏ qua dấu cách; GameSfx tự giãn nhịp để không dồn dập)
            int revealed = Mathf.Min(characterCount, Mathf.FloorToInt(elapsed / delayBetweenChars) + 1);
            if (revealed > typedChars)
            {
                typedChars = revealed;
                if (textInfo.characterInfo[revealed - 1].isVisible) GameSfx.Play(GameSfx.Kind.Typing);
            }
            
            for (int i = 0; i < textInfo.materialCount; i++)
            {
                System.Array.Copy(originalVertices[i], textInfo.meshInfo[i].vertices, originalVertices[i].Length);
            }
            
            for (int i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                float charStartTime = i * delayBetweenChars;
                
                if (elapsed >= charStartTime)
                {
                    charTimes[i] += Time.deltaTime;
                    float t = Mathf.Clamp01(charTimes[i] / bounceDuration);
                    charScales[i] = OvershootEaseOut(t);
                }
                else
                {
                    charScales[i] = 0; 
                }

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                Vector3 center = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
                
                for (int j = 0; j < 4; j++)
                {
                    Vector3 offset = vertices[vertexIndex + j] - center;
                    vertices[vertexIndex + j] = center + offset * charScales[i];
                }
            }
            
            for (int i = 0; i < textInfo.materialCount; i++)
            {
                if (textInfo.meshInfo[i].mesh != null)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    tmpText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }
            }

            yield return null;
        }
        isTypewriterFinished = true;
    }

    private float OvershootEaseOut(float t)
    {
        t -= 1.0f;
        return (t * t * ((1.70158f + 1) * t + 1.70158f) + 1.0f);
    }
}
