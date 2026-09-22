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
    
    public Transform choicesContainer; // Nơi chứa các nút (Dùng Vertical Layout Group)
    public GameObject choiceButtonPrefab; // Prefab của một nút chọn

    public Action<StatImpactType, float> onChoiceImpact;

    private int currentSituationIndex = 0;
    private List<GameObject> activeButtons = new List<GameObject>();
    private Dictionary<string, SituationData> situationLookup = new Dictionary<string, SituationData>();
    private Coroutine playSituationRoutine;

    private bool advanceConsumedThisFrame = false;

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

    private void Start()
    {
        // Tự động tạo Video Player nếu chưa gán
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

        if (bgVideoPlayer != null)
        {
            bgVideoPlayer.gameObject.SetActive(false);
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

    private void PlayVideo(UnityEngine.Video.VideoClip clip, bool loop)
    {
        if (bgVideoPlayer == null) return;

        if (clip == null)
        {
            // Do NOT stop the video if clip is null.
            // This maintains continuous video architecture across situations.
            return;
        }

        bgVideoPlayer.gameObject.SetActive(true);
        if (bgVideoPlayer.clip != clip || !bgVideoPlayer.isPlaying)
        {
            bgVideoPlayer.clip = clip;
            bgVideoPlayer.isLooping = loop;
            bgVideoPlayer.Play();
        }
        else
        {
            bgVideoPlayer.isLooping = loop;
        }
    }

    private System.Collections.IEnumerator PlaySituationCoroutine(SituationData sit)
    {
        // 1. Play Dialogue Video (Continues if clip is null or unchanged)
        PlayVideo(sit.dialogueVideo, sit.loopDialogueVideo);

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

        // Đợi theo cài đặt delay
        yield return new WaitForSeconds(sit.autoTransitionDelay);

        isProcessingChoice = false;
        ExecuteTransition(sit.autoTargetType, sit.autoTargetGuid);
    }

    private System.Collections.IEnumerator ProcessChoiceCoroutine(ChoiceData choice)
    {
        isProcessingChoice = true;

        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        // Play old animation video (if any)
        if (choice.videoClip != null)
        {
            PlayVideo(choice.videoClip, false);
        }

        if (onChoiceImpact != null)
        {
            foreach (var impact in choice.impacts)
            {
                onChoiceImpact.Invoke(impact.targetStat, impact.value);
            }
        }

        // Đợi một khoảng thời gian trước khi tải câu hỏi tiếp theo
        yield return new WaitForSeconds(choice.delayAfterChoice);

        isProcessingChoice = false;
        ExecuteTransition(choice.targetType, choice.targetGuid);
    }

    private void ExecuteTransition(TargetType tType, string tGuid)
    {
        if (tType == TargetType.EndScene)
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
