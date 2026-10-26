using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIButtonEffect))]
public class LevelButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Scene to Load")]
    public string sceneName;

    [Header("Ngày chơi")]
    [Tooltip("Số ngày của nút này. Để 0 = tự lấy theo thứ tự nút trong nhóm (nút đầu tiên = Ngày 1)")]
    public int day = 0;

    private static readonly Color LockedTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    public int Day => day > 0 ? day : transform.GetSiblingIndex() + 1;

    // Ngày chơi được khi đã mở khóa và đã có nội dung (Resources/Days/Day{n})
    private bool IsPlayable => GameProgress.IsUnlocked(Day) && GameProgress.GetDayConfig(Day) != null;

    private void Start()
    {
        // Vừa có sao chưa dùng (sau trận thắng đầu tiên) -> giải thích sao & nâng cấp 1 lần
        UpgradeTutorialUI.TryShowOnMap();

        // Ngày chưa mở khóa / chưa có nội dung -> làm tối nút
        if (!IsPlayable)
        {
            foreach (var img in GetComponentsInChildren<Image>())
            {
                img.color = img.color * LockedTint;
            }
            return;
        }

        ShowBestStars();
    }

    /// <summary>3 ngôi sao nhỏ dưới nút: số sao tốt nhất đã đạt ở ngày này.</summary>
    private void ShowBestStars()
    {
        UISkin skin = UISkin.Instance;
        if (skin == null || skin.starFilled == null || skin.starEmpty == null) return;

        int best = GameProgress.GetBestStars(Day);
        const float starSize = 46f;
        RectTransform row = RuntimeUI.CreateRect(transform, "BestStars", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
        row.anchoredPosition = new Vector2(0f, 6f);
        for (int i = 0; i < 3; i++)
        {
            RectTransform starRt = RuntimeUI.CreateCentered(row, "Star" + i, new Vector2(starSize, starSize));
            starRt.anchoredPosition = new Vector2((i - 1) * (starSize + 4f), 0f);
            Image img = RuntimeUI.AddImage(starRt, Color.white, false);
            img.sprite = i < best ? skin.starFilled : skin.starEmpty;
            img.preserveAspect = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameSfx.Play(GameSfx.Kind.UIClick);
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty on " + gameObject.name);
            return;
        }

        if (GameProgress.GetDayConfig(Day) == null)
        {
            ShowNotice($"Ngày {Day} đang được phát triển, hẹn bạn sớm nhé!");
            return;
        }

        if (!GameProgress.IsUnlocked(Day))
        {
            ShowNotice($"Hãy chiến thắng Ngày {Day - 1} để mở khóa Ngày {Day}!");
            return;
        }

        GameProgress.CurrentDay = Day;
        SceneChuyenCanhManager.LoadScene(sceneName);
    }

    private static GameObject noticeCanvas;

    private void ShowNotice(string message)
    {
        if (noticeCanvas != null) Destroy(noticeCanvas);

        Canvas canvas = RuntimeUI.CreateOverlayCanvas("LevelNotice", 500);
        noticeCanvas = canvas.gameObject;
        // Băng cảnh báo CanhBao phía dưới màn hình
        RectTransform box = RuntimeUI.CreateWarningBanner(canvas.transform, "Notice", new Vector2(1200f, 180f), out RectTransform content);
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0f);
        box.anchoredPosition = new Vector2(0f, 160f);
        var text = RuntimeUI.CreateText(content, message, 30f, RuntimeUI.TextDark);
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 28f;

        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        StartCoroutine(FadeNotice(group, canvas.gameObject));
    }

    private static IEnumerator FadeNotice(CanvasGroup group, GameObject canvasObject)
    {
        yield return new WaitForSecondsRealtime(2.2f);
        if (group == null) yield break;
        yield return RuntimeUI.FadeOut(group, 0.3f);
        if (canvasObject != null) Object.Destroy(canvasObject);
    }
}
