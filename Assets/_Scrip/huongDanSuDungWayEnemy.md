1. Cập nhật Code (Enum)
Trong file Assets/_Scrip/Enemies/Enums/PathID.cs, enum hiện tại chỉ có từ Path1 đến Path5. Bạn cần mở file này và thêm Path6 vào danh sách.

2. Copy & Chỉnh sửa Config Assets (ScriptableObject)
Các dữ liệu của wave được lưu trong thư mục Assets/_Scrip/Enemies/Config/Resources/:

WaveConfig: Copy một file hiện có (ví dụ WaveConfig5_1.asset) thành WaveConfig6_1.asset. Mở file mới lên và đổi trường Path ID thành Path6.
TuneConfig: Copy một file hiện có (ví dụ TuneConfigMap1_5.asset) thành TuneConfigMap1_6.asset. Mở file này lên, trong list Waves, hãy gán file WaveConfig6_1.asset vừa tạo vào đó.
3. Copy & Setup GameObject trên Scene
Đường đi (Path): Duplicate một GameObject Path cũ (ví dụ Path5), đổi tên thành Path6. Chỉnh tọa độ Y của nó tụt xuống một chút (hiện tại game đang set mỗi làn cách nhau theo trục Y là -1.55f).
Điểm sinh quái (Spawner): Duplicate một GameObject waySpawn cũ (ví dụ waySpawn5), đổi tên thành waySpawn6. Dịch chuyển tọa độ Y của nó cho khớp với làn Path6 mới.
4. Gắn tham chiếu (References) cho Spawner
Chọn waySpawn6 vừa tạo trên Scene, nhìn vào component WaveSpawner trên Inspector và sửa 3 thông số:

Spawn Point: Kéo thả transform của chính waySpawn6 vào đây.
Assigned Path: Kéo thả GameObject Path6 (hoặc component WaypointPath của nó) vào đây.
My Path ID: Đổi dropdown thành Path6.
5. Cập nhật WaveManager
Tìm GameObject có chứa component WaveManager trong Scene (thường là cục Manager hoặc GameLogic).
Trong inspector của WaveManager, tìm mảng Tunes (danh sách các lượt quái). Tăng Array Size lên thêm 1 (ví dụ từ 5 thành 6).
Kéo file TuneConfigMap1_6.asset (đã tạo ở bước 2) vào phần tử mới nhất của mảng này.
Làm đúng 5 bước trên là bạn đã có một làn quái mới hoàn chỉnh với đường đi và config sinh quái độc lập! Bạn có cần tôi hỗ trợ viết một script Editor để tự động hóa việc tạo làn thứ 6 (và thứ N) này luôn không?