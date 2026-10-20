using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Thẻ "Nhận vật phẩm" ở Scene 1: hiện sau lựa chọn tốt có thưởng vật phẩm, chờ người chơi bấm.
/// </summary>
public static class ItemRewardUI
{
    public static IEnumerator ShowAndWait(ItemCatalog.Entry item)
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("ItemReward", 500);
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(900f, 520f), "NHẬN VẬT PHẨM", RuntimeUI.HeaderOrange, out RectTransform body);

        if (item.icon != null)
        {
            RectTransform iconRt = RuntimeUI.CreateRect(body, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            iconRt.sizeDelta = new Vector2(170f, 170f);
            iconRt.anchoredPosition = new Vector2(110f, 45f);
            Image img = RuntimeUI.AddImage(iconRt, Color.white, false);
            img.sprite = item.icon;
            img.preserveAspect = true;
        }

        float textLeft = item.icon != null ? 220f : 20f;
        RectTransform nameArea = RuntimeUI.CreateRect(body, "Name", new Vector2(0f, 1f), Vector2.one, new Vector2(textLeft, -70f), new Vector2(-20f, -10f));
        var nameText = RuntimeUI.CreateText(nameArea, item.displayName, 40f, RuntimeUI.HeaderOrange, TMPro.TextAlignmentOptions.MidlineLeft);
        nameText.fontStyle = TMPro.FontStyles.Bold;

        RectTransform effectArea = RuntimeUI.CreateRect(body, "Effect", Vector2.zero, Vector2.one, new Vector2(textLeft, 100f), new Vector2(-20f, -80f));
        RuntimeUI.CreateText(effectArea, item.effectText + "\nDùng trong trận hôm nay.", 28f, RuntimeUI.TextDark, TMPro.TextAlignmentOptions.TopLeft);

        bool closed = false;
        Button button = RuntimeUI.CreateButton(body, "Cất vào túi", RuntimeUI.HeaderOrange, new Vector2(300f, 70f), () => closed = true);
        RectTransform btnRt = (RectTransform)button.transform;
        btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 35f);

        yield return RuntimeUI.PopIn(group, card);
        while (!closed) yield return null;
        yield return RuntimeUI.FadeOut(group);

        Object.Destroy(canvas.gameObject);
    }
}
