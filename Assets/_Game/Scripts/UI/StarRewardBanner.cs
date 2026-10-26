using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Băng thông báo trên bảng Thắng khi đạt kỷ lục sao mới:
/// "+N sao mới! Về Menu chính → Nâng cấp để tế bào mạnh hơn." (ngôi sao nhún nhảy, chạy cả khi game dừng).
/// </summary>
public class StarRewardBanner : MonoBehaviour
{
    private const float StarSize = 70f;
    private RectTransform starsRow;

    public static void Show(int newStars)
    {
        if (newStars <= 0) return;
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("StarReward", 600);
        canvas.gameObject.AddComponent<StarRewardBanner>().Build(canvas, newStars);
    }

    private void Build(Canvas canvas, int newStars)
    {
        RectTransform banner = RuntimeUI.CreateWarningBanner(canvas.transform, "Banner", new Vector2(1100f, 140f), out RectTransform content);
        // Mép dưới màn hình: bảng Thắng chiếm phần giữa-trên nên không bị che
        banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 0f);
        banner.anchoredPosition = new Vector2(0f, 90f);
        banner.GetComponent<Image>().raycastTarget = false;

        int available = UpgradeProgress.AvailableStars;
        TextMeshProUGUI text = RuntimeUI.CreateText(content,
            $"<b>+{newStars} sao mới!</b> Bạn đang có {available} sao chưa dùng. Về <b>Menu chính → Nâng cấp</b> để tế bào mạnh hơn.",
            28f, BattleHUD.TipColor);
        text.enableAutoSizing = true;
        text.fontSizeMin = 16f;
        text.fontSizeMax = 28f;

        // Ngôi sao nhún nhảy ở mép trái băng
        Sprite star = UISkin.Instance != null ? UISkin.Instance.starFilled : null;
        if (star != null)
        {
            starsRow = RuntimeUI.CreateRect(banner, "Stars", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            starsRow.anchoredPosition = new Vector2(-40f, 0f);
            for (int i = 0; i < Mathf.Min(newStars, 3); i++)
            {
                RectTransform rt = RuntimeUI.CreateCentered(starsRow, "Star" + i, new Vector2(StarSize, StarSize));
                rt.anchoredPosition = new Vector2(-i * (StarSize * 0.7f), 0f);
                Image img = RuntimeUI.AddImage(rt, Color.white, false);
                img.sprite = star;
                img.preserveAspect = true;
            }
        }

        GameSfx.Play(GameSfx.Kind.Reward);
        StartCoroutine(Animate(banner));
    }

    private IEnumerator Animate(RectTransform banner)
    {
        // Trồi từ dưới lên rồi nhún nhẹ liên tục (thời gian thực vì bảng Thắng đặt timeScale = 0)
        Vector2 home = banner.anchoredPosition;
        for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / 0.4f, 3f);
            banner.anchoredPosition = home + Vector2.down * (200f * (1f - k));
            yield return null;
        }
        banner.anchoredPosition = home;

        while (true)
        {
            if (starsRow != null) starsRow.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.unscaledTime * 5f));
            yield return null;
        }
    }
}
