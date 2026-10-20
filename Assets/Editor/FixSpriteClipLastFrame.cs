using UnityEditor;
using UnityEngine;

/// <summary>
/// Sửa lỗi "frame cuối bị nuốt" của animation đổi Sprite.
/// Khi kéo Sprite vào Timeline, Unity đặt độ dài clip = key cuối + 1/60s,
/// nên frame cuối chỉ hiện ~0.017s trong khi các frame khác hiện 0.1–0.2s -> vòng lặp bị giật.
/// Tool này thêm 1 key lặp lại sprite cuối để frame cuối hiện đủ lâu như các frame trước.
/// Chạy nhiều lần vẫn an toàn (clip đã sửa sẽ được bỏ qua). KHÔNG tự chạy khi load script.
/// </summary>
public static class FixSpriteClipLastFrame
{
    private const string AnimationFolder = "Assets/ImageCallOurMyCell/animation";

    [MenuItem("Tools/Animation/Fix Sprite Clip Last Frame")]
    public static void FixAll()
    {
        int fixedCount = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimationFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null && FixClip(clip))
            {
                fixedCount++;
                Debug.Log($"🎞️ [Tools] Đã sửa frame cuối: {path} (độ dài mới {clip.length:0.###}s)");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ [Tools] Fix Sprite Clip Last Frame: đã sửa {fixedCount} clip.");
    }

    private static bool FixClip(AnimationClip clip)
    {
        bool changed = false;
        float frameTime = 1f / clip.frameRate;

        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
        {
            ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (keys == null || keys.Length < 2) continue;

            ObjectReferenceKeyframe last = keys[keys.Length - 1];
            ObjectReferenceKeyframe prev = keys[keys.Length - 2];

            // Đã sửa rồi (2 key cuối cùng sprite) -> bỏ qua
            if (last.value == prev.value) continue;

            // Frame cuối nên hiện lâu bằng khoảng cách giữa 2 frame trước đó
            float interval = last.time - prev.time;
            if (interval <= frameTime * 1.5f) continue;

            ObjectReferenceKeyframe hold = new ObjectReferenceKeyframe
            {
                time = last.time + interval - frameTime,
                value = last.value
            };

            ObjectReferenceKeyframe[] newKeys = new ObjectReferenceKeyframe[keys.Length + 1];
            keys.CopyTo(newKeys, 0);
            newKeys[keys.Length] = hold;

            Undo.RecordObject(clip, "Fix Sprite Clip Last Frame");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, newKeys);
            changed = true;
        }

        if (changed) EditorUtility.SetDirty(clip);
        return changed;
    }
}
