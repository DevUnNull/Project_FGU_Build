using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ScenarioPlayer : MonoBehaviour
{
    [Header("Data")]
    public ScenarioData currentScenario;

    [Header("UI References")]
    public TextMeshProUGUI timeTextUI;
    public TextMeshProUGUI descriptionTextUI;
    public Image illustrationImage;
    public UnityEngine.Video.VideoPlayer bgVideoPlayer;

    [Header("Character UI References")]
    public Image characterPoseImage;
    public Image characterExpressionImage;
    public Image characterHairImage;
    
    public Transform choicesContainer; // Nơi chứa các nút (Dùng Vertical Layout Group)
    public GameObject choiceButtonPrefab; // Prefab của một nút chọn

    public Action<StatImpactType, float> onChoiceImpact;

    private int currentSituationIndex = 0;
    private List<GameObject> activeButtons = new List<GameObject>();
    private Dictionary<string, SituationData> situationLookup = new Dictionary<string, SituationData>();
    private Coroutine playSituationRoutine;
    private Coroutine videoTransitionRoutine;

    private UnityEngine.Video.VideoPlayer activeVideoPlayer;
    private UnityEngine.Video.VideoPlayer standbyVideoPlayer;

    private bool advanceConsumedThisFrame = false;

    private void UpdateCharacterDisplay(Sprite pose, Sprite expression, Sprite hair)
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

    private void Update()
    {
        advanceConsumedThisFrame = false;
    }

    private bool IsAdvanceDialogueRequested()
    {
        if (advanceConsumedThisFrame) return false;
        return Input.GetMouseButtonDown(0);
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

        if (activeVideoPlayer != null)
        {
            activeVideoPlayer.gameObject.SetActive(false);
        }

        if (StomachDayData.Instance != null)
        {
            StomachDayData.Instance.ResetData();
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
            
            // Cập nhật Ảnh minh họa
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

            // Cập nhật Biểu cảm/Tư thế nhân vật mặc định của Tình huống
            UpdateCharacterDisplay(sit.characterPose, sit.characterExpression, sit.characterHair);

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
            SceneManager.LoadScene("Scene2_Transition");
        }
    }

    private System.Collections.IEnumerator PlayVideoCoroutine(UnityEngine.Video.VideoClip clip, bool loop)
    {
        if (clip == null) yield break;

        InitVideoPlayers();

        // 1. If active player is already playing this exact clip, just update looping and continue seamlessly!
        if (activeVideoPlayer != null && activeVideoPlayer.clip == clip && activeVideoPlayer.isPlaying)
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
        standbyVideoPlayer.clip = clip;
        standbyVideoPlayer.isLooping = loop;
        standbyVideoPlayer.waitForFirstFrame = true;
        standbyVideoPlayer.Prepare();

        // Wait until standby player finishes preparing (with a 3-second safety timeout)
        float timeout = 3.0f;
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

    private void PlayVideo(UnityEngine.Video.VideoClip clip, bool loop)
    {
        if (clip == null) return;
        if (videoTransitionRoutine != null) StopCoroutine(videoTransitionRoutine);
        videoTransitionRoutine = StartCoroutine(PlayVideoCoroutine(clip, loop));
    }

    private System.Collections.IEnumerator PlaySituationCoroutine(SituationData sit)
    {
        // 1. Play Dialogue Video (Continues if clip is null or unchanged, prepares asynchronously without white flash)
        yield return StartCoroutine(PlayVideoCoroutine(sit.dialogueVideo, sit.loopDialogueVideo));

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

            PlayDescriptionText(line.text);

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
                float timer = 0f;
                while (timer < line.displayDuration)
                {
                    timer += Time.deltaTime;
                    // Yêu cầu: Không cho phép click để skip khi đang trong chế độ Auto
                    // Click trong thời gian này sẽ bị bỏ qua (không làm gì cả)
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
                
                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = choice.choiceText;

                Button btnComponent = btnObj.GetComponent<Button>();
                if (btnComponent != null)
                {
                    ChoiceData currentChoice = choice;
                    btnComponent.onClick.AddListener(() => OnChoiceSelected(currentChoice));
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
        UpdateCharacterDisplay(choice.characterPose, choice.characterExpression, choice.characterHair);

        // Play animation video (if any)
        if (choice.videoClip != null)
        {
            yield return StartCoroutine(PlayVideoCoroutine(choice.videoClip, false));
        }

        if (onChoiceImpact != null)
        {
            foreach (var impact in choice.impacts)
            {
                onChoiceImpact.Invoke(impact.targetStat, impact.value);
            }
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

    private void ExecuteTransition(TargetType tType, string tGuid)
    {
        // Hỗ trợ tương thích ngược (Backward Compatibility) cho chuỗi "END" cũ
        if (tType == TargetType.EndScene || tGuid == "END")
        {
            SceneManager.LoadScene("Scene2_Transition");
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
                SceneManager.LoadScene("Scene2_Transition");
            }
        }
    }

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

        float delayBetweenChars = 0.02f; // Tốc độ xuất hiện từng chữ
        float bounceDuration = 0.3f; // Thời gian nảy của mỗi chữ
        float totalDuration = characterCount * delayBetweenChars + bounceDuration;
        float elapsed = 0;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            
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
