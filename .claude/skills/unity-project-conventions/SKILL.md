---
name: unity-project-conventions
description: Quy ước vị trí thư mục và đặt tên cho project Unity "Call Out My Cells". Dùng TRƯỚC KHI tạo, đổi tên hoặc di chuyển bất kỳ file nào trong Assets/ — script C#, ScriptableObject, prefab, scene, sprite, animation, âm thanh, video, editor tool, tài liệu. Kích hoạt khi người dùng nói "tạo script", "thêm class", "tạo prefab", "thêm asset", "làm tính năng mới", "thêm ngày chơi", "thêm wave", "thêm tế bào/virus", "đặt file ở đâu", "đổi tên file", "create script", "add asset", "new feature", "where should this go".
---

# Quy ước project Call Out My Cells

Mọi nội dung của game nằm trong `Assets/_Game/`. Trước khi tạo file, xác định **loại** file (bảng 1), rồi **hệ thống** của game mà nó thuộc về (bảng 2), sau đó đặt tên theo mục 3.

## 1. File đặt ở đâu

| Loại file | Thư mục |
|---|---|
| Scene | `Assets/_Game/Scenes/` |
| Script chạy trong game | `Assets/_Game/Scripts/<Hệ thống>/` (bảng 2) |
| Script chỉ dùng trong Editor (`using UnityEditor`, `[MenuItem]`, custom inspector) | `Assets/_Game/Scripts/Editor/Tools/`, hoặc `Assets/_Game/Scripts/Editor/Narrative/` nếu thuộc Visual Novel |
| ScriptableObject (dữ liệu) | `Assets/_Game/Data/<Loại>/`, gồm `Scenarios`, `Heroes`, `Enemies`, `Waves/Day<n>` |
| Prefab | `Assets/_Game/Prefabs/<Loại>/`, gồm `Heroes`, `Enemies`, `Projectiles`, `UI`, `Narrative` |
| Sprite đơn vị (tế bào, virus) | `Assets/_Game/Art/Sprites/Units/` |
| Sprite đạn, vũng độc | `Assets/_Game/Art/Sprites/Projectiles/` |
| Ảnh UI | `Assets/_Game/Art/UI/<Màn>/`, gồm `Battle`, `Map`, `Menu`, `Icons`, `Common` |
| Ảnh nền | `Assets/_Game/Art/Backgrounds/<Scenario hoặc Battle>/` |
| Nhân vật hội thoại | `Assets/_Game/Art/Characters/` |
| Animation (.anim) và Animator Controller, kèm sprite sheet của nó | `Assets/_Game/Art/Animations/<Heroes, Enemies, Characters hoặc UI>/<Tên>/` |
| Font gốc (.ttf) | `Assets/_Game/Art/Fonts/<Tên font>/` |
| Nhạc nền | `Assets/_Game/Audio/Music/` |
| Hiệu ứng âm thanh | `Assets/_Game/Audio/SFX/` |
| Giọng nói | `Assets/_Game/Audio/Voices/` |
| Video (VideoClip, dùng trong Editor) | `Assets/_Game/Videos/<Tutorials hoặc Transitions>/` |
| Tài liệu (.md) | `Assets/_Game/Docs/` |

## 2. Hệ thống trong `Scripts/`

| Thư mục | Chứa gì | Ví dụ có sẵn |
|---|---|---|
| `Core/` | Dữ liệu xuyên scene, tiến trình, tốc độ, pause | `StomachDayData`, `GameProgress`, `DayConfig`, `GameSpeed` |
| `SceneFlow/` | Chuyển scene | `SceneVideoTransition`, `SceneSlideTransition`, `ChangeScene` |
| `Board/` | Bàn cờ, ô | `GridManager`, `TileCell` |
| `Heroes/` | Tế bào phe ta; có `StateMachine/`, `Projectiles/`, `UI/` | `HeroBase`, `HeroData`, `HeroAttackState` |
| `Enemies/` | Mầm bệnh; có `Core/`, `Behaviours/`, `Config/`, `Managers/`, `Spawning/`, `PopUp/`, `Enums/`, `Interfaces/` | `_Enemy`, `EnemyEcoli`, `WaveManager` |
| `Shop/` | Mua bán, kéo thả, vàng | `DragAndDrop`, `GoldManager`, `ShopSpawner` |
| `Score/` | Mạng, sao, thắng/thua | `LifeManager`, `GameOverHandler` |
| `Narrative/` | Visual Novel (Scene 1) | `ScenarioPlayer`, `ScenarioData` |
| `Progression/` | Tiến trình lâu dài: Sổ tay sức khỏe, nâng cấp bằng sao | `NotebookProgress`, `NotebookCatalog`, `UpgradeProgress` |
| `Items/` | Vật phẩm lối sống và hiệu ứng trong trận | `LifestyleItem`, `ItemCatalog`, `BattleBuffs` |
| `UI/` | Giao diện; có `Effects/`, `Items/`, `Map/`, `Menu/`, `Notebook/`, `Tutorial/` | `BattleHUD`, `RuntimeUI`, `UISkin`, `NotebookUI`, `ItemBarUI` |
| `Audio/` | Âm thanh | `AudioManager`, `GameSfx` |
| `Effects/` | Hiệu ứng dùng chung | `UnitJuice` |
| `Legacy/` | Code cũ còn giữ lại. **Không thêm code mới vào đây** | |

Nếu không hệ thống nào khớp, tạo thư mục hệ thống mới tên PascalCase tiếng Anh, rồi bổ sung vào bảng này và mục 6.1 của `Assets/_Game/Docs/GDD_CallOurMyCell.md`.

## 3. Đặt tên

- **Thư mục:** PascalCase tiếng Anh, số nhiều cho nhóm (`Heroes`, `Prefabs`). Không dấu cách, không tiếng Việt có dấu, không đặt theo tên người làm (không dùng `_HaiImage`).
- **Script:** tên file trùng đúng tên class (Unity bắt buộc), PascalCase. Mỗi file chỉ một class `MonoBehaviour` hoặc `ScriptableObject`.
  - Hậu tố theo vai trò: `...Manager` (singleton quản lý), `...Data` hoặc `...Config` (ScriptableObject), `...UI` (giao diện), `...State` (state machine), `...Effect`, `...Tool` (editor).
  - Interface bắt đầu bằng `I`, ví dụ `ILifeListener`.
- **ScriptableObject asset:** `Loại_Tên` hoặc `LoạiTên_Chỉsố`, ví dụ `Scenario_Day1`, `Cell_HongCau`, `WaveConfigDay2_L1_R1`, `TuneConfigDay2_3`.
- **Prefab:** `Loại_Tên` hoặc `TênĐơnVị`, ví dụ `Enemy_Ecoli`, `Bullet_CellB`, `CellHongCau`.
- **Asset nghệ thuật mới:** tên tiếng Anh hoặc tiếng Việt không dấu, không dấu cách, dạng `PascalCase` hoặc `Tên_TrạngThái`, ví dụ `HongCau_Idle.png`, `Ecoli_Run.anim`. File cũ có tên tiếng Việt có dấu thì **giữ nguyên**, không đổi.
- **Ngày chơi:** `Resources/Days/Day<n>.asset`, `Data/Scenarios/Scenario_Day<n>.asset`, `Data/Waves/Day<n>/`.

## 4. Thư mục đặc biệt: không đổi tên, không di chuyển

| Thư mục | Lý do |
|---|---|
| `Assets/Resources/` | Code gọi `Resources.Load("Days/Day1")`, `"UISkin"`, `"GameSfx"`, `"ItemCatalog"`, `"NotebookCatalog"`, `"PopUp"`, `"HongCauDroppedCoin"`. Chỉ để vào đây asset thật sự cần nạp bằng tên; giữ nguyên đường dẫn con. |
| `Assets/StreamingAssets/` | Video phát qua URL trên WebGL. Video tutorial (`HD_*.mp4`) và chuyển cảnh nằm ở gốc; video kịch bản nằm ở `Scenario/` và được tham chiếu bằng **tên file** trong `ScenarioData` (`dialogueVideoFile`, `videoFile`). |
| Mọi thư mục tên `Editor` | Unity coi code trong đó là editor-only. |
| `Assets/WebGLTemplates/`, `Assets/Plugins/`, `Assets/TextMesh Pro/`, `Assets/Settings/` | Unity hoặc package tìm chúng theo đường dẫn cố định. |
| `Assets/ThirdParty/` | Asset mua hoặc tải về. Không sửa file bên trong; cần chỉnh thì copy sang `_Game`. |
| `Assets/_Legacy/` | Asset prototype cũ. Không lấy ra dùng nếu chưa hỏi người dùng. |

## 5. Cách viết code cho khớp project

- Comment và `<summary>` viết **tiếng Việt**, ngắn gọn, giải thích *vì sao*. Không namespace (project chưa dùng).
- Field hiện trong Inspector: `public` hoặc `[SerializeField] private`, kèm `[Tooltip("...")]` tiếng Việt khi ý nghĩa không hiển nhiên.
- ScriptableObject mới: `[CreateAssetMenu(fileName = "...", menuName = "Game/...")]`.
- Editor tool: `[MenuItem("Tools/<Nhóm>/<Việc>")]`. **Không** dùng `[InitializeOnLoad]` hoặc `[InitializeOnLoadMethod]` để tự sửa scene hay asset, vì chúng chạy lại mỗi lần biên dịch và đã từng ghi đè vị trí ô bàn cờ.
- Input: dùng Input System mới, không gọi `UnityEngine.Input` cũ.
- UI tạo bằng code: dùng `RuntimeUI` (`CreateCard`, `CreateButton`…) và `UISkin`, để có khung CanhBao và nút countWave đồng bộ.
- Âm thanh hiệu ứng: thêm clip vào `Resources/GameSfx.asset`, gọi qua `GameSfx`.
- Danh sách đơn vị đang sống: dùng `HeroBase.Active` hoặc `_Enemy.Active`. Không gọi `FindObjectsOfType` trong `Update`.
- Đường dẫn `"Assets/..."` ghi cứng chỉ được dùng trong code Editor, và phải trỏ đúng cấu trúc ở mục 1.

## 6. Tạo, di chuyển, đổi tên file

1. **Tạo script mới:** viết file `.cs` vào đúng thư mục, để Unity tự sinh `.meta`. Không tự viết hay sửa file `.meta`.
2. **Di chuyển hoặc đổi tên asset đã có:** dùng `AssetDatabase.MoveAsset(old, new)` (qua Unity MCP `run_csharp`) hoặc kéo thả trong cửa sổ Project. Không dùng `mv` hay Finder, vì sẽ mất GUID và làm hỏng tham chiếu trong scene/prefab.
3. **Đổi tên script:** đổi cả tên class lẫn tên file cùng lúc, rồi sửa mọi chỗ code đang dùng class đó.
4. Sau khi di chuyển: `grep -rn '"Assets/'` trong `Assets/_Game/Scripts` để sửa đường dẫn ghi cứng. Nếu là scene thì kiểm tra lại Build Settings.
5. Với asset được game dùng qua `Resources.Load` hoặc qua tên file trong StreamingAssets: giữ nguyên tên, hoặc sửa chuỗi trong code cùng lúc.

## 7. Kiểm tra trước khi báo xong

- [ ] File nằm đúng thư mục theo mục 1–2, tên đúng mục 3.
- [ ] Không có file nào của game nằm ngay `Assets/` gốc hoặc trong thư mục cũ (`_Scrip`, `ImageCallOurMyCell`, `_UI`, `_Prefabs`). Các thư mục cũ này không còn tồn tại; nếu thấy chúng xuất hiện lại thì đó là lỗi.
- [ ] Unity biên dịch không lỗi (kiểm tra bằng Unity MCP, không dùng lệnh `dotnet`).
- [ ] Nếu Unity đã biên dịch lại khi đang mở scene: chạy `git diff` scene đó trước khi lưu, vì tool editor có thể đã tự sửa scene.
- [ ] Thêm hệ thống hoặc thư mục mới thì cập nhật mục 6.1 trong GDD.
