# Nguồn âm thanh (CC0)

Tất cả file dưới đây là **CC0 1.0 (Public Domain)**: dùng tự do, kể cả thương mại, không bắt buộc ghi công. Ghi lại nguồn để nhóm tra cứu và để cảm ơn tác giả. Tải ngày 21/10/2026.

| File trong `Assets/_Game/Audio/SFX/` | Dùng cho | Nguồn (file gốc) |
|---|---|---|
| `Virus_Hit.mp3` | Tế bào đánh trúng mầm bệnh | [Squish sound effects – EZduzziteh](https://opengameart.org/content/squish-sounds-effects) (`squishsplat_impact.mp3`) |
| `Virus_Pop.mp3` | Mầm bệnh bị tiêu diệt | [Squish sound effects – EZduzziteh](https://opengameart.org/content/squish-sounds-effects) (`squishpop.mp3`) |
| `Cell_Hit.ogg` | Tế bào bị đánh | [Impact Sounds – Kenney](https://kenney.nl/assets/impact-sounds) (`impactSoft_medium_001.ogg`) |
| `Cell_Death.ogg` | Tế bào hy sinh | [40 CC0 water / splash / slime SFX – rubberduck](https://opengameart.org/content/40-cc0-water-splash-slime-sfx) (`slime_02.ogg`) |
| `Acid_Bubbles.ogg` | Sóng Axit dạ dày | [40 CC0 water / splash / slime SFX – rubberduck](https://opengameart.org/content/40-cc0-water-splash-slime-sfx) (`loop_bubbles_1.ogg`) |
| `Heart_Beat.wav` | Mầm bệnh lọt tới Trái Tim | [Heartbeat sounds – bart](https://opengameart.org/content/heartbeat-sounds) (`heartbeat_fast_0.wav`) |
| `Item_PowerUp.ogg` | Dùng vật phẩm lối sống | [Digital Audio – Kenney](https://kenney.nl/assets/digital-audio) (`powerUp2.ogg`) |
| `Merge_LevelUp.ogg` | Ghép tế bào lên cấp | [Digital Audio – Kenney](https://kenney.nl/assets/digital-audio) (`powerUp11.ogg`) |
| `Roll_Shake.ogg` | Bấm Roll / khay tự đổi | [Casino Audio – Kenney](https://kenney.nl/assets/casino-audio) (`dice-shake-1.ogg`) |
| `Roll_Lock.ogg` | Từng ô khay dừng quay | [Casino Audio – Kenney](https://kenney.nl/assets/casino-audio) (`chip-lay-1.ogg`) |
| `UI_Click.ogg` | Bấm mọi nút | [Interface Sounds – Kenney](https://kenney.nl/assets/interface-sounds) (`select_002.ogg`) |
| `UI_Open.ogg` | Mở bảng / thẻ | [Interface Sounds – Kenney](https://kenney.nl/assets/interface-sounds) (`maximize_008.ogg`) |
| `UI_Error.ogg` | Thao tác không hợp lệ, cảnh báo | [Interface Sounds – Kenney](https://kenney.nl/assets/interface-sounds) (`bong_001.ogg`) |
| `UI_Toast.ogg` | Thông báo mẹo trong trận | [UI Audio – Kenney](https://kenney.nl/assets/ui-audio) (`rollover6.ogg`) |
| `UI_Reward.ogg` | Nhận vật phẩm, sao, mua nâng cấp | [Interface Sounds – Kenney](https://kenney.nl/assets/interface-sounds) (`confirmation_001.ogg`) |
| `Cell_Place.ogg` | Đặt tế bào xuống ô | [Interface Sounds – Kenney](https://kenney.nl/assets/interface-sounds) (`drop_002.ogg`) |
| `Coin_Collect.ogg` | Nhặt xu ATP, bán tế bào | [Casino Audio – Kenney](https://kenney.nl/assets/casino-audio) (`chips-collide-2.ogg`) |
| `UI_Type.ogg` | Gõ chữ khi thoại hiện ra | [UI Audio – Kenney](https://kenney.nl/assets/ui-audio) (`click2.ogg`) |

Giấy phép: https://creativecommons.org/publicdomain/zero/1.0/

> Tiếng giao diện được chọn theo phổ tần: ưu tiên file trầm (tần số trung bình < 700 Hz, gần như không có năng lượng trên 4 kHz) để không chói tai khi lặp lại nhiều.

## Nhạc nền chill (CC0, `Assets/_Game/Audio/Music/`)

| File | Dùng cho | Nguồn |
|---|---|---|
| `Chill_SlowStride.ogg` (2:11) | Menu chính | [Slow Stride – isaiah658](https://opengameart.org/content/slow-stride) |
| `Chill_LofiLoop.ogg` (2:08) | MainMap → Scene 1 → Scene 2 (liền mạch) | [Lofi Hip Hop Loop – omfgdude](https://opengameart.org/content/lofi-hip-hop-loop) |
| `Chill_LofiAgain.ogg` (1:28) | Dự phòng | [Lofi again – omfgdude](https://opengameart.org/content/lofi-again) |
| `Chill_Sherwood.ogg` (0:35) | Dự phòng | [Sherwood – zesona (Chris Murphy)](https://opengameart.org/content/sherwood) |
| `Chill_Heavenly.ogg` (0:34) | Dự phòng | [Heavenly Loop – isaiah658](https://opengameart.org/content/heavenly-loop) |
| `Battle_TowerDefense.mp3` (3:19) | Nhạc trận (Ingame_1, LuTile) | [Tower Defense Theme – DST](https://opengameart.org/content/tower-defense-theme) |
| `Battle_HappyAdventure.mp3` (0:47) | Dự phòng cho trận | [Happy Adventure (Loop) – TinyWorlds](https://opengameart.org/content/happy-adventure-loop) |

Các tác giả không bắt buộc ghi công nhưng rất cảm kích nếu có (zesona muốn ghi tên "Chris Murphy"; omfgdude có thể ghi "OMF-Games").

Nhạc bộ "FGU theme" có sẵn của nhóm: trận cũ = `BackGroundSoundInGame` (đã thay bằng `Battle_TowerDefense`), thắng = `Win game`, thua = `Lose game` (bài menu/map cũ `Call to Adventure_Menu theme`, `Map theme` vẫn giữ trong thư mục).

Gán lại toàn bộ: **Tools → Audio → Assign Default Game Sounds**. Nhạc trận nằm ở AudioSource của `SceneUI` trong scene trận; đổi bài: kéo clip khác vào ô AudioClip, hoặc chạy **Tools → Battle → Setup Pause Volume Sliders**.
