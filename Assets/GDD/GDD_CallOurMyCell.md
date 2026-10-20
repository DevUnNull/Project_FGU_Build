# CALL OUT MY CELLS – CHAPTER 1: NGÀY 1

> **Phiên bản tài liệu:** 2.0 – viết lại theo code hiện tại (nhánh `CallOutMyCell`, 20/10/2026)
> **Nền tảng:** Unity (URP 2D), build WebGL
> **Thể loại:** Visual Novel lựa chọn lối sống + Tower Defense theo làn (kiểu Plants vs Zombies)

Tài liệu mô tả **đúng những gì đang chạy trong project**. Những chỗ thiết kế chưa thống nhất hoặc code có lỗi được ghi ở **Mục 8 – Vấn đề đã biết**, để không bị lẫn với phần mô tả.

---

## 1. TỔNG QUAN

### 1.1. Ý tưởng cốt lõi
Người chơi sống một ngày của nhân vật chính (sinh viên tên **Linh**). Mỗi lựa chọn sinh hoạt (ngủ, ăn, uống, đi chơi…) làm thay đổi **5 chỉ số cơ thể**. Đến tối, mầm bệnh tấn công. Người chơi "vào trong cơ thể" và chỉ huy các tế bào miễn dịch phòng thủ. Sức mạnh của đội tế bào phụ thuộc trực tiếp vào lối sống ban ngày.

**Thông điệp:** lối sống hằng ngày quyết định sức đề kháng của cơ thể.

### 1.2. Luồng scene

| # | Scene | Vai trò |
|---|---|---|
| 0 | `MainMenuScene` | Menu chính |
| 1 | `MainMap` | Bản đồ chọn màn (nút màn → `Scene1_LifeSimulation`) |
| 2 | `Scene1_LifeSimulation` | Visual Novel – một ngày sinh hoạt, chọn lựa → chỉnh chỉ số |
| 3 | `Scene2_Transition` | Màn tổng kết trạng thái cơ thể (Tốt / Xấu) |
| 4 | `Ingame_1` | Tower Defense – tế bào chống mầm bệnh |

Chuyển scene đi qua `SceneVideoTransition` (video In/Out, đã tối ưu WebGL qua StreamingAssets). `SceneChuyenCanhManager` chỉ là lớp bọc (wrapper) gọi sang nó.

> Toàn bộ scene của dự án nằm trong `Assets/_ScenesCallOurMyCell` (gồm cả `LuTile`, hiện chưa có trong Build Settings). Các scene ở thư mục khác (`GUIPackCartoon/Demo`, `_Recovery`, `Settings/Scenes`) là của package/template, không thuộc game.

---

## 2. HỆ THỐNG CHỈ SỐ CƠ THỂ (`StomachDayData`)

`StomachDayData` là singleton `DontDestroyOnLoad`. Nó được `ScenarioPlayer` reset khi bắt đầu Scene 1, và được Scene 2 cùng Scene 3 đọc lại.

| Chỉ số | Biến | Mặc định | Giới hạn | Tác động gameplay |
|---|---|---|---|---|
| **Immunity** (Kháng thể) | `immunityMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **Máu và Sát thương** của mọi tế bào |
| **Energy** (Thể lực) | `energyMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **ATP khởi điểm** và **tốc độ hồi ATP** |
| **Hydration** (Nước) | `hydrationMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **tốc độ đánh** của Tế bào B và **tốc độ rơi xu** của Hồng Cầu (tướng có `affectedByHydration`) |
| **Recovery** (Hồi phục) | `recoveryMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **Máu Cơ Thể** (số mạng ở Trái Tim) |
| **Toxicity** (Độc tố) | `toxicityLevel` | 0 | ≥ 0 | **Số ô bị khóa** = `round(Toxicity)`. Nếu ≥ 3 thì kích hoạt **Sóng Axit** |

**Điểm sức khỏe** (`StomachDayData.GetHealthScore()`):
`(Immunity − 1) + (Energy − 1) + (Hydration − 1) + (Recovery − 1) − 0.1 × Toxicity`.
Điểm > 0 là khỏe hơn bình thường, < 0 là yếu hơn. Scene 2 dùng điểm này để chấm trạng thái.

### 2.1. Quy đổi giá trị impact (`ScenarioPlayer.ApplyImpactToStomachData`)
Trong `ScenarioData`, mỗi lựa chọn có danh sách `impacts` (chỉ số + giá trị). Khi người chơi chọn, giá trị được quy đổi rồi cộng vào chỉ số tương ứng:

- **Immunity / Energy / Hydration / Recovery:** nếu `|giá trị| ≥ 1` thì chia 100, tức là nhập theo **phần trăm**. Ví dụ `+15` cho ra `+0.15`.
- **Toxicity:** luôn chia 10 (nhập theo đơn vị ×10). Ví dụ `+30` cho ra `+3` ô, `+5` cho ra `+0.5`.

Mỗi impact hiện một popup (`StatImpactPopup`). Các popup cách nhau 0.18s, mỗi cái cao hơn cái trước 45px.

---

## 3. SCENE 1 – MỘT NGÀY CỦA LINH (VISUAL NOVEL)

**Dữ liệu:** `Assets/_Scrip/Data/Day1_v1.asset` (`ScenarioData`), chỉnh bằng **Scenario Graph Editor** (`VisualNovelSystem/Editor/ScenarioGraph`).
**Runtime:** `ScenarioPlayer`. Hỗ trợ 2 nhân vật (pose, biểu cảm, tóc), nền ảnh hoặc video, thoại tự chạy hoặc chờ click, và tự chuyển cảnh khi tình huống không có lựa chọn.

### 3.1. Cây kịch bản

```
Start ("Ngày 1")
 └─ CẢNH 1 – 06:30 Tiếng chuông
     ├─ 1A Dậy tập giãn cơ ──► NHÁNH 2A – 07:00 Ăn sáng
     ├─ 1B Ngủ nướng ───────► 2B (7h15 trễ giờ) ─► NHÁNH 2B – Chạy vội
     └─ 1C Thức trắng đêm ──► NHÁNH 2C – Cần chất kích thích
                                   │
                    (mọi nhánh 2 hội tụ)
                                   ▼
 CẢNH 3 – 11:30 Lớp học & tin bùng dịch (Cúm A, vi khuẩn hô hấp)
                                   ▼
 CẢNH 4 – 18:30 Lời rủ đi nhậu & bữa tối
                                   ▼
 CẢNH 5 – 23:30 Báo động sinh học (rát họng, sốt)
                                   ▼
 "Có nên uống thuốc không?" ─► END ─► Scene2_Transition
```

### 3.2. Bảng lựa chọn và tác động

| Cảnh | Lựa chọn | Impact nhập | Thay đổi thực tế |
|---|---|---|---|
| **1 – 06:30** | A. Bật dậy, giãn cơ | Energy +15, Immunity +10, Toxicity −5, Recovery +10 | E +0.15, I +0.10, T −0.5, R +0.10 |
| | B. Tắt chuông ngủ tiếp | Energy −10 | E −0.10 |
| | C. Thức trắng cày deadline | Energy −10, Toxicity +30, Immunity −15, Recovery −15 | E −0.10, T +3, I −0.15, R −0.15 |
| **2A – 07:00** | A. Phở tái + trà đá | Immunity +20, Hydration +20 | I +0.20, H +0.20 |
| | B. Bạc xỉu + bánh mì | Energy +10, Toxicity +5 | E +0.10, T +0.5 |
| | C. Đi bộ tới trường | Energy +15, Immunity +10, Hydration −10 | E +0.15, I +0.10, H −0.10 |
| **2B – 07:15** | A. Nhịn ăn, phóng xe | Energy −15, Toxicity +15, Hydration −10 | E −0.15, T +1.5, H −0.10 |
| | B. Bánh mì vừa chạy vừa ăn | Energy +5, Toxicity +10 | E +0.05, T +1 |
| **2C** | A. Nước tăng lực lạnh | Energy +15, Toxicity +25, Immunity −10, Hydration −10 | E +0.15, T +2.5, I −0.10, H −0.10 |
| | B. Cà phê đen đá đậm | Energy +10, Toxicity +15, Hydration −10 | E +0.10, T +1.5, H −0.10 |
| **3 – 11:30** | A. Đeo khẩu trang, hỏi triệu chứng | Immunity +10 | I +0.10 |
| | B. Gạt đi, "đề kháng trâu bò" | Immunity −10, Toxicity +10 | I −0.10, T +1 |
| **4 – 18:30** | A. Từ chối, nấu cơm rau, ngủ sớm | Recovery +20, Immunity +15, Toxicity −20, Hydration +15 | R +0.20, I +0.15, T −2, H +0.15 |
| | B. Đi đồ nướng + bia/nước có gas | Toxicity +25, Immunity −15, Hydration −15, Recovery −10 | T +2.5, I −0.15, H −0.15, R −0.10 |
| | C. Bỏ bữa tối, thức cày xuyên đêm | Toxicity +35, Immunity −25, Recovery −20 | T +3.5, I −0.25, R −0.20 |
| **Kết – 23:30** | A. Uống (thuốc hạ sốt) | Recovery +10, Energy +10 | R +0.10, E +0.10 → END |
| | B. Thôi nằm nghỉ | Recovery +15 | R +0.15 → END |

### 3.3. Kết quả mẫu

| Kịch bản | Immunity | Energy | Hydration | Recovery | Ô khóa | Điểm | Scene 2 | Sóng Axit |
|---|---|---|---|---|---|---|---|---|
| **Tốt nhất** (1A → Phở → Khẩu trang → 4A → Nằm nghỉ) | 1.55 | 1.15 | 1.35 | 1.45 | 0 | +1.5 | Tốt | Không |
| Lưng chừng (1A → Bạc xỉu → Khẩu trang → 4B → Nằm nghỉ) | 1.05 | 1.25 | 0.85 | 1.15 | 3 | 0.0 | Trung bình | Có |
| Ngủ nướng (1B → Nhịn ăn → Khẩu trang → 4B → Nằm nghỉ) | 0.95 | 0.75 | 0.75 | 1.05 | 4 | −0.9 | Xấu | Có |
| **Xấu nhất** (1C → Tăng lực → Gạt đi → 4C → Uống) | 0.40 | 1.15 | 0.90 | 0.75 | 10 | −1.8 | Xấu | Có |

---

## 4. SCENE 2 – TỔNG KẾT (`Scene2Transition`)

- Panel fade in, giữ, fade out, rồi tự load `Ingame_1`.
- Chấm theo **Điểm sức khỏe** (mục 2), ngưỡng chỉnh được trong Inspector:
  - **Tốt** (điểm ≥ 0.3): nền xanh, *"Cơ thể sung sức! Hệ miễn dịch sẵn sàng chiến đấu."*
  - **Trung bình** (−0.3 ≤ điểm < 0.3): nền vàng cam, *"Cơ thể tạm ổn... nhưng hệ miễn dịch đang có điểm yếu."*
  - **Xấu** (điểm < −0.3): nền đỏ, *"Cơ thể kiệt quệ! Vi khuẩn đang thừa cơ tấn công."*
- Trong `Ingame_1`, `DayReportUI` hiện **Báo cáo tình trạng cơ thể** gồm 4 chỉ số tích cực (%), số ô bị khóa do Toxicity, và nút đóng.

---

## 5. SCENE 3 – TOWER DEFENSE (`Ingame_1`)

### 5.1. Bàn cờ và làn
- **Bàn cờ 5 hàng × 7 cột = 35 ô** (`TileCell` đặt tay trong scene, mỗi ô có `row`/`col`; ô cách nhau ≈ 1.8 × 1.4 đơn vị). Cột 0 không bao giờ bị khóa bởi độc tố.
  - `GridManager.rows/cols/cellSize` (5 × 9, 1.5) là thông số cũ, không còn dùng cho độc tố hay Sóng Axit. Mọi logic theo ô đều đi qua `GridManager.GetBoardTiles()`.
- **5 làn (Path1–Path5).** Quái đi theo waypoint của làn mình (`WaypointMovement`, `PathManager`) từ phải sang trái về phía Trái Tim.
- Quái chạm đích thì trừ **Máu Cơ Thể** (`DestinationEnemy` → `LifeManager`).

### 5.2. Kinh tế ATP (`GoldManager`)
| Thông số | Giá trị (Inspector `Ingame_1`) |
|---|---|
| ATP khởi điểm | `250 × Energy` |
| Hồi ATP tự động | `+5 ATP` mỗi `4 / Energy` giây, **chỉ sau khi bấm GỌI WAVE** |
| Hồng Cầu | Mỗi `5 / Hydration` giây rơi 1 đồng xu, **click để nhặt** +5 ATP |
| Tiêu diệt quái | +30 ATP (`rewardGold`) |
| Bán tế bào | Kéo vào thùng rác, **hoàn 100% giá** (`TrashSellAnimation`) |

### 5.3. Shop và đặt quân
- Shop có **4 ô card**, nút **Roll** quay kiểu máy slot (`SlotMachineReelController`, `RollButtonAnimation`) để random tế bào từ `ShopSpawner.unitPrefabs`.
- **Kéo thả** (`DragAndDrop`) card lên ô trống. Đủ ATP thì trừ tiền và tế bào ra sân. Không thể đặt lên ô bị khóa hay ô đã có quân.
- Lần đầu vào trận có bảng **Hướng dẫn tướng** (`HeroTutorialManager`), game tạm dừng trong lúc xem.

### 5.4. Tế bào phe ta (`HeroData` – `Assets/_Scrip/Hero/Resources`)

| Tế bào | Prefab | Giá | Máu | Sát thương | Nhịp đánh | Vai trò | Chỉ số ảnh hưởng |
|---|---|---|---|---|---|---|---|
| **Hồng Cầu** | `CellHongCau` | 75 | 100 | 0 | – | Kinh tế: rơi xu ATP mỗi 5s | Immunity (máu), Hydration (tốc độ rơi xu) |
| **Bạch Cầu** | `CellBachCau` | 100 | 120 | 25 | 1.0s | **Cận chiến** (tầm 1.5). Prefab có `ProjectileLauncher` nhưng `HeroAttackState` ép đánh gần theo tên | Immunity |
| **Tế bào B** | `CellB` | 125 | 90 | 25 | 0.45s | Xạ thủ bắn nhanh (đạn tự dẫn) | Immunity, Hydration |
| **Đại Thực Bào** | `CellDaiThucBao` | 100 | 400 | 5 | 1.5s | Tanker chặn đường, đánh theo hàng | Immunity |

Khi đặt quân, `HeroBase.SetFromData()` tính:
`Sát thương = gốc × Immunity`, `Máu = gốc × Immunity`, `Nhịp đánh = gốc / Hydration` (chỉ với tướng tích `HeroData.affectedByHydration`: hiện là Tế bào B và Hồng Cầu).
Hồng Cầu rơi xu mỗi `5 / Hydration` giây.

### 5.5. Mầm bệnh (`EnemyData` – `Assets/_Scrip/Enemies/Infomation`)

| Mầm bệnh | EnemyType | Máu | Sát thương | Tốc độ | Cơ chế đặc biệt |
|---|---|---|---|---|---|
| **Virus Cúm A** | `Corona` | 150 | 15 | 1 | **Bắn xa** (tầm 7) vào tế bào cùng hàng, đứng lại bắn khi có mục tiêu |
| **Vi khuẩn E.coli** | `Ecoli` | 100 | 10 | 2 | **Cảm tử:** chạm tế bào hoặc bị giết thì nổ AoE 25 dmg (bán kính 1.8), để lại **Vũng Axit** 5 dmg/s trong 8s (bán kính 1.5) |
| **Phế Cầu** | `Staphyloccot` | 300 | 25 | 1 | **Cận chiến:** dừng lại, lao tới đánh rồi lùi về (lunge) |

### 5.6. Cấu trúc wave (`WaveManager`)
- Mỗi làn có một `TuneConfig` (`TuneConfigMap1_1…5`) gồm 7 lượt. Năm làn chạy **song song**, nghỉ **30s** giữa các lượt (có nút Skip).
- Lượt trống (`isEmptyWave`) không sinh quái. **Bộ đếm wave** trên UI tính theo lượt chung: một lượt có ít nhất một làn sinh quái thì được tính là 1 wave (Map 1 có 7 wave).

| Lượt | Làn 1 | Làn 2 | Làn 3 | Làn 4 | Làn 5 |
|---|---|---|---|---|---|
| 1 | 2 Cúm A | – | – | – | – |
| 2 | – | 1 Cúm A | 1 Cúm A | – | – |
| 3 | 2 Cúm A | 3 Cúm A | – | – | – |
| 4 | 1 E.coli | 1 E.coli | – | 1 E.coli | – |
| 5 | 1 E.coli | 1 E.coli | 2 Cúm A | 2 E.coli | 2 Cúm A |
| 6 | 2 Phế Cầu | – | 1 Phế Cầu | 2 E.coli | – |
| 7 (cuối) | 2 Cúm A, 1 E.coli, 2 Phế Cầu | 1 Cúm A, 2 E.coli, 1 Phế Cầu | 1 Phế Cầu | 2 Cúm A, 2 E.coli | 2 Phế Cầu |

**Tổng cộng:** 18 Cúm A, 14 E.coli, 9 Phế Cầu = **41 mầm bệnh**. (Lượt 3 của làn 4, `WaveConfig4_2`, vẫn để trống vì dùng EnemyType `Normal` chưa có prefab.)

### 5.7. Độc tố và sự kiện xấu
- **Ô Độc tố:** `GridManager.SpawnToxinObstacles()` khóa ngẫu nhiên `round(Toxicity)` ô thật còn trống (trừ cột 0, tối đa 30 ô). Ô bị khóa được đánh dấu là đã chiếm, nên **không thể đặt tế bào lên**. Hình ảnh là vũng axit (`toxinObstacleSprite` = `baiAxit_0`, cỡ 1.5).
- **Sóng Axit Dạ Dày:** nếu `Toxicity ≥ 3` (`gastricAcidToxicityThreshold`), **một lần duy nhất mỗi trận**, khi làn đầu tiên bắt đầu **lượt thứ 2**, một luồng axit quét **hàng đang có nhiều tế bào nhất** (hòa thì chọn ngẫu nhiên, sân trống thì hàng giữa) trong 10s, gây 10 dmg mỗi giây cho mọi tế bào trên hàng đó (`WaveManager.SpawnGastricAcidWave`, các thông số chỉnh được trong Inspector).
  - Trước khi gây sát thương có **2s cảnh báo**: dải axit xanh nhấp nháy phủ hàng đó kèm chữ "AXIT DẠ DÀY TRÀN!" (`GastricAcidWaveEffect`), để người chơi kịp bán hoặc dời tế bào. Có thể gán âm thanh vào `gastricAcidSound`.

### 5.8. Thắng / Thua / Sao
- **Máu Cơ Thể:** `10 × Recovery` (làm tròn). Mỗi quái tới đích trừ máu.
- **Thua:** Máu Cơ Thể = 0 (`GameOverHandler.ShowGameOver`).
- **Thắng:** hết wave cuối của mọi làn và không còn quái trên sân (`ShowVictory`).
- **Sao** (`StarRatingCalculator`): **3★** khi không mất máu, **2★** khi còn ≥ 50% máu, **1★** khi còn < 50% máu.

---

## 6. KIẾN TRÚC KỸ THUẬT

| Nhóm | Script chính | Ghi chú |
|---|---|---|
| Dữ liệu xuyên scene | `StomachDayData` | Singleton, DontDestroyOnLoad |
| Visual Novel | `ScenarioPlayer`, `ScenarioData`, `StatImpactPopup`, `ScenarioGraphWindow` | Editor dạng GraphView, có validator |
| Chuyển scene | `SceneVideoTransition`, `SceneChuyenCanhManager`, `ChangeScene` | Video In/Out |
| Bàn cờ | `GridManager`, `TileCell` | Highlight ô, khóa ô độc tố |
| Kinh tế / Shop | `GoldManager`, `ShopSpawner`, `DragAndDrop`, `SlotMachineReelController`, `TrashSellAnimation` | |
| Tế bào | `HeroBase` / `_Hero`, `HeroStateMachine` (Idle/Attack), `LineHeroStateMachine`, `ProjectileLauncher`, `HomingProjectile`, `HongCauCoinSpawner` | State pattern |
| Mầm bệnh | `_Enemy`, `EnemyBase`, `EnemyCumA`, `EnemyEcoli`, `EnemyPheCau`, `AcidPuddle`, `EnemyProjectile`, `WaypointMovement` | Object pool: `MultiEnemyPool` + `PoolConfig_Virus` |
| Wave | `WaveManager`, `TuneConfig`, `WaveConfig` | |
| Điểm / kết thúc | `LifeManager` (Observer), `StarRatingCalculator` (Strategy), `GameOverHandler` | |
| UI | `DayReportUI`, `Scene2Transition`, `WaveCountDisplay`, `HeroHealthUI`, `LifeUI` | |

---

## 7. ĐỊNH HƯỚNG NỘI DUNG
- **Giọng văn:** đời thường, hài hước kiểu sinh viên Việt ("cày deadline", "bùng kèo").
- **Liên kết cốt truyện → gameplay:** Cảnh 3 báo trước dịch Cúm A và vi khuẩn đường hô hấp, đúng với đội hình quái ở Scene 3 (Cúm A, Phế Cầu).
- **Mục tiêu giáo dục:** người chơi thấy ngay hậu quả. Ví dụ thức khuya làm tế bào yếu và bàn cờ bị khóa ô, ăn uống lành mạnh làm tế bào khỏe hơn.

---

## 8. VẤN ĐỀ ĐÃ BIẾT (CẦN QUYẾT ĐỊNH / SỬA)

**Đã sửa (20/10/2026):** Sóng Axit không gây sát thương, quy đổi Toxicity không nhất quán, Sóng Axit kích hoạt khi Toxicity chỉ > 0 và có thể chạy nhiều lần, Hydration chọn tướng theo tên GameObject (Bạch Cầu bị buff nhầm, Hồng Cầu không được buff), làn 4 không có quái, lỗi chính tả "kiệt quệ".

**Đã sửa đợt 2 (20/10/2026):** Sóng Axit quét hàng cố định, bộ đếm wave cộng dồn theo làn, Scene 2 chỉ có 2 mức, Cảnh 3 và cảnh kết không có impact, Hydration/Recovery ít được dùng, chưa có giới hạn trên cho chỉ số.

**Đã sửa đợt 3 (20/10/2026):** ô độc tố không chặn đặt quân, spawn lệch khỏi ô thật và vô hình (object `DummyToxin` rỗng). Sóng Axit dò hàng bằng tọa độ sai và không có hiệu ứng báo hiệu.

**Đã sửa đợt 4 (20/10/2026):** gỡ `TileCell` + collider gắn nhầm trên object cha "Tiles" (một ô giả nằm ngoài bàn cờ, có thể nhận quân). Sửa tool `SetupSceneTiles` để không gắn nhầm lại. Tắt chế độ tự chạy khi recompile của `SetupPerfectTileGrid` (nó tự xếp lại vị trí mọi ô theo thông số cứng, ghi đè vị trí chỉnh tay); menu "Tools/Re-align All Tiles to Perfect Grid" vẫn dùng được.

**Đã sửa đợt 5 – Animation (20/10/2026):**
- Frame cuối của 12 clip đổi sprite chỉ hiện 1/60s (Unity đặt độ dài clip = key cuối + 1/60s), nên mỗi vòng lặp bị giật. Đã thêm key giữ frame cuối bằng tool **Tools → Animation → Fix Sprite Clip Last Frame** (`Assets/Editor/FixSpriteClipLastFrame.cs`). **Mỗi khi thêm clip mới, chạy lại tool này.**
- Đòn đánh của tế bào trước đây bắn cứng ở 0.25s và kết thúc ở 0.42s, không theo clip. Giờ bắn ở 60% và kết thúc khi hết clip tấn công thật, có tính cả tốc độ Animator (Hydration).
- Bỏ các `Debug.Log` chạy ở tần suất trận đánh (mỗi phát bắn, mỗi nhịp axit, mỗi lần đổi state), tránh khựng khung hình trên WebGL.

**Đã sửa đợt 6 – Hiệu ứng & Pool (20/10/2026):**
- **Pool tái sử dụng quái đã chết mà không reset máu** (pool có 10 con/loại, trận có 18 Cúm A, 14 E.coli). Quái tái sử dụng xuất hiện với máu ≤ 0, tế bào bỏ qua nên chúng đi thẳng tới Trái Tim; E.coli tái sử dụng không bao giờ nổ. Đã reset máu, trạng thái chết và cờ nổ trong `OnEnable`.
- **E.coli bị bắn chết không phát animation chết và không nổ** (bị trả về Pool ngay, cắt ngang coroutine nổ). Giờ E.coli tự phát `die` → nổ → về Pool.
- Chặn quái "chết hai lần" (cộng tiền hai lần) khi đạn bay tới trong lúc đang chết.
- Thêm hiệu ứng `UnitJuice` (`Assets/_Scrip/Effects/UnitJuice.cs`): tế bào và quái **nháy đỏ + giật nhẹ khi trúng đòn**; tế bào **nảy khi được đặt xuống** và **vươn người khi ra đòn**; quái thường **mờ dần + thu nhỏ trong 0.35s khi chết**. Tế bào khi chết ngừng mọi hành động, ẩn thanh máu, đứng hình rồi mờ dần 0.4s trước khi bị xóa.

**Lưu ý về Editor tools:** các script trong `Assets/Editor` có `[InitializeOnLoadMethod]` (`SetupSceneTiles`, `SetupHeroPrefabs`, `SetupVirusPrefabs`, `SetupAnimatorController`, `FixSpritePivots`, `AlignGridTiles`) **tự chạy mỗi lần Unity biên dịch script** và có thể sửa scene/asset đang mở. Khi thấy file thay đổi lạ trong git, hãy kiểm tra các tool này trước.

**Còn lại:**
- **Thiếu animation (cần họa sĩ):** Cúm A chỉ có clip `Run` (bắn đạn nhưng vẫn chạy hoạt ảnh di chuyển); Đại Thực Bào chỉ có 1 clip (`New Animation`), không có đòn đánh; hầu hết clip chỉ 3–4 frame.
- **Hydration tăng tốc cả animation đứng yên** (`anim.speed` áp cho toàn Animator), không chỉ đòn đánh.
- **Sóng Axit chưa có âm thanh** (trường `gastricAcidSound` đang để trống).
- **Asset thừa còn lại:** các EnemyType không dùng trong enum (`Normal`, `Fast`, `Tank`, `Minotaur`, `Salmonella`). Đã xóa 11 prefab tướng game cũ, 10 `HeroData` tương ứng và `test.asset`.
- **Đặt tên** (giữ nguyên theo quyết định, tránh phải sửa Build Settings): "Call**Our**MyCell" so với tên game "Call **Out** My Cells".

---

## 9. BACKLOG ĐỀ XUẤT
- [x] Sửa các lỗi gameplay chính (Sóng Axit, Toxicity, Hydration, làn 4).
- [ ] Cân bằng lại độ khó sau khi bật làn 4 (+9 quái).
- [x] Giới hạn chỉ số 0.1 – 2.0.
- [ ] Chơi thử kịch bản xấu nhất (Immunity 0.4, 10 ô khóa) xem còn thắng được không.
- [x] Thêm impact cho Cảnh 3 và cảnh kết, tăng số lựa chọn liên quan đến Hydration và Recovery.
- [x] Hiệu ứng hình ảnh cho Sóng Axit.
- [ ] Âm thanh cho Sóng Axit.
- [ ] Thêm dòng "Kiến thức y khoa" sau mỗi lựa chọn, kèm nguồn tham khảo.
- [x] Màn tổng kết 3 mức.
- [ ] Hiển thị chỉ số thay đổi so với đầu ngày ở màn tổng kết.
- [ ] Tăng độ khó dần cho các màn tiếp theo trên `MainMap`.
