using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Script quản lý bảng hướng dẫn giới thiệu từng tướng (Hero Tutorial / Giới thiệu trận).
/// Gắn script này vào object Manager (hoặc HuonngDan) để tùy chỉnh danh sách các tướng.
/// Tự động chạy Animation mở/đóng mượt mà bất kể Game đang Pause (Time.timeScale = 0).
/// </summary>
public class HeroTutorialManager : MonoBehaviour
{
    public static HeroTutorialManager Instance { get; private set; }

    [System.Serializable]
    public class HeroTutorialData
    {
        [Tooltip("Tên tướng (dễ nhận biết trong Inspector)")]
        public string heroName = "Tướng mới";

        [Tooltip("Video clip giới thiệu kỹ năng / cách dùng tướng")]
        public VideoClip videoClip;

        [Tooltip("Mô tả tác dụng / kỹ năng của tướng")]
        [TextArea(3, 8)]
        public string description = "Mô tả kỹ năng và cách dùng...";

        [Tooltip("Hình ảnh biểu cảm Player ở góc trái")]
        public Sprite playerExpression;
    }

    [Header("=== Cấu Hình UI Components ===")]
    [Tooltip("GameObject bảng hướng dẫn (HuonngDan hoặc BoardMenu)")]
    public GameObject tutorialBoard;

    [Tooltip("Animator của BoardMenu (Tự động tìm nếu rỗng)")]
    public Animator boardMenuAnimator;

    [Tooltip("Component VideoPlayer (gắn trên VideoHuongDan hoặc RawImage)")]
    public VideoPlayer videoPlayer;

    [Tooltip("UI Image góc trái hiển thị biểu cảm Player")]
    public Image playerImage;

    [Tooltip("UI Text / TextMeshProUGUI hiển thị nội dung mô tả")]
    public TextMeshProUGUI descriptionTMP;
    public Text descriptionLegacyText;

    [Tooltip("Nút Next (Chuyển tướng tiếp theo / Đóng bảng)")]
    public Button nextButton;

    [Tooltip("Text trên nút Next (TextMeshProUGUI hoặc UI Text)")]
    public TextMeshProUGUI nextButtonTMP;
    public Text nextButtonLegacyText;

    [Header("=== Cấu Hình Animation ===")]
    [Tooltip("Tên State Animation mở bảng trong Animator Controller")]
    public string openAnimState = "BoardMenu";

    [Tooltip("Tên State Animation đóng bảng trong Animator Controller")]
    public string closeAnimState = "BoardMenuClouse";

    [Tooltip("Thời gian chờ chạy animation đóng (giây)")]
    public float closeAnimTime = 0.3f;

    [Header("=== Cấu Hình Nhãn Nút Next ===")]
    [Tooltip("Chữ hiển thị khi còn tướng tiếp theo")]
    public string nextLabel = "Tiếp theo";

    [Tooltip("Chữ hiển thị ở tướng cuối cùng (ấn để tắt bảng)")]
    public string closeLabel = "Tắt";

    [Header("=== Danh Sách Tướng Hướng Dẫn ===")]
    [Tooltip("Thêm / bớt các tướng và cấu hình video, mô tả, hình biểu cảm tại đây")]
    public List<HeroTutorialData> heroTutorials = new List<HeroTutorialData>();

    private int currentIndex = 0;
    private bool isClosing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        AutoFindUIComponents();
        RegisterEvents();

        // Nếu có bảng Báo Cáo Ngày (DayReportUI) đang active, tạm ẩn HuonngDan để chờ DayReportUI tắt mới hiện lên.
        // Ngược lại nếu không có DayReportUI, sẽ mở ngay hướng dẫn nếu bảng đang active.
        DayReportUI dayReport = FindObjectOfType<DayReportUI>();
        if (dayReport != null && dayReport.gameObject.activeInHierarchy)
        {
            if (tutorialBoard != null)
            {
                tutorialBoard.SetActive(false);
            }
        }
        else
        {
            if (tutorialBoard != null && tutorialBoard.activeInHierarchy)
            {
                OpenTutorial();
            }
        }
    }

    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
    }

    private void UnregisterEvents()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }
    }

    /// <summary>
    /// Bắt đầu hiển thị bảng hướng dẫn từ tướng đầu tiên + chạy Animation Nảy Mở Bảng
    /// </summary>
    public void OpenTutorial()
    {
        if (heroTutorials == null || heroTutorials.Count == 0)
        {
            Debug.LogWarning("⚠️ HeroTutorialManager: Danh sách heroTutorials rỗng!");
            return;
        }

        isClosing = false;

        if (tutorialBoard != null)
        {
            tutorialBoard.SetActive(true);
        }

        // Tự động tìm Animator nếu chưa gán
        if (boardMenuAnimator == null && tutorialBoard != null)
        {
            boardMenuAnimator = tutorialBoard.GetComponentInChildren<Animator>();
        }

        // Chạy Animation Nảy Mở Bảng (Chế độ UnscaledTime để chạy được ngay cả khi Time.timeScale = 0)
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            boardMenuAnimator.enabled = true;
            try
            {
                boardMenuAnimator.Rebind();
                boardMenuAnimator.Play(openAnimState, 0, 0f);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"⚠️ Không thể phát animation '{openAnimState}': {ex.Message}");
            }
        }

        currentIndex = 0;
        DisplayHeroAtIndex(currentIndex);
    }

    /// <summary>
    /// Đóng / Tắt bảng hướng dẫn có chạy Animation Thu Nhỏ Mờ Dần
    /// </summary>
    public void CloseTutorial()
    {
        if (isClosing) return;
        StartCoroutine(CloseTutorialRoutine());
    }

    private IEnumerator CloseTutorialRoutine()
    {
        isClosing = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // Chạy Animation Đóng Bảng
        if (boardMenuAnimator != null)
        {
            boardMenuAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            try
            {
                boardMenuAnimator.Play(closeAnimState, 0, 0f);
            }
            catch { }

            yield return new WaitForSecondsRealtime(closeAnimTime);
        }

        if (tutorialBoard != null)
        {
            tutorialBoard.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        isClosing = false;
    }

    /// <summary>
    /// Hiển thị thông tin tướng tại index tương ứng
    /// </summary>
    public void DisplayHeroAtIndex(int index)
    {
        if (index < 0 || index >= heroTutorials.Count) return;

        HeroTutorialData data = heroTutorials[index];

        // 1. Cập nhật Biểu cảm Player (Hình ảnh góc trái)
        if (playerImage != null && data.playerExpression != null)
        {
            playerImage.sprite = data.playerExpression;
            playerImage.gameObject.SetActive(true);
        }

        // 2. Cập nhật Mô tả tác dụng của tướng
        if (descriptionTMP != null)
        {
            descriptionTMP.text = data.description;
        }
        else if (descriptionLegacyText != null)
        {
            descriptionLegacyText.text = data.description;
        }

        // 3. Cập nhật và phát Video giới thiệu tướng (Tối ưu WebGL)
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            if (data.videoClip != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Tránh browser Autoplay Policy chặn

                #if UNITY_WEBGL && !UNITY_EDITOR
                string fileName = data.videoClip.name + ".mp4";
                string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = videoUrl;
                #else
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = data.videoClip;
                #endif

                videoPlayer.isLooping = true;
                videoPlayer.Play();
            }
        }

        // 4. Cập nhật Chữ nút Next (nếu là tướng cuối cùng -> đổi thành "Tắt")
        bool isLastHero = (index == heroTutorials.Count - 1);
        string currentButtonText = isLastHero ? closeLabel : nextLabel;

        if (nextButtonTMP != null)
        {
            nextButtonTMP.text = currentButtonText;
        }
        else if (nextButtonLegacyText != null)
        {
            nextButtonLegacyText.text = currentButtonText;
        }
    }

    /// <summary>
    /// Xử lý sự kiện khi ấn nút Next / Đóng
    /// </summary>
    public void OnNextButtonClicked()
    {
        if (heroTutorials == null || heroTutorials.Count == 0)
        {
            CloseTutorial();
            return;
        }

        if (currentIndex < heroTutorials.Count - 1)
        {
            // Sang tướng tiếp theo
            currentIndex++;
            DisplayHeroAtIndex(currentIndex);
        }
        else
        {
            // Tướng cuối cùng -> Tắt bảng
            CloseTutorial();
        }
    }

    /// <summary>
    /// Tự động tìm kiếm các UI Component dựa theo cây Hierarchy nếu chưa được kéo thả
    /// </summary>
    [ContextMenu("Tự động tìm UI Components")]
    public void AutoFindUIComponents()
    {
        // 1. Tìm Bảng HuonngDan / BoardMenu
        if (tutorialBoard == null)
        {
            GameObject huonngDanObj = GameObject.Find("HuonngDan");
            if (huonngDanObj != null)
            {
                tutorialBoard = huonngDanObj;
            }
            else
            {
                tutorialBoard = gameObject;
            }
        }

        Transform boardTransform = tutorialBoard != null ? tutorialBoard.transform : transform;

        // 2. Tìm Animator trên BoardMenu
        if (boardMenuAnimator == null)
        {
            Transform boardTF = FindDeepChild(boardTransform, "BoardMenu");
            if (boardTF != null)
            {
                boardMenuAnimator = boardTF.GetComponent<Animator>();
            }
            else
            {
                boardMenuAnimator = boardTransform.GetComponentInChildren<Animator>();
            }
        }

        // 3. Tìm Player Image
        if (playerImage == null)
        {
            Transform playerTr = FindDeepChild(boardTransform, "Player");
            if (playerTr != null) playerImage = playerTr.GetComponent<Image>();
        }

        // 4. Tìm VideoPlayer
        if (videoPlayer == null)
        {
            Transform videoTr = FindDeepChild(boardTransform, "VideoHuongDan");
            if (videoTr != null)
            {
                videoPlayer = videoTr.GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = videoTr.gameObject.AddComponent<VideoPlayer>();
                }
            }
        }

        // 5. Tìm Description Text
        if (descriptionTMP == null && descriptionLegacyText == null)
        {
            Transform descTr = FindDeepChild(boardTransform, "Description");
            if (descTr != null)
            {
                descriptionTMP = descTr.GetComponent<TextMeshProUGUI>();
                if (descriptionTMP == null)
                {
                    descriptionLegacyText = descTr.GetComponent<Text>();
                }
            }
        }

        // 6. Tìm Next Button & Text
        if (nextButton == null)
        {
            Transform nextTr = FindDeepChild(boardTransform, "Next");
            if (nextTr != null)
            {
                nextButton = nextTr.GetComponent<Button>();
                if (nextButton != null)
                {
                    nextButtonTMP = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (nextButtonTMP == null)
                    {
                        nextButtonLegacyText = nextButton.GetComponentInChildren<Text>();
                    }
                }
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}
