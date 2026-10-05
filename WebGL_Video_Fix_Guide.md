# Hướng Dẫn Sửa Lỗi VideoPlayer Phát Video Trên Unity WebGL

Tài liệu này tổng hợp toàn bộ nguyên nhân, giải pháp và mã nguồn để xử lý triệt để lỗi **VideoPlayer không chạy được, bị cháy trắng hoặc bị đơ giao diện trên nền tảng Unity WebGL**.

---

## 1. Nguyên Nhân Cốt Lõi Khi Phát Video Trên WebGL

1. **Unity WebGL không mã hóa được `VideoClip` nhúng nội bộ**:
   - Trên PC/Android, Unity dùng bộ giải mã OS để đọc file `.mp4` nhúng trong build.
   - Trên **WebGL**, Unity WebGL **không hỗ trợ** phát `VideoClip` nhúng trực tiếp trong file binary `.wasm`. Trình duyệt HTML5 bắt buộc phải nạp video thông qua đường dẫn HTTP/URL.

2. **Chính Sách Chặn Autoplay Của Trình Duyệt Web (Browser Autoplay Policy)**:
   - Các trình duyệt web hiện đại (Chrome, Safari, Edge, Firefox) mặc định **chặn** mọi video tự động phát nếu video đó có phát ra âm thanh mà người dùng chưa tương tác nhấp chuột với trang web.

3. **Vòng Lặp Chờ Vô Hạn Làm Đơ Game (`while (!vp.isPrepared)`)**:
   - Nếu gọi `vp.Prepare()` nhưng video bị kẹt/chặn trên WebGL mà không đặt thời gian chờ tối đa (Timeout), Coroutine sẽ bị lặp vô hạn, giữ nguyên màn che UI `CanvasGroup` phủ kín màn hình làm liệt toàn bộ nút bấm.

4. **Hiện Tượng Cháy Trắng (White Flash)**:
   - Khi `RenderTexture` vừa được khởi tạo, nó sẽ là một khung hình màu trắng rỗng. Nếu bật `CanvasGroup.alpha = 1` trước khi Video render xong Frame 0, người chơi sẽ nhìn thấy một vệt chói trắng.

---

## 2. Quy Trình Sửa Lỗi Chi Tiết (4 Bước)

### 📁 Bước 1: Chuyển toàn bộ Video `.mp4` vào thư mục `Assets/StreamingAssets/`
- Tạo thư mục `Assets/StreamingAssets/` trong dự án Unity.
- Coppy tất cả các file video `.mp4` vào thư mục này (Ví dụ: `In_ChuyenCanh.mp4`, `Out_ChuyenCanh.mp4`, `HD_HongCau.mp4`, ...).
- **Lý do**: Thư mục `StreamingAssets` khi xuất ra WebGL sẽ tự động biến thành thư mục web tĩnh `Build/StreamingAssets/filename.mp4`, giúp trình duyệt web tải và giải mã video bằng phần cứng HTML5 trực tiếp.

---

### 💻 Bước 2: Cấu Hình `VideoPlayer` Đọc URL Trên WebGL & Tắt Audio
Trong C# Script, viết hàm cấu hình nguồn Video linh hoạt cho cả Editor và WebGL:

```csharp
private void ConfigureVideoSource(VideoPlayer vp, VideoClip clip, string fileName)
{
    if (vp == null) return;

    vp.playOnAwake = false;
    vp.renderMode = VideoRenderMode.RenderTexture;
    
    // ⚠️ QUAN TRỌNG: Tắt Audio của Video để không bị Browser Autoplay Policy chặn!
    vp.audioOutputMode = VideoAudioOutputMode.None;

    #if UNITY_WEBGL && !UNITY_EDITOR
    // Nền tảng WebGL: Đọc trực tiếp từ đường dẫn URL StreamingAssets
    string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
    vp.source = VideoSource.Url;
    vp.url = videoUrl;
    #else
    // Nền tảng Editor / Standalone: Ưu tiên dùng VideoClip
    if (clip != null)
    {
        vp.source = VideoSource.VideoClip;
        vp.clip = clip;
    }
    else
    {
        string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = videoUrl;
    }
    #endif
}
```

---

### ✨ Bước 3: Cơ Chế Chống Cháy Trắng (Zero-Flash Pre-Preparation)
- Giữ `CanvasGroup.alpha = 0f` trong suốt quá trình `vp.Prepare()` nạp video.
- Sau khi `vp.isPrepared` chuẩn bị xong và `vp.Play()` được gọi, chờ 1 frame (`yield return null`) để Frame 0 vẽ lên RenderTexture rồi mới bật `CanvasGroup.alpha = 1f`.

```csharp
// 1. Giữ ẩn 100% lúc bắt đầu nạp
cg.alpha = 0f;
cg.blocksRaycasts = true;

// 2. Chờ nạp video có Timeout an toàn (Ví dụ: max 5 giây)
vp.Prepare();
float timer = 0f;
while (!vp.isPrepared && timer < prepareTimeout)
{
    timer += Time.unscaledDeltaTime;
    yield return null;
}

// 3. Đã nạp xong Frame 0 -> Phát video rồi mới hiện Alpha = 1 (Chống cháy trắng)
vp.Play();
yield return null; // Chờ 1 frame render
cg.alpha = 1f;
```

---

### 🔓 Bước 4: Tự Động Mở Khóa Giao Diện (Unblock Raycasts)
Khi hiệu ứng video kết thúc hoặc bị tắt, bắt buộc phải giải phóng raycast để người chơi thao tác được với các nút bấm giao diện bên dưới:

```csharp
cg.alpha = 0f;
cg.blocksRaycasts = false; // 🔓 Mở khóa cho phép click xuyên qua
panelObject.SetActive(false);
```

---

## 3. Các Script Tham Chiếu Đã Hoàn Thiện Trong Dự Án

1. **[`SceneVideoTransition.cs`](file:///Users/user/Documents/Unity/ProjectUnity/Project_FGU_Build/Assets/_Scrip/Managers/SceneVideoTransition.cs)**:
   - Quản lý hiệu ứng chuyển cảnh mượt mà giữa các Scene với cơ chế Double-Buffering chống nháy và cấu hình WebGL URL tự động.
2. **[`HeroTutorialManager.cs`](file:///Users/user/Documents/Unity/ProjectUnity/Project_FGU_Build/Assets/_Scrip/Managers/HeroTutorialManager.cs)**:
   - Quản lý bảng hướng dẫn từng loại tướng kèm Video minh họa hoạt động 100% trên WebGL.

---
*Tài liệu được khởi tạo tự động bởi Antigravity AI Agent.*
