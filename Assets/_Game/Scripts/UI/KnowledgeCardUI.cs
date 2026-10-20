using System.Collections;
using UnityEngine;

/// <summary>
/// Thẻ "Kiến thức y khoa" hiện sau mỗi lựa chọn ở Scene 1.
/// Chặn tương tác phía sau và chờ người chơi bấm "Đã hiểu".
/// </summary>
public static class KnowledgeCardUI
{
    public static IEnumerator ShowAndWait(string tip)
    {
        Canvas canvas = RuntimeUI.CreateOverlayCanvas("KnowledgeCard", 500);
        CanvasGroup group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        RectTransform root = RuntimeUI.Stretch(canvas.transform, "Backdrop");
        RuntimeUI.AddImage(root, RuntimeUI.Backdrop);

        RectTransform card = RuntimeUI.CreateCard(root, new Vector2(1000f, 460f), "KIẾN THỨC Y KHOA",
            RuntimeUI.HeaderGreen, out RectTransform body);

        // Nội dung ở phần trên, nút ở đáy thẻ
        RectTransform textArea = RuntimeUI.CreateRect(body, "TipArea", Vector2.zero, Vector2.one,
            new Vector2(0f, 90f), Vector2.zero);
        RuntimeUI.CreateText(textArea, tip, 34f, RuntimeUI.TextDark, TMPro.TextAlignmentOptions.MidlineLeft);

        bool closed = false;
        var button = RuntimeUI.CreateButton(body, "Đã hiểu", RuntimeUI.HeaderGreen, new Vector2(260f, 70f), () => closed = true);
        RectTransform btnRt = (RectTransform)button.transform;
        btnRt.anchorMin = btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.anchoredPosition = new Vector2(0f, 35f);

        yield return RuntimeUI.PopIn(group, card);
        while (!closed) yield return null;
        yield return RuntimeUI.FadeOut(group);

        Object.Destroy(canvas.gameObject);
    }
}
