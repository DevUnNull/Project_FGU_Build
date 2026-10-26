# CALL OUT MY CELLS – CHAPTER 1

> **Phiên bản tài liệu:** 3.0 – tổng kết theo code và dữ liệu hiện tại (nhánh `CallOutMyCell`, 21/10/2026)
> **Nền tảng:** Unity 6 (URP 2D), build **WebGL** chạy trên máy tính và điện thoại (xoay ngang, cảm ứng)
> **Thể loại:** Visual Novel chọn lối sống + Tower Defense theo làn (kiểu Plants vs Zombies) có ghép tế bào
> **Đối tượng:** học sinh, sinh viên

Tài liệu mô tả **đúng những gì đang chạy trong project**. Việc chưa xong hoặc cần quyết định nằm ở **Mục 11**. Lịch sử sửa lỗi nằm ở **Mục 13**.

---

## 1. TỔNG QUAN

### 1.1. Ý tưởng cốt lõi
Người chơi sống một ngày của nhân vật chính (xưng "Tôi", hình chibi nam sinh viên). Mỗi lựa chọn sinh hoạt (ngủ, ăn, uống, đi chơi…) làm thay đổi **5 chỉ số cơ thể**. Đến tối, mầm bệnh tấn công. Người chơi "vào trong cơ thể" và chỉ huy các tế bào miễn dịch phòng thủ. Sức mạnh của đội tế bào phụ thuộc trực tiếp vào lối sống ban ngày.

**Thông điệp:** lối sống hằng ngày quyết định sức đề kháng của cơ thể.

### 1.2. Vòng chơi
1. **Sống một ngày** (Scene 1): chọn lối sống → chỉ số cơ thể thay đổi, đọc thẻ kiến thức y khoa, nhận vật phẩm khi chọn đúng.
2. **Tổng kết** (Scene 2): xem cơ thể mạnh/yếu ở đâu và điều đó ảnh hưởng gì tới trận đánh.
3. **Phòng thủ** (Scene 3): mua, đặt, ghép tế bào; dùng vật phẩm; chặn mầm bệnh trên 5 làn.
4. **Bài học**: sau trận, thẻ "Bài học hôm nay" nối kết quả trận với lối sống.
5. **Tiến trình**: thắng → mở ngày mới, lấy sao để mua nâng cấp, mở thêm mục trong Sổ tay sức khỏe.

---

## 2. LUỒNG SCENE & ĐIỀU HƯỚNG

| # | Scene | Vai trò |
|---|---|---|
| 0 | `MainMenuScene` | Menu chính: Chơi, Khám phá (Sổ tay), Xem ngày (Nhật ký), Cài đặt, Nâng cấp |
| 1 | `MainMap` | Bản đồ chọn ngày (9 nút, có 3 sao tốt nhất dưới mỗi ngày) |
| 2 | `Scene1_LifeSimulation` | Visual Novel – một ngày sinh hoạt |
| 3 | `Scene2_Transition` | Tổng kết trạng thái cơ thể + nút "Vào trận!" |
| 4 | `Ingame_1` | Tower Defense |
| – | `LuTile` | Scene thử nghiệm (không có trong Build Settings) |

- **Chuyển cảnh:** `SceneVideoTransition.LoadScene` chỉ phát **video** (In/Out, từ StreamingAssets) khi đi giữa `MainMenuScene` ↔ `MainMap`. Các chuyển cảnh khác dùng `SceneSlideTransition`: các khối UI trượt ra khỏi màn hình, nạp scene mới, rồi UI trượt vào (có lớp đen ngắn che lúc đổi scene).
- Toàn bộ scene của game nằm trong `Assets/_Game/Scenes`.

### 2.1. Menu chính (`MainMenuScene`)
| Nút | Mở ra |
|---|---|
| `BT_Play` | MainMap |
| `BT_KhamPha` | **Sổ tay sức khỏe** (mục 7.4) |
| `BT_XemNgay` | **Nhật ký các ngày** (mục 7.5) |
| `BT_Setting` | **Cài đặt** (mục 7.6) |
| `BT_NangCap` | **Nâng cấp** bằng sao (mục 7.3), nhãn kèm số sao chưa dùng, VD "Nâng cấp (2)" |

OnClick gán trong Inspector tới `MainMenuUI` (trên Canvas). Thiết lập lại bằng **Tools → Main Menu → Setup Menu Buttons In Open Scene**.

---

## 3. HỆ THỐNG CHỈ SỐ CƠ THỂ (`StomachDayData`)

`StomachDayData` là singleton `DontDestroyOnLoad`. `ScenarioPlayer` reset nó khi bắt đầu Scene 1; Scene 2 và Scene 3 đọc lại. Nó cũng giữ **vật phẩm nhận được trong ngày**.

| Chỉ số | Biến | Mặc định | Giới hạn | Tác động gameplay |
|---|---|---|---|---|
| **Immunity** (Kháng thể) | `immunityMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **máu và sát thương** của mọi tế bào |
| **Energy** (Thể lực) | `energyMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **ATP khởi điểm** và **tốc độ hồi ATP** |
| **Hydration** (Nước) | `hydrationMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **tốc độ đánh** của Tế bào B và **tốc độ rơi xu** của Hồng cầu (`HeroData.affectedByHydration`) |
| **Recovery** (Hồi phục) | `recoveryMultiplier` | 1.0 | 0.1 – 2.0 | Nhân **máu Trái Tim** |
| **Toxicity** (Độc tố) | `toxicityLevel` | 0 | ≥ 0 | **Số ô bị khóa** = `round(Toxicity)`; ≥ 3 thì có **Sóng Axit** |

- **Điểm sức khỏe** (`DayStats.HealthScore`): `(I−1) + (E−1) + (H−1) + (R−1) − 0.1 × Toxicity`. Dương = khỏe hơn bình thường.
- **Quy đổi impact** (`DayStats.ApplyImpact`, dùng chung cho game và công cụ cân bằng): 4 chỉ số đầu nhập theo **%** (`+15` → `+0.15`), Toxicity luôn chia 10 (`+30` → `+3` ô). Mỗi impact hiện 1 popup `StatImpactPopup`.

---

## 4. SCENE 1 – MỘT NGÀY SINH HOẠT (VISUAL NOVEL)

- **Dữ liệu:** `Assets/_Game/Data/Scenarios/Scenario_Day{n}.asset` (`ScenarioData`), chỉnh bằng **Scenario Graph Editor**. Ngày đang chơi lấy kịch bản từ `Resources/Days/Day{n}.asset`.
- **Runtime:** `ScenarioPlayer` – 2 nhân vật (tư thế, biểu cảm, tóc, làm mờ người không nói), nền ảnh hoặc video, thoại tự chạy hoặc chờ bấm.
- **Video tình huống** phát theo **tên file** trong `StreamingAssets/Scenario/` (trường `dialogueVideoFile` / `videoFile`), chạy được cả trên WebGL. Thêm video mới: chép `.mp4` vào thư mục đó và điền đúng tên.
- **Nhịp đọc:** câu tự chuyển được giữ đủ lâu để đọc (≈15 ký tự/giây, không ngắn hơn `displayDuration`). Bấm khi chữ đang chạy → hiện hết câu; bấm tiếp → sang câu ngay. Nút **"Tua nhanh"** (góc trên phải): chữ hiện ngay, mỗi câu dừng 0.35s.
- **Sau mỗi lựa chọn:** popup chỉ số bay lên → thẻ **"Kiến thức y khoa"** (chờ "Đã hiểu", mở mục trong Sổ tay) → thẻ **"Nhận vật phẩm"** nếu lựa chọn có thưởng.

### 4.1. Ngày 1 – Một ngày đi học
```
06:30 Tiếng chuông ─┬─ Dậy giãn cơ ──► 07:00 Ăn sáng
                    ├─ Ngủ nướng ────► 07:15 Trễ giờ, chạy vội
                    └─ Thức trắng ───► Cần chất kích thích
                                (hội tụ)
11:30 Lớp học & tin bùng dịch (Cúm A, vi khuẩn hô hấp)
18:30 Lời rủ đi nhậu & bữa tối
23:30 Rát họng, sốt – "Có nên uống thuốc không?" ─► Scene 2
```

| Cảnh | Lựa chọn | Impact | Vật phẩm |
|---|---|---|---|
| 06:30 | Bật dậy, giãn cơ | E+15 I+10 T−5 R+10 | |
| | Tắt chuông ngủ tiếp | E−10 | |
| | Thức trắng cày deadline | E−10 T+30 I−15 R−15 | |
| 07:00 | Phở tái + trà đá | I+20 H+20 | **Bữa ăn ấm** |
| | Bạc xỉu + bánh mì | E+10 T+5 | |
| | Đi bộ tới trường | E+15 I+10 H−10 | |
| 07:15 | Nhịn ăn, phóng xe | E−15 T+15 H−10 | |
| | Bánh mì vừa chạy vừa ăn | E+5 T+10 | |
| Thức trắng | Nước tăng lực lạnh | E+15 T+25 I−10 H−10 | |
| | Cà phê đen đá đậm | E+10 T+15 H−10 | |
| 11:30 | Đeo khẩu trang, hỏi triệu chứng | I+10 | |
| | Gạt đi | I−10 T+10 | |
| 18:30 | Từ chối, nấu cơm rau, ngủ sớm | R+20 I+15 T−20 H+15 | **Giấc ngủ ngon** |
| | Đồ nướng + bia/nước có gas | T+25 I−15 H−15 R−10 | |
| | Bỏ bữa tối, thức xuyên đêm | T+35 I−25 R−20 | |
| 23:30 | Uống thuốc hạ sốt | R+10 E+10 | |
| | Nằm nghỉ, uống nhiều nước | R+15 | **Bình nước** |

### 4.2. Ngày 2 – Cuối tuần thả ga
4 cảnh tuyến tính (ảnh minh họa **tạm dùng lại của Ngày 1**).

| Cảnh | Lựa chọn | Impact | Vật phẩm |
|---|---|---|---|
| 08:00 Dậy trễ | Cà phê đen + nhịn ăn | T+10 I−10 E+5 | |
| | Mì tôm + rau + trứng | I+10 E+10 | |
| | Ngũ cốc ấm, bánh mì, sữa | I+15 E+10 H+5 | **Bữa ăn ấm** |
| 12:30 Buffet nướng | Thịt cháy + bia | T+25 H−15 I−10 | |
| | Thịt + nhiều rau + nước lọc | H+15 I+10 | **Bình nước** |
| 16:00 Stress deadline | Trà sữa size L | T+10 E+10 H−10 | |
| | Nước, trái cây, đi dạo | H+20 R+10 | **Vitamin C** |
| 22:00 Cày phim, đói | Gà rán + khoai chiên, thức tới 2h | T+25 R−20 I−10 | |
| | Sữa ấm + chuối rồi ngủ | R+20 I+5 | **Giấc ngủ ngon** |

---

## 5. SCENE 2 – TỔNG KẾT (`Scene2Transition`, `DaySummaryUI`)
- Chấm theo điểm sức khỏe (ngưỡng trong Inspector): **Tốt** (≥ 0.3), **Trung bình** (−0.3 … 0.3), **Xấu** (< −0.3), mỗi mức có màu nền và câu thông báo riêng.
- **Bảng tổng kết ngày:** 4 chỉ số so với đầu ngày (100%) kèm tác dụng trong trận, số ô bị khóa, có/không Sóng Axit.
- Người chơi bấm **"Vào trận!"** để sang Scene 3. (Bảng "Báo cáo tình trạng cơ thể" trong trận cũ đã bỏ vì trùng nội dung.)

---

## 6. SCENE 3 – TOWER DEFENSE (`Ingame_1`)

### 6.1. Bàn cờ và làn
- **5 hàng × 7 cột = 35 ô** (`TileCell` đặt tay trong scene, cách nhau ≈ 1.8 × 1.4). Mọi logic theo ô đi qua `GridManager.GetBoardTiles()`.
- **5 làn (Path1–Path5)**, quái đi theo waypoint từ phải sang trái về **Trái Tim**. Quái chạm đích trừ máu Trái Tim.

### 6.2. Kinh tế ATP (`GoldManager`)
| Nguồn | Giá trị |
|---|---|
| ATP khởi điểm | `250 × Energy` + nâng cấp "Dự trữ năng lượng" |
| Hồi tự động | +5 ATP mỗi `4 / Energy` giây, chỉ sau khi bấm **Bắt đầu** |
| Hồng cầu | rơi 1 xu mỗi `5 / Hydration` giây (nhanh hơn ở cấp 2–3); **bấm hoặc vuốt qua** để nhặt +5 ATP |
| Diệt quái | +30 ATP |
| Gọi wave sớm | + số giây còn lại, **chỉ khi wave sắp tới của làn đó có quái** |
| Bán tế bào | kéo vào thùng rác: hoàn 100% giá × số con đã gộp |

### 6.3. Shop, đặt quân & ghép tế bào
- **Khay 4 ô** (`ShopSpawner`). Nút **Roll** (2 ATP) quay kiểu máy slot đổi tế bào chưa mua; bấm khi đang quay không bị trừ tiền.
- **Roll miễn phí:** mỗi khi sang wave mới (từ wave 2), khay tự đổi 1 lần không tốn ATP (chờ người chơi thả tế bào đang kéo từ khay; lần đầu có Toast giải thích).
- **Kéo thả** (`DragAndDrop`): kéo đủ xa mới tính là kéo; chỉ nhấp hoặc thả ngoài bàn thì tế bào trở về chỗ cũ (chưa mua). Trên điện thoại, tế bào nổi lên trên ngón tay và ô được chọn theo vị trí hình tế bào.
- **Màu ô khi kéo:** xanh = đặt được, **vàng = ghép được**, đỏ = không được (ô bận, ô độc tố, thiếu ATP).
- **Ghép:** thả lên tế bào **cùng loại, cùng cấp** trên bàn → con bên dưới lên 1 cấp (tối đa **cấp 3**). Tế bào từ khay trả giá như khi mua.
  - Cấp 2: máu & sát thương **×1.8**; cấp 3: **×3.2**; hồi đầy máu khi ghép. Hồng cầu cấp 2/3 rơi xu nhanh ×1.5 / ×2.
  - Tế bào to hơn 12%/cấp, có **huy hiệu số cấp** (ảnh `level_Charactor`) và chữ "Cấp 2!" bay lên.
  - **Trong lúc kéo (chưa thả):** mọi con ghép được sáng viền vàng + chữ "Ghép được"; kéo ngay lên một con thì viền sáng mạnh + "Thả để lên cấp N!", con đang kéo cũng sáng (`MergePreview`).
  - Thả lên con cùng loại khác cấp / đã cấp 3 → Toast giải thích.

### 6.4. Tế bào phe ta (`HeroData` – `Assets/_Game/Data/Heroes`)
| Tế bào | Giá | Máu | Sát thương | Nhịp đánh | Vai trò |
|---|---|---|---|---|---|
| **Hồng cầu** | 75 | 100 | 0 | – | Kinh tế: rơi xu ATP |
| **Bạch cầu** | 100 | 120 | 25 | 1.0s | Cận chiến |
| **Tế bào B** | 125 | 90 | 25 | 0.45s | Bắn xa, đạn tự dẫn (chịu Hydration) |
| **Đại thực bào** | 100 | 400 | 5 | 1.5s | Tanker cận chiến |

Chỉ số thực tế (`HeroBase.SetFromData`): `gốc × Immunity × nâng cấp × hệ số cấp`; nhịp đánh chia cho Hydration với tướng có `affectedByHydration`. Đòn đánh ra ở 60% clip tấn công và kết thúc khi hết clip.

### 6.5. Mầm bệnh (`EnemyData` – `Assets/_Game/Data/Enemies`)
| Mầm bệnh | Máu | Sát thương | Tốc độ | Cơ chế |
|---|---|---|---|---|
| **Virus Cúm A** | 150 | 15 | 1 | Đứng lại **bắn xa** (tầm 7) tế bào cùng hàng |
| **Vi khuẩn E. coli** | 100 | 10 | 2 | **Cảm tử:** chạm tế bào hoặc bị giết thì nổ AoE 25 (bán kính 1.8), để lại vũng axit 5/s trong 8s |
| **Phế cầu khuẩn** | 300 | 25 | 1 | **Cận chiến:** lao tới đánh rồi lùi lại |

Quái dùng object pool (`MultiEnemyPool` + `PoolConfig_Virus`), được reset máu/trạng thái mỗi lần lấy ra.

### 6.6. Wave (`WaveManager`, `TuneConfig`, `WaveConfig`)
- Bấm **Bắt đầu** → 5 làn chạy **song song**, mỗi làn 1 `TuneConfig`; nghỉ **30s** giữa các lượt.
- Sau **2s** đếm ngược có thể gọi sớm: nút riêng từng làn hoặc nút **"Gọi wave sớm"** gọi mọi làn cùng lúc.
- Bộ đếm wave trên UI tính theo lượt chung.

**Ngày 1:** 7 lượt, **41 mầm bệnh** (18 Cúm A, 14 E. coli, 9 Phế cầu; tổng máu 6800).

| Lượt | Làn 1 | Làn 2 | Làn 3 | Làn 4 | Làn 5 |
|---|---|---|---|---|---|
| 1 | 2 Cúm A | – | – | – | – |
| 2 | – | 1 Cúm A | 1 Cúm A | – | – |
| 3 | 2 Cúm A | 3 Cúm A | – | – | – |
| 4 | 1 E.coli | 1 E.coli | – | 1 E.coli | – |
| 5 | 1 E.coli | 1 E.coli | 2 Cúm A | 2 E.coli | 2 Cúm A |
| 6 | 2 Phế cầu | – | 1 Phế cầu | 2 E.coli | – |
| 7 | 2 Cúm A, 1 E.coli, 2 Phế cầu | 1 Cúm A, 2 E.coli, 1 Phế cầu | 1 Phế cầu | 2 Cúm A, 2 E.coli | 2 Phế cầu |

**Ngày 2:** 8 lượt, cả 5 làn đều có quái, **50 mầm bệnh** (tổng máu 8450).

### 6.7. Độc tố & Sóng Axit
- **Ô độc tố:** khóa ngẫu nhiên `round(Toxicity)` ô trống (trừ cột 0), hình vũng axit; không đặt được tế bào lên. Thả vào → Toast giải thích hậu quả lối sống.
- **Sóng Axit dạ dày** (Toxicity ≥ 3): **1 lần mỗi trận**, khi làn đầu tiên vào lượt 2. Cảnh báo 2s ("AXIT DẠ DÀY TRÀN!") rồi quét **hàng có nhiều tế bào nhất** trong 10s, 10 sát thương/giây, có âm thanh.

### 6.8. Vật phẩm lối sống (`BattleBuffs`, `ItemBarUI`)
| Vật phẩm | Tác dụng |
|---|---|
| **Bình nước** | ATP (Hồng cầu + hồi tự động) nhanh **×2** trong 15s |
| **Vitamin C** | Sát thương tế bào **×1.5** trong 12s |
| **Giấc ngủ ngon** | Hồi 2 máu Trái Tim + hồi đầy máu mọi tế bào |
| **Bữa ăn ấm** | **+60 ATP** ngay |
- Thanh vật phẩm góc dưới trái: biểu tượng, số lượng còn lại, giây hiệu lực. Chỉ dùng được sau khi bấm Bắt đầu.
- Mỗi lần vào trận (kể cả Chơi lại) dùng lại đủ số đã nhận trong ngày.

### 6.9. Thắng / Thua / Sao
- **Máu Trái Tim:** `10 × Recovery` + nâng cấp "Trái tim khỏe".
- **Thua:** máu = 0. **Thắng:** hết wave cuối mọi làn và không còn quái thật trên sân.
- **Sao:** 3★ không mất máu · 2★ còn ≥ 50% · 1★ còn < 50%. Lưu **sao tốt nhất** của ngày.
- Trước bảng Thắng/Thua là thẻ **"Bài học hôm nay"** (`BattleLessonUI`): điểm mạnh/yếu theo từng chỉ số và lời khuyên. Thắng → mở khóa ngày kế tiếp.

### 6.10. HUD trận đấu (`BattleHUD`)
Object `BattleHUD` **đặt sẵn trong `Ingame_1`** để chỉnh tay vị trí (scene khác không có thì tự tạo bằng code; tạo lại bằng **Tools → Battle HUD → Create HUD Objects In Open Scene**):
- `Button_x1` – tốc độ x1/x2 (nhớ giữa các trận) · `Button_Help` – "?" mở bảng **Cách chơi** · `Button_CallAllWaves` – gọi wave sớm (tự ẩn khi chưa gọi được)
- `ToastArea` – vùng thông báo ngắn (bấm/chạm để tắt sớm) · `ItemBarArea` – vị trí ô vật phẩm đầu tiên
- Nút tự đổi sang khung viên thuốc và tự gắn sự kiện lúc chạy; tự ẩn khi game dừng.
- Viền đỏ + rung màn hình khi mất máu Trái Tim; viền đỏ nhấp nháy khi máu ≤ 30%.
- Âm thanh trận (`GameSfx`, file CC0 – nguồn ở `Docs/AudioCredits.md`): mầm bệnh trúng đòn ("squish") và vỡ ("pop"), tế bào trúng đòn / hy sinh (slime), Sóng Axit (bong bóng sủi), mất máu Trái Tim (nhịp tim), dùng vật phẩm & ghép tế bào (power-up), Roll (lắc + chốt từng ô).

### 6.11. Hướng dẫn người chơi trong trận
| Hướng dẫn | Khi nào |
|---|---|
| **Giới thiệu tế bào** (`HeroTutorialManager`, có video) | Tự hiện ở lần vào trận đầu tiên; mở lại từ bảng Cách chơi |
| **Cách chơi** (`HowToPlayUI`, 4 bước có hình: đặt – ghép – Roll – bán) | Tự hiện lần đầu bấm Bắt đầu (tạm dừng trận); mở lại bằng "?" |
| **Gợi ý ghép** (`MergeHintUI`) | Có 2 con ghép được: bong bóng "Kéo con cùng loại vào đây để nâng cấp!" + nháy vàng; tắt sau lần ghép đầu |
| **Gợi ý Roll** | Có tế bào trên bàn, đủ 2 ATP, không có cặp ghép, đứng yên 12s → nút Roll nhún nhảy + 1 lời nhắc mỗi trận |
| **Toast** | Không đủ ATP, ô độc tố, Sóng Axit, mầm bệnh mới, mầm bệnh lọt tới Trái Tim, máu thấp, mẹo Hồng cầu… |

---

## 7. TIẾN TRÌNH DÀI HẠN

### 7.1. Ngày chơi (`DayConfig`, `GameProgress`)
- Mỗi ngày là `Resources/Days/Day{n}.asset` = kịch bản + 5 `TuneConfig`. Hiện có **2 ngày** nội dung; MainMap có 9 nút (ngày chưa có nội dung báo "đang phát triển").
- Thắng Ngày n → mở Ngày n+1. Ngày khóa bị làm tối và báo lý do khi bấm.
- Sinh nội dung ngày: **Tools → Days → Generate Day Content**.

### 7.2. Sao
Sao tốt nhất mỗi ngày hiện dưới nút ngày trên MainMap và trong Nhật ký. Tổng sao dùng mua nâng cấp.

**Hướng dẫn sao & nâng cấp cho người mới:**
- Bảng Thắng: băng **"+N sao mới! … Về Menu chính → Nâng cấp"** ở mép dưới khi đạt kỷ lục sao (`StarRewardBanner`).
- Về MainMap lần đầu có sao chưa dùng: bảng **"SAO & NÂNG CẤP"** (`UpgradeTutorialUI`) giải thích cách nhận sao, dùng sao, Đặt lại; nút **"Đi nâng cấp"** về Menu chính và mở thẳng bảng Nâng cấp.
- Mở bảng Nâng cấp lần đầu mà chưa xem: bảng hướng dẫn hiện đè lên.
- Menu chính: nút **Nâng cấp nhún nhẹ** khi còn sao chưa dùng.

### 7.3. Nâng cấp (`UpgradeProgress`, `UpgradeUI`)
Mỗi cấp tốn **1 sao**, tối đa **3 cấp/loại**, có nút **Đặt lại**. Mỗi dòng có hình minh họa (4 tế bào / Hồng cầu + xu ATP / trái tim).

| Nâng cấp | Mỗi cấp |
|---|---|
| Tế bào bền bỉ | +10% máu mọi tế bào |
| Tế bào mạnh mẽ | +10% sát thương mọi tế bào |
| Dự trữ năng lượng | +25 ATP khởi đầu |
| Trái tim khỏe | +1 máu Trái Tim |

Hiện có 2 ngày → tối đa 6 sao, mua được 6/12 cấp.

### 7.4. Sổ tay sức khỏe (`NotebookUI`)
- 3 trang: **Kiến thức** (26 thẻ: 17 của Ngày 1, 9 của Ngày 2), **Tế bào** (4), **Mầm bệnh** (3).
- Mở mục: đọc thẻ kiến thức ở Scene 1 · đặt tế bào lên bàn · gặp mầm bệnh trong trận. Mục chưa mở hiện "???" (bóng đen) + gợi ý cách mở; có tiến độ "Đã mở x/y".
- Nội dung trang Tế bào/Mầm bệnh: `Resources/NotebookCatalog.asset`.

### 7.5. Nhật ký các ngày (`DayJournalUI`)
Tổng sao và tổng thẻ kiến thức; mỗi ngày: trạng thái (đã thắng / đang chờ / chưa mở + điều kiện), 3 sao, thẻ kiến thức đã mở, nút **Chơi / Chơi lại** vào thẳng Scene 1 của ngày đó; cuối danh sách là ngày "đang được phát triển".

### 7.6. Cài đặt (`SettingsUI`, `GameSettings`)
- **Âm lượng tổng**, **nhạc nền** và **hiệu ứng** (thanh kéo dùng ảnh `Art/UI/Setting`: BackGroundBar, Fill, Knob, Music, EFX).

### 7.6b. Âm thanh
- **Nhạc nền chill** (`MusicPlayer` + `Resources/MusicLibrary`, tự khởi tạo): Menu `Chill_SlowStride`; Map → Scene 1 → Scene 2 `Chill_LofiLoop` chạy liền mạch; trận đấu dùng nhạc riêng của scene (`BackGroundSoundInGame`). Chuyển bài mượt 1.2s. Bài dự phòng: `Chill_LofiAgain`, `Chill_Sherwood`, `Chill_Heavenly`.
- **Hiệu ứng** (`GameSfx`, phát qua nguồn 2D xuyên scene): mọi nút bấm (`UIClickSounds` tự gắn), mở bảng, đặt tế bào, nhặt xu/bán, lỗi/cảnh báo, thông báo mẹo, phần thưởng, thua trận + toàn bộ tiếng trận đấu.
- **Tiếng gõ chữ** khi thoại Scene 1 hiện ra (nhẹ, lệch cao độ ngẫu nhiên, tối đa ~16 tiếng/giây; Tua nhanh thì không gõ).
- **Tiếng cảnh báo** (lỗi, mẹo, mất máu, Sóng Axit) nhỏ hơn tiếng thường và không lặp lại trong vòng 1s; chỉnh ở nhóm "Âm lượng cảnh báo" của `Resources/GameSfx`.
- Nguồn file: `Docs/AudioCredits.md` (CC0). Gán lại mặc định: **Tools → Audio → Assign Default Game Sounds**.
- **Tốc độ trận mặc định** x1/x2.
- **Xem lại hướng dẫn** (giới thiệu tế bào, Cách chơi, gợi ý ghép hiện lại).
- **Chơi lại từ đầu** (có xác nhận): khóa lại ngày, xóa sao, nâng cấp, Sổ tay, hướng dẫn; giữ cài đặt âm thanh.

### 7.7. Dữ liệu lưu (PlayerPrefs)
| Key | Nội dung |
|---|---|
| `CallOutMyCells_UnlockedDay` | Ngày đã mở |
| `CallOutMyCells_Stars_Day{n}` | Sao tốt nhất của ngày n |
| `CallOutMyCells_Upgrade_{Loại}` | Cấp nâng cấp |
| `CallOutMyCells_Notebook` | Các mục Sổ tay đã mở |
| `CallOutMyCells_GameSpeed` | Tốc độ trận x1/x2 |
| `CallOutMyCells_MasterVolume`, `_SfxVolume` | Âm lượng |
| `CallOutMyCells_HeroTutorialSeen`, `_HowToPlaySeen`, `_MergeLearned`, `_UpgradeTutorialSeen` | Đã xem hướng dẫn |

---

## 8. NỘI DUNG GIÁO DỤC & GIỌNG VĂN
- **Giọng văn:** đời thường, hài hước kiểu sinh viên Việt ("cày deadline", "bùng kèo").
- **Cốt truyện → gameplay:** Ngày 1 báo trước dịch Cúm A và vi khuẩn đường hô hấp, đúng đội hình quái trong trận.
- **Thấy hậu quả ngay:** thức khuya làm tế bào yếu và khóa ô; ăn uống lành mạnh làm tế bào khỏe và cho vật phẩm.
- **Kênh truyền tải:** thẻ Kiến thức y khoa (sau mỗi lựa chọn) → Tổng kết ngày → Toast trong trận → Bài học hôm nay → Sổ tay sức khỏe.
- **Nội dung y khoa (thẻ kiến thức, mô tả tế bào/mầm bệnh) cần người có chuyên môn duyệt trước khi phát hành.**

---

## 9. KIẾN TRÚC KỸ THUẬT

### 9.1. Hệ thống chính
| Nhóm (`Scripts/…`) | Script chính |
|---|---|
| `Core/` | `StomachDayData` (+`DayStats`), `GameProgress`, `DayConfig`, `GameSpeed`, `GameSettings`, `PauseGame`, `LogFilter` |
| `SceneFlow/` | `SceneVideoTransition`, `SceneSlideTransition`, `SceneChuyenCanhManager`, `ChangeScene` |
| `Narrative/` | `ScenarioPlayer`, `ScenarioData`, `StatImpactPopup` (Editor: Scenario Graph + validator) |
| `Board/` | `GridManager`, `TileCell` |
| `Shop/` | `GoldManager`, `ShopSpawner`, `DragAndDrop`, `SlotMachineReelController`, `TrashSellAnimation` |
| `Heroes/` | `HeroBase`/`_Hero`, `HeroData`, `HeroStateMachine` (Idle/Attack), `HongCauCoinSpawner`, `HeroLevelBadge` |
| `Enemies/` | `_Enemy`/`EnemyBase`, `EnemyCumA`, `EnemyEcoli`, `EnemyPheCau`, `WaveManager`, `MultiEnemyPool` |
| `Score/` | `LifeManager` (Observer), `StarRatingCalculator` (Strategy), `GameOverHandler` |
| `Items/` | `LifestyleItem`, `ItemCatalog`, `BattleBuffs` |
| `Progression/` | `NotebookProgress`, `NotebookCatalog`, `UpgradeProgress` |
| `UI/` | `RuntimeUI` + `UISkin` (khung CanhBao, nút countWave), `BattleHUD`, `ItemBarUI`, `NotebookUI`, `UpgradeUI`, `MainMenuUI`, `SettingsUI`, `DayJournalUI`, `HowToPlayUI`, `MergeHintUI`, `HeroTutorialManager`, `KnowledgeCardUI`, `DaySummaryUI`, `BattleLessonUI` |
| `Effects/` | `UnitJuice` (nháy, nảy, mờ dần), `MergePreview` (viền sáng khi kéo ghép) |
| `Audio/` | `AudioManager`, `GameSfx`, `GlobalAudioMuter` |

Đơn vị đang sống dùng `HeroBase.Active` / `_Enemy.Active` (không gọi `FindObjectsOfType` mỗi frame). UI mới chủ yếu dựng bằng code qua `RuntimeUI`; HUD trận và nút menu đặt sẵn trong scene để chỉnh tay.

### 9.2. Cấu trúc thư mục
```
Assets/
├─ _Game/          Scenes · Scripts · Data · Prefabs · Art · Audio · Videos · Docs
├─ _Legacy/        Asset prototype cũ (LoL), không scene nào dùng
├─ ThirdParty/     GUIPackCartoon, Remove_Background_Image/Video
├─ Plugins/        DOTween
├─ Resources/      Days, UISkin, GameSfx, ItemCatalog, NotebookCatalog, PopUp…
├─ StreamingAssets/ Video phát qua URL cho WebGL (HD_*, ChuyenCanh, Scenario/)
├─ WebGLTemplates/ Template web 16:9 (CallOutMyCells16x9)
└─ Settings/, TextMesh Pro/, Synaptic AI Pro/
```
Quy ước đặt tên và vị trí file: xem skill `.claude/skills/unity-project-conventions`. Không đổi tên `Resources`, `StreamingAssets`, `Editor`, `WebGLTemplates`.

### 9.3. WebGL & điện thoại
- **Template `CallOutMyCells16x9`:** giữ khung game đúng 16:9 (camera co theo chiều cao, UI co theo chiều rộng – lệch tỉ lệ sẽ làm ô bàn cờ lệch khỏi nền), viền đen phần thừa, nút toàn màn hình.
- **Điện thoại:** nhắc xoay ngang khi cầm dọc; chạm lần đầu → toàn màn hình + khóa ngang (Android); giới hạn devicePixelRatio 2; chặn cuộn/phóng to.
- **Video:** mọi video chạy trên web phát từ `StreamingAssets` qua URL.
- **Log:** bản build thường chỉ giữ Warning/Error (`LogFilter`) để đỡ khựng hình.
- **Âm thanh:** đã bật (`GlobalAudioMuter.isTestMuteActive = false`, Project Settings → Audio → *Disable Unity Audio* đã tắt). Đổi thành `true` nếu cần build bản thử nghiệm không tiếng. Video luôn tắt tiếng (`audioOutputMode = None`) để trình duyệt không chặn tự phát.

### 9.4. Công cụ Editor
| Menu | Việc |
|---|---|
| Tools → Days → Generate Day Content | Tạo DayConfig, kịch bản & wave ngày |
| Tools → Balance → Day Reports | Báo cáo cân bằng mọi nhánh lựa chọn (`Docs/BalanceReport_Day{n}.md`) |
| Tools → Progression → Build Notebook & Item Catalogs | Nội dung Sổ tay, vật phẩm, gán ảnh UISkin & âm thanh |
| Tools → Battle HUD → Create HUD Objects In Open Scene | Tạo HUD trận chỉnh tay được |
| Tools → Main Menu → Setup Menu Buttons In Open Scene | Nối nút menu chính |
| Tools → Animation → Fix Sprite Clip Last Frame | Sửa frame cuối clip bị giật (chạy lại khi thêm clip) |

> ⚠️ Các tool có `[InitializeOnLoadMethod]` (`SetupSceneTiles`, `SetupHeroPrefabs`, `SetupVirusPrefabs`, `SetupAnimatorController`, `FixSpritePivots`, `AlignGridTiles`) **tự chạy mỗi lần Unity biên dịch** và có thể sửa scene đang mở (đã từng xếp lại ô bàn cờ, sinh object thừa `Bullet_CellB`/`Bullet_CumA`). Trước khi lưu scene, kiểm tra `git diff`.

---

## 10. CÂN BẰNG
- Báo cáo (Tools → Balance → Day Reports): Ngày 1 có 84 nhánh, chỉ số sức mạnh **1.64 → 12.09** (> 1 = đủ sức thắng về lý thuyết); Ngày 2 có 24 nhánh, chênh ×3.6.
- **Ngày 1 đang dễ:** nhánh yếu nhất vẫn thắng được về lý thuyết. Vật phẩm, ghép tế bào và nâng cấp bằng sao còn làm game dễ hơn → cần chơi thử rồi tăng máu/số lượng quái (đề xuất bắt đầu ×1.5 máu Ngày 1).
- Các con số chỉnh nhanh: hằng số đầu file `BattleBuffs.cs`, `UpgradeProgress.cs`, `HeroBase.LevelMultiplier`; `earlySpawnRewardMultiplier`, `delayBetweenWaves`, `showButtonDelay` trong `WaveManager`.

---

## 11. VẤN ĐỀ ĐÃ BIẾT / CẦN QUYẾT ĐỊNH
- **Độ khó** (mục 10) chưa được cân lại sau khi thêm vật phẩm, ghép, nâng cấp.
- **Thiếu animation:** Cúm A chỉ có `Run`; Đại thực bào chỉ có 1 clip, không có đòn đánh; nhiều clip chỉ 3–4 frame.
- **Hydration tăng tốc cả animation đứng yên** (`anim.speed` áp cho toàn Animator).
- **Ngày 2 dùng lại ảnh Ngày 1.** Ngày 3–9 chưa có nội dung.
- **Nội dung y khoa** chưa được chuyên gia duyệt.
- **Chưa chơi thử đầy đủ:** ghép tế bào bằng thao tác thật, kéo thả trên điện thoại, video giới thiệu tế bào khi mở bằng "?".
- **HUD:** nút "?" đang ở giữa màn hình (y = 0) và "Gọi wave sớm" trùng vùng Toast – chỉnh trong object `BattleHUD`.
- **Asset/code thừa:** EnemyType không dùng (`Normal`, `Fast`, `Tank`, `Minotaur`, `Salmonella`); `Scripts/Legacy/` (code TD cũ).
- **Đặt tên:** "Call**Our**MyCell" trong tên file/thư mục so với tên game "Call **Out** My Cells" (giữ nguyên).

---

## 12. BACKLOG
- [ ] Cân bằng lại Ngày 1–2 sau khi có vật phẩm, ghép, nâng cấp; chơi thử nhánh mạnh nhất/yếu nhất.
- [x] Bật âm thanh.
- [x] Nhạc nền cho Menu, Map, Scene 1–2; tiếng cho mọi nút.
- [ ] Ảnh minh họa riêng cho Ngày 2; nội dung Ngày 3–9.
- [ ] Animation còn thiếu (Cúm A tấn công, Đại thực bào).
- [ ] Chuyên gia y tế duyệt thẻ kiến thức + bổ sung nguồn tham khảo.
- [ ] Chơi thử trên điện thoại thật (xoay ngang, cảm ứng, hiệu năng).
- [ ] Bản đồ kết cục (đã khám phá x/84 nhánh của Ngày 1).
- [ ] Trang phục nhân vật làm phần thưởng (khi có họa sĩ).
- [x] Sổ tay sức khỏe · Vật phẩm lối sống · Sao & Nâng cấp · Ghép tế bào · Nhật ký ngày · Cài đặt
- [x] Ngày 2 + mở khóa ngày · Thẻ kiến thức · Tổng kết 3 mức · Bài học hôm nay · Công cụ cân bằng
- [x] WebGL 16:9 + điện thoại · Chuyển cảnh trượt UI · Hướng dẫn trong trận

---

## 13. LỊCH SỬ THAY ĐỔI

**20/10/2026 – Sửa lỗi gameplay & hiệu ứng**
- Sóng Axit: gây sát thương thật, chỉ khi Toxicity ≥ 3, 1 lần/trận, nhắm hàng đông tế bào nhất, có cảnh báo 2s và âm thanh.
- Quy đổi Toxicity nhất quán; giới hạn chỉ số 0.1–2.0; Hydration/melee/sinh ATP theo cờ trong `HeroData` thay vì theo tên GameObject; làn 4 có quái.
- Ô độc tố khóa đúng ô thật và chặn đặt quân; gỡ `TileCell` giả trên object "Tiles"; tắt tự xếp lại ô của `SetupPerfectTileGrid`.
- Pool quái: gán đúng loại, reset máu/trạng thái khi tái sử dụng; E. coli chết vẫn nổ; chặn quái "chết hai lần".
- Phế cầu từng không gây sát thương (Animation Event thiếu tên hàm) – đã sửa; Tế bào B bắn đúng frame.
- Animation: sửa frame cuối bị giật (12 clip); đòn đánh theo clip thật.
- Hiệu ứng `UnitJuice`: nháy/giật khi trúng đòn, nảy khi đặt, mờ dần khi chết.
- Thay `FindObjectsOfType` mỗi frame bằng danh sách `Active`; bỏ log tần suất cao.
- Nội dung giáo dục: thẻ kiến thức, tổng kết ngày 3 mức, bài học sau trận; Ngày 2; công cụ cân bằng; tốc độ x2, Toast, âm thanh trận.
- Kéo thả: nhấp/thả ngoài bàn trả tế bào về khay; giao diện khung CanhBao + nút countWave; WebGL 16:9, điện thoại; video tutorial đúng tướng, chỉ hiện lần đầu; chuyển cảnh trượt UI.

**21/10/2026 – Sắp xếp project & trải nghiệm**
- Sắp xếp lại toàn bộ thư mục (`_Game`, `_Legacy`, `ThirdParty`), viết skill quy ước đặt tên/vị trí.
- Video kịch bản phát theo tên file trong StreamingAssets (sửa video Day 1 mất tham chiếu).
- Thoại đọc kịp + Tua nhanh; nút "?"; kéo thả cảm ứng theo vị trí hình tế bào; `LogFilter`; gọi wave sớm cho mọi làn (2s); nhớ tốc độ x2.
- Sửa: làn 1 khởi động 2 lần, thưởng gọi sớm cho lượt trống, Roll trừ ATP khi đang quay, lỗi `Gate`, spam cảnh báo bộ đếm quái.
- Thêm Sổ tay sức khỏe, vật phẩm lối sống, sao & nâng cấp, ghép tế bào (huy hiệu cấp, gợi ý, bảng Cách chơi, hiệu ứng khi kéo).
- Bỏ bảng Báo cáo tình trạng cơ thể trong trận; HUD trận đặt sẵn trong scene.
- Menu chính: Khám phá → Sổ tay, Xem ngày → Nhật ký, Cài đặt, nút Nâng cấp mới; gỡ 2 nút khỏi MainMap.
- Roll miễn phí mỗi wave mới; hướng dẫn sao & nâng cấp (băng trên bảng Thắng, bảng "SAO & NÂNG CẤP", nút Nâng cấp nhún).
