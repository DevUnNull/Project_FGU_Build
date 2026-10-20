# CALL OUT MY CELLS – CHAPTER 1

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

> Toàn bộ scene của dự án nằm trong `Assets/_Game/Scenes` (gồm cả `LuTile`, hiện chưa có trong Build Settings). Các scene ở thư mục khác (`GUIPackCartoon/Demo`, `_Recovery`, `Settings/Scenes`) là của package/template, không thuộc game.

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

**Dữ liệu:** `Assets/_Game/Data/Scenarios/Scenario_Day1.asset` (`ScenarioData`), chỉnh bằng **Scenario Graph Editor** (`Assets/_Game/Scripts/Editor/Narrative/ScenarioGraph`).
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

- Panel fade in, hiện bảng tổng kết ngày, fade out rồi load `Ingame_1`.
- Chấm theo **Điểm sức khỏe** (mục 2), ngưỡng chỉnh được trong Inspector:
  - **Tốt** (điểm ≥ 0.3): nền xanh, *"Cơ thể sung sức! Hệ miễn dịch sẵn sàng chiến đấu."*
  - **Trung bình** (−0.3 ≤ điểm < 0.3): nền vàng cam, *"Cơ thể tạm ổn... nhưng hệ miễn dịch đang có điểm yếu."*
  - **Xấu** (điểm < −0.3): nền đỏ, *"Cơ thể kiệt quệ! Vi khuẩn đang thừa cơ tấn công."*
- (21/10/2026) Đã bỏ bảng **Báo cáo tình trạng cơ thể** (`DayReportUI`) trong `Ingame_1`: tình trạng cơ thể đã được báo ở Scene 2 (thẻ tóm tắt ngày). Vào trận lần đầu sẽ mở ngay bảng giới thiệu tế bào.
- **Bảng tổng kết ngày** (`DaySummaryUI`): sau thông báo, hiện bảng so sánh 4 chỉ số với đầu ngày (100%) kèm tác dụng trong trận, số ô bị khóa, có/không Sóng Axit. Người chơi bấm **"Vào trận!"** mới chuyển scene (không còn tự chuyển sau 3s).

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

### 5.4. Tế bào phe ta (`HeroData` – `Assets/_Game/Data/Heroes`)

| Tế bào | Prefab | Giá | Máu | Sát thương | Nhịp đánh | Vai trò | Chỉ số ảnh hưởng |
|---|---|---|---|---|---|---|---|
| **Hồng Cầu** | `CellHongCau` | 75 | 100 | 0 | – | Kinh tế: rơi xu ATP mỗi 5s | Immunity (máu), Hydration (tốc độ rơi xu) |
| **Bạch Cầu** | `CellBachCau` | 100 | 120 | 25 | 1.0s | **Cận chiến** (tầm 1.5). Prefab có `ProjectileLauncher` nhưng `HeroAttackState` ép đánh gần theo tên | Immunity |
| **Tế bào B** | `CellB` | 125 | 90 | 25 | 0.45s | Xạ thủ bắn nhanh (đạn tự dẫn) | Immunity, Hydration |
| **Đại Thực Bào** | `CellDaiThucBao` | 100 | 400 | 5 | 1.5s | Tanker chặn đường, đánh theo hàng | Immunity |

Khi đặt quân, `HeroBase.SetFromData()` tính:
`Sát thương = gốc × Immunity`, `Máu = gốc × Immunity`, `Nhịp đánh = gốc / Hydration` (chỉ với tướng tích `HeroData.affectedByHydration`: hiện là Tế bào B và Hồng Cầu).
Hồng Cầu rơi xu mỗi `5 / Hydration` giây.

### 5.5. Mầm bệnh (`EnemyData` – `Assets/_Game/Data/Enemies`)

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
- **Thẻ "Bài học hôm nay"** (`BattleLessonUI`) hiện **trước** bảng Thắng/Thua: liệt kê điểm mạnh/điểm yếu nhờ/do lối sống (theo từng chỉ số và số ô bị khóa) và lời khuyên. Thắng một ngày sẽ mở khóa ngày kế tiếp.

### 5.9. HUD trận đấu (`BattleHUD`, tự gắn bởi `GridManager`)
- **Nút tốc độ x1/x2** cạnh nút Setting (ẩn khi game đang dừng). `GameSpeed` nhớ tốc độ để Resume từ Pause không reset về x1.
- **Thông báo ngắn (Toast)** giữa phía trên, giải thích hậu quả lối sống ngay lúc nó xảy ra: thả quân vào ô Độc tố, Sóng Axit tràn vào, không đủ ATP, mẹo Hồng Cầu ở wave đầu, mầm bệnh lọt tới Trái Tim (lần đầu), tế bào đầu tiên hy sinh, máu thấp.
- **Viền đỏ + rung màn hình** khi mất máu Trái Tim; viền đỏ nhấp nháy khi máu ≤ 30%.
- **Âm thanh** (`GameSfx`, asset `Resources/GameSfx`, dùng file trong `FGU theme`): trúng đòn & chết của quái/tế bào, Sóng Axit, mất máu Trái Tim. Mỗi loại có giới hạn tần suất (0.08s) và lệch cao độ ngẫu nhiên để đỡ ồn.

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
| UI | `Scene2Transition`, `DaySummaryUI`, `WaveCountDisplay`, `HeroHealthUI`, `LifeUI` | |

### 6.1. Cấu trúc thư mục

```
Assets/
├─ _Game/                  Toàn bộ nội dung của game
│  ├─ Scenes/              6 scene (5 scene trong Build Settings + LuTile để test)
│  ├─ Scripts/             Code chia theo hệ thống: Core, SceneFlow, Board, Heroes, Enemies,
│  │                       Shop, Score, Narrative, Progression, Items, UI, Audio, Effects, Legacy
│  │  └─ Editor/           Tool chỉ chạy trong Editor (Tools/, Narrative/)
│  ├─ Data/                ScriptableObject: Scenarios, Heroes, Enemies, Waves/Day1, Waves/Day2
│  ├─ Prefabs/             Heroes, Enemies, Projectiles, UI, Narrative
│  ├─ Art/                 Sprites, Animations, Backgrounds, Characters, UI, Fonts
│  ├─ Audio/               Music, SFX, Voices
│  ├─ Videos/              Tutorials, Transitions (VideoClip dùng trong Editor)
│  └─ Docs/                GDD, báo cáo cân bằng, ghi chú
├─ _Legacy/                Asset của bản prototype cũ (LoL), không scene nào dùng
├─ ThirdParty/             GUIPackCartoon, Remove_Background_Image/Video
├─ Plugins/                DOTween
├─ Resources/              Asset nạp bằng Resources.Load (Days, UISkin, GameSfx, ItemCatalog, NotebookCatalog, PopUp...)
├─ StreamingAssets/        Video phát qua URL cho WebGL (HD_*, ChuyenCanh, Scenario/)
├─ WebGLTemplates/         Template web 16:9
└─ Settings/, TextMesh Pro/, Synaptic AI Pro/   Cấu hình URP, TMP, tool MCP
```

Quy ước: thư mục và file tên tiếng Anh dạng PascalCase. Asset dữ liệu đặt tên `Loại_Tên` (ví dụ `Scenario_Day1`, `WaveConfigDay2_L1_R1`). Không đổi tên `Resources`, `StreamingAssets`, `Editor`, `WebGLTemplates` vì Unity dựa vào các tên này.

---

## 7. ĐỊNH HƯỚNG NỘI DUNG
- **Giọng văn:** đời thường, hài hước kiểu sinh viên Việt ("cày deadline", "bùng kèo").
- **Liên kết cốt truyện → gameplay:** Cảnh 3 báo trước dịch Cúm A và vi khuẩn đường hô hấp, đúng với đội hình quái ở Scene 3 (Cúm A, Phế Cầu).
- **Mục tiêu giáo dục:** người chơi thấy ngay hậu quả. Ví dụ thức khuya làm tế bào yếu và bàn cờ bị khóa ô, ăn uống lành mạnh làm tế bào khỏe hơn.

---

## 7B. HỆ THỐNG NGÀY CHƠI & NỘI DUNG GIÁO DỤC

### Ngày chơi (`DayConfig`, `GameProgress`)
- Mỗi ngày là 1 asset `Resources/Days/Day{n}.asset` gồm **kịch bản** (`ScenarioData`) và **5 `TuneConfig`** (5 làn). `ScenarioPlayer` và `WaveManager` tự dùng cấu hình của ngày đang chơi; không có cấu hình thì dùng giá trị trong Inspector.
- **MainMap:** 9 nút màn; nút thứ *n* (theo thứ tự trong nhóm `Level`) là **Ngày n** (`LevelButton.day = 0` → tự suy ra). Ngày chưa mở khóa / chưa có nội dung bị làm tối và báo lý do khi bấm.
- **Mở khóa:** thắng Ngày n → mở Ngày n+1 (lưu `PlayerPrefs`).
- **Sinh nội dung:** *Tools → Days → Generate Day Content* (`Assets/_Game/Scripts/Editor/Tools/SetupDay2.cs`) tạo Day1/Day2 config, kịch bản và wave Ngày 2. Chạy lại an toàn.

### Ngày 2 – Cuối tuần thả ga (`Day2_v1.asset`)
4 cảnh tuyến tính, ảnh minh họa **tạm dùng lại của Ngày 1**:

| Cảnh | Lựa chọn | Impact |
|---|---|---|
| 08:00 Dậy trễ | Cà phê đen + nhịn ăn / Mì tôm + rau + trứng / Ngũ cốc + bánh mì + sữa | T+10 I−10 E+5 / I+10 E+10 / I+15 E+10 H+5 |
| 12:30 Buffet nướng | Thịt cháy + bia / Thịt + nhiều rau + nước lọc | T+25 H−15 I−10 / H+15 I+10 |
| 16:00 Stress deadline | Trà sữa size L / Nước + trái cây + đi dạo | T+10 E+10 H−10 / H+20 R+10 |
| 22:00 Cày phim, đói | Gà rán + khoai chiên, thức tới 2h / Sữa ấm + chuối, đi ngủ | T+25 R−20 I−10 / R+20 I+5 |

**Wave Ngày 2:** 8 lượt, cả 5 làn đều có quái, tổng **50 mầm bệnh** (tổng máu 8450, so với 41 con / 6800 của Ngày 1).

### Nội dung giáo dục
- **Thẻ "Kiến thức y khoa"** (`ChoiceData.knowledgeTip`, `KnowledgeCardUI`): hiện sau mỗi lựa chọn, chờ người chơi bấm "Đã hiểu". Đã viết cho cả 17 lựa chọn Ngày 1 và 9 lựa chọn Ngày 2; sửa trong Scenario Editor (ô "Kiến thức y khoa"). **Nội dung cần được người có chuyên môn y tế duyệt lại trước khi phát hành.**
- **Bảng tổng kết ngày** (Scene 2) và **Bài học hôm nay** (sau trận): xem mục 4 và 5.8.

### Công cụ cân bằng
- *Tools → Balance → Day Reports* (`Assets/_Game/Scripts/Editor/Tools/BalanceReportTool.cs`) duyệt **mọi nhánh** lựa chọn của từng ngày (Ngày 1: 84 nhánh, Ngày 2: 24 nhánh), tính chỉ số cuối ngày bằng đúng công thức game (`DayStats`) và ước tính sức mạnh so với tổng máu quái → `Assets/_Game/Docs/BalanceReport_Day{n}.md`.
- Sức mạnh là **ước tính tương đối**: Ngày 1 chênh ×7.4 giữa nhánh mạnh nhất và yếu nhất, Ngày 2 chênh ×3.6. Cần chơi thử để lấy mốc tuyệt đối.

---

## 7C. TIẾN TRÌNH & PHẦN THƯỞNG (21/10/2026)

### Menu chính (MainMenuScene)
- **BT_Play** → MainMap · **BT_KhamPha** → Sổ tay sức khỏe · **BT_XemNgay** → Nhật ký các ngày · **BT_Setting** → Cài đặt · **BT_NangCap** → Nâng cấp (nhãn kèm số sao chưa dùng).
- OnClick gán trong Inspector tới `MainMenuUI` (trên Canvas). Thiết lập lại bằng **Tools → Main Menu → Setup Menu Buttons In Open Scene**.
- **Nhật ký các ngày:** mỗi ngày hiện trạng thái (đã thắng / đang chờ / chưa mở), 3 sao tốt nhất, số thẻ kiến thức đã mở, nút Chơi/Chơi lại vào thẳng Scene 1 của ngày đó; cuối danh sách là ngày "đang phát triển".
- **Cài đặt:** âm lượng tổng, âm lượng hiệu ứng, tốc độ trận mặc định (x1/x2), Xem lại hướng dẫn, Chơi lại từ đầu (có xác nhận; giữ cài đặt âm thanh). Lưu: `GameSettings`.
- Lưu ý: `GlobalAudioMuter.isTestMuteActive = true` đang tắt toàn bộ âm thanh, thanh âm lượng không có tác dụng cho tới khi tắt cờ này.

### Sổ tay sức khỏe (MainMenuScene → BT_KhamPha)
- 3 trang: **Kiến thức** (mọi thẻ "Kiến thức y khoa" của các ngày, hiện có 26), **Tế bào** (4), **Mầm bệnh** (3).
- Mở mục: đọc thẻ kiến thức ở Scene 1; đặt tế bào lên bàn; gặp mầm bệnh trong trận (có Toast "Mầm bệnh mới"). Mục chưa mở hiện "???" + gợi ý cách mở.
- Lưu: PlayerPrefs `CallOutMyCells_Notebook`. Nội dung trang Tế bào/Mầm bệnh: `Resources/NotebookCatalog.asset`.

### Vật phẩm lối sống (kho đồ trong ngày)
| Vật phẩm | Nhận khi chọn | Tác dụng trong trận |
|---|---|---|
| Bình nước | D1 "Thôi nằm nghỉ" (uống nhiều nước); D2 thịt nướng kèm rau + nước lọc | ATP (Hồng cầu + ATP theo thời gian) nhanh ×2 trong 15s |
| Vitamin C | D2 nước + trái cây + đi dạo | Sát thương tế bào ×1.5 trong 12s |
| Giấc ngủ ngon | D1 nấu cơm nhà, ngủ sớm; D2 sữa ấm + chuối rồi ngủ | Hồi 2 máu tim + hồi đầy máu tế bào |
| Bữa ăn ấm | D1 phở; D2 ngũ cốc ấm + sữa | +60 ATP ngay |
- Gán trong Scenario Editor: trường **Vật phẩm thưởng** của mỗi lựa chọn. Sau lựa chọn có thẻ "Nhận vật phẩm".
- Vật phẩm thuộc về ngày đang chơi; mỗi lần vào trận (kể cả Chơi lại) dùng lại đủ số đã nhận. Chỉ dùng được sau khi bấm Bắt đầu.
- Thanh vật phẩm ở góc dưới trái màn trận (`ItemBarUI`). Dữ liệu: `Resources/ItemCatalog.asset`; hiệu ứng: `BattleBuffs`.

### Ghép tế bào (trong trận)
- Kéo tế bào (từ khay hoặc trên bàn) thả lên tế bào **cùng loại, cùng cấp** trên bàn → con bên dưới lên 1 cấp (tối đa 3). Tế bào từ khay trả giá như khi mua.
- Cấp 2: máu & sát thương ×1.8, cấp 3: ×3.2, hồi đầy máu khi ghép; Hồng cầu rơi xu nhanh ×1.5 / ×2. To hơn 12%/cấp, có huy hiệu số cấp + chữ "Cấp 2!" bay lên.
- Khi kéo: ô **vàng** = ghép được, xanh = đặt được, đỏ = không được. Thả lên con cùng loại khác cấp / cấp tối đa → Toast giải thích.
- Bán tế bào đã ghép hoàn tiền của mọi con đã gộp (cấp 2 = 2 con, cấp 3 = 4 con).
- **Hướng dẫn:** bảng "Cách chơi" 4 bước có hình (tự hiện lần đầu bấm Bắt đầu, mở lại bằng nút "?"); bong bóng "Kéo con cùng loại vào đây để nâng cấp!" + nháy vàng 2 con ghép được (tắt sau lần ghép đầu); nút Roll nhún nhảy + nhắc khi đứng yên 12s mà không có cặp ghép.
- PlayerPrefs: `CallOutMyCells_HowToPlaySeen`, `CallOutMyCells_MergeLearned`.

### Sao & Nâng cấp (MainMenuScene → BT_NangCap)
- Lưu sao **tốt nhất** của mỗi ngày (`CallOutMyCells_Stars_Day{n}`); hiện 3 ngôi sao dưới nút ngày trên bản đồ.
- Mỗi cấp nâng cấp tốn 1 sao, tối đa 3 cấp/loại, có nút **Đặt lại** để phân bổ lại:
  - Tế bào bền bỉ: +10% máu mọi tế bào/cấp · Tế bào mạnh mẽ: +10% sát thương/cấp
  - Dự trữ năng lượng: +25 ATP khởi đầu/cấp · Trái tim khỏe: +1 máu tim/cấp
- Hiện có 2 ngày → tối đa 6 sao, mua được 6/12 cấp.
- Công cụ tạo nội dung: **Tools → Progression → Build Notebook & Item Catalogs** (ghi đè nội dung mặc định của 2 catalog, gán hình ngôi sao và âm thanh dùng vật phẩm).

---

## 8. VẤN ĐỀ ĐÃ BIẾT (CẦN QUYẾT ĐỊNH / SỬA)

**Đã sửa (20/10/2026):** Sóng Axit không gây sát thương, quy đổi Toxicity không nhất quán, Sóng Axit kích hoạt khi Toxicity chỉ > 0 và có thể chạy nhiều lần, Hydration chọn tướng theo tên GameObject (Bạch Cầu bị buff nhầm, Hồng Cầu không được buff), làn 4 không có quái, lỗi chính tả "kiệt quệ".

**Đã sửa đợt 2 (20/10/2026):** Sóng Axit quét hàng cố định, bộ đếm wave cộng dồn theo làn, Scene 2 chỉ có 2 mức, Cảnh 3 và cảnh kết không có impact, Hydration/Recovery ít được dùng, chưa có giới hạn trên cho chỉ số.

**Đã sửa đợt 3 (20/10/2026):** ô độc tố không chặn đặt quân, spawn lệch khỏi ô thật và vô hình (object `DummyToxin` rỗng). Sóng Axit dò hàng bằng tọa độ sai và không có hiệu ứng báo hiệu.

**Đã sửa đợt 4 (20/10/2026):** gỡ `TileCell` + collider gắn nhầm trên object cha "Tiles" (một ô giả nằm ngoài bàn cờ, có thể nhận quân). Sửa tool `SetupSceneTiles` để không gắn nhầm lại. Tắt chế độ tự chạy khi recompile của `SetupPerfectTileGrid` (nó tự xếp lại vị trí mọi ô theo thông số cứng, ghi đè vị trí chỉnh tay); menu "Tools/Re-align All Tiles to Perfect Grid" vẫn dùng được.

**Đã sửa đợt 5 – Animation (20/10/2026):**
- Frame cuối của 12 clip đổi sprite chỉ hiện 1/60s (Unity đặt độ dài clip = key cuối + 1/60s), nên mỗi vòng lặp bị giật. Đã thêm key giữ frame cuối bằng tool **Tools → Animation → Fix Sprite Clip Last Frame** (`Assets/_Game/Scripts/Editor/Tools/FixSpriteClipLastFrame.cs`). **Mỗi khi thêm clip mới, chạy lại tool này.**
- Đòn đánh của tế bào trước đây bắn cứng ở 0.25s và kết thúc ở 0.42s, không theo clip. Giờ bắn ở 60% và kết thúc khi hết clip tấn công thật, có tính cả tốc độ Animator (Hydration).
- Bỏ các `Debug.Log` chạy ở tần suất trận đánh (mỗi phát bắn, mỗi nhịp axit, mỗi lần đổi state), tránh khựng khung hình trên WebGL.

**Đã sửa đợt 6 – Hiệu ứng & Pool (20/10/2026):**
- **Pool tái sử dụng quái đã chết mà không reset máu** (pool có 10 con/loại, trận có 18 Cúm A, 14 E.coli). Quái tái sử dụng xuất hiện với máu ≤ 0, tế bào bỏ qua nên chúng đi thẳng tới Trái Tim; E.coli tái sử dụng không bao giờ nổ. Đã reset máu, trạng thái chết và cờ nổ trong `OnEnable`.
- **E.coli bị bắn chết không phát animation chết và không nổ** (bị trả về Pool ngay, cắt ngang coroutine nổ). Giờ E.coli tự phát `die` → nổ → về Pool.
- Chặn quái "chết hai lần" (cộng tiền hai lần) khi đạn bay tới trong lúc đang chết.
- Thêm hiệu ứng `UnitJuice` (`Assets/_Game/Scripts/Effects/UnitJuice.cs`): tế bào và quái **nháy đỏ + giật nhẹ khi trúng đòn**; tế bào **nảy khi được đặt xuống** và **vươn người khi ra đòn**; quái thường **mờ dần + thu nhỏ trong 0.35s khi chết**. Tế bào khi chết ngừng mọi hành động, ẩn thanh máu, đứng hình rồi mờ dần 0.4s trước khi bị xóa.

**Đã sửa đợt 7 – Hiệu năng, dữ liệu & lỗi phát hiện khi chơi thử (20/10/2026):**
- **Pool không hoạt động:** `WayEnemy.myType` không bao giờ được gán (luôn `Normal`) nên trả quái về pool bị lỗi `KeyNotFoundException`, quái chết/tới đích vẫn còn trên sân. Pool giờ gán đúng loại khi tạo quái.
- **Phế Cầu chưa từng gây sát thương:** Animation Event trong clip `attack` không có tên hàm. Đã gán `OnAttackHit`. Tương tự clip tấn công Tế bào B gán `OnAnimationAttack` (bắn đúng frame, hết lỗi console).
- Nút `NextWave` chạy Coroutine khi đã bị ẩn → đã chặn.
- Thay `FindObjectsOfType` chạy mỗi frame (tế bào tìm quái, Cúm A/Phế Cầu/E.coli tìm tế bào) bằng danh sách `HeroBase.Active` / `_Enemy.Active`.
- Bỏ kiểm tra theo tên GameObject: cận chiến dùng `HeroData.isMelee` (Bạch Cầu, Đại Thực Bào), sinh ATP dùng `HeroData.producesATP` (Hồng Cầu); bỏ gắn cơ chế virus theo tên (prefab đã gắn sẵn).
- Công thức quy đổi impact tách thành `DayStats` (dùng chung cho game và công cụ cân bằng).

**Đã sửa đợt 8 – Trải nghiệm chơi (21/10/2026):**
- **Thoại Scene 1:** câu tự chuyển được giữ đủ lâu để đọc (khoảng 15 ký tự/giây, không ngắn hơn `displayDuration`); bấm thì sang câu ngay; nút **Tua nhanh** (góc trên phải) cho chữ hiện ngay và mỗi câu chỉ dừng 0,35s.
- **Nút "?"** trên HUD trận mở lại bảng giới thiệu tế bào bất cứ lúc nào, trận tạm dừng trong lúc xem.
- **Kéo thả bằng cảm ứng:** ô được chọn theo vị trí hình tế bào (đang nhấc lên trên ngón tay), không theo ngón tay.
- **Log:** bản build phát hành chỉ giữ Warning/Error (`LogFilter`), tránh khựng hình trên WebGL.
- **Gọi wave sớm:** nút trên HUD gọi wave tiếp cho mọi làn đang đếm ngược; nút hiện sau 2s thay vì 10s (`showButtonDelay`).
- **Tốc độ x1/x2** được lưu (PlayerPrefs `CallOutMyCells_GameSpeed`) và dùng lại ở các trận sau.

**Đã sửa đợt 9 – Lỗi tìm thấy khi chạy thử trong Editor (21/10/2026):**
- **Làn 1 khởi động 2 lần:** nút StartGame gọi `StartTune(0)` (Inspector) và `LogicSpawner` gọi `StartTune` cho cả 5 làn → wave 1 làn 1 sinh quái gấp đôi. `StartTune` giờ bỏ qua làn đang chạy.
- **Thưởng gọi wave sớm:** chỉ thưởng khi wave sắp tới của làn đó có quái (trước đây gọi sớm cả lượt trống vẫn được thưởng, 5 làn = +130 ATP mỗi lần).
- **Nút Roll:** kiểm tra máy quay đang chạy *trước* khi trừ 2 ATP (bấm liên tục từng mất ATP oan); thiếu ATP thì hiện Toast.
- **`Gate`** chưa gán cánh cửa (Ingame_1) không còn ném `UnassignedReferenceException` mỗi lần vào trận.
- Cảnh báo bộ đếm quái lệch hạ xuống log thường (điều kiện thắng đếm quái thật nên không ảnh hưởng), tránh spam console trên WebGL.

**Lưu ý về Editor tools:** các script trong `Assets/_Game/Scripts/Editor/Tools` có `[InitializeOnLoadMethod]` (`SetupSceneTiles`, `SetupHeroPrefabs`, `SetupVirusPrefabs`, `SetupAnimatorController`, `FixSpritePivots`, `AlignGridTiles`) **tự chạy mỗi lần Unity biên dịch script** và có thể sửa scene/asset đang mở. Khi thấy file thay đổi lạ trong git, hãy kiểm tra các tool này trước.

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
- [x] Âm thanh cho Sóng Axit và trận đấu (`GameSfx`).
- [x] Thẻ "Kiến thức y khoa" sau mỗi lựa chọn.
- [ ] Nhờ người có chuyên môn y tế duyệt nội dung thẻ kiến thức, bổ sung nguồn tham khảo.
- [x] Màn tổng kết 3 mức.
- [x] Hiển thị chỉ số thay đổi so với đầu ngày ở màn tổng kết.
- [x] Ngày 2 (kịch bản + wave) và hệ thống mở khóa ngày.
- [ ] Ảnh minh họa riêng cho Ngày 2 (đang dùng lại ảnh Ngày 1).
- [ ] Nội dung Ngày 3–9.
- [ ] Chơi thử nhánh mạnh nhất / yếu nhất của mỗi ngày để hiệu chỉnh mốc trong báo cáo cân bằng.
