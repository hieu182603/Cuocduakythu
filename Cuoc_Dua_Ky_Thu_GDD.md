# Cuộc Đua Kỳ Thú - Game Design Document

## 1. Tổng quan

-   Thể loại: Party Board Game + Quiz.
-   Người chơi: 2-50.
-   Mục tiêu: Hoàn thành 1 vòng và quay lại ô Start/Finish đầu tiên.

## 2. Gameplay

Mỗi lượt: 1. Kiểm tra hiệu ứng (mất lượt, chờ...). 2. Tung xúc xắc 1-6.
3. Di chuyển. 4. Kích hoạt ô. 5. Kết thúc lượt.

### Ô Câu hỏi (\~70% thiết kế gốc, \~53.1% thực tế bàn cờ)

-   Trả lời đúng: kết thúc lượt.
-   Sai: random hình phạt (lùi ô, chờ 10/15/20s, mất lượt...).
-   Sai liên tiếp: cộng thêm 5 giây mỗi lần.
-   Đúng sẽ reset chuỗi sai.

### Ô Bẫy (\~15% thiết kế gốc, \~18.75% thực tế bàn cờ)

Random: - Lùi 2/3/5 ô - Mất lượt - Đổi vị trí - Giảm xúc xắc 3 lượt -
Quay về Start (hiếm)

### Ô Thưởng (\~7.5% thiết kế gốc, \~15.6% thực tế bàn cờ)

Random: - Tiến 2/3/5 ô - Lá chắn - Đi thêm lượt - Double Dice - Miễn
phạt - 20% quà là troll.

### Ô Vòng quay (3 ô - \~9.4% bàn cờ)

12 kết quả gồm thưởng và phạt.

### Chi tiết Thiết kế Bản đồ & Phân phối Ô thực tế (map.png)

Bản đồ là một đường đua khép kín (racetrack loop) dạng lưới **11 x 7 ô**, tổng cộng **32 ô** (khác biệt so với mục tiêu MVP 40 ô ban đầu).
-   **Start/Finish**: 1 ô (Màu xanh lá, có cờ caro trắng đen).
-   **Ô Câu hỏi**: 17 ô (Màu tím/cam, ký hiệu `?`).
-   **Ô Bẫy**: 6 ô (Màu đỏ, bao gồm hình ảnh quả bom hoặc bẫy chông sắt).
-   **Ô Thưởng**: 5 ô (Màu xanh lá, hình hộp quà thắt nơ vàng).
-   **Ô Vòng quay**: 3 ô (Màu xanh dương, biểu tượng vòng quay nhiều màu).

#### Thứ tự các ô theo chiều kim đồng hồ từ ô Start/Finish (ô số 1):
1.  **START/FINISH** (Ô góc dưới-trái)
2.  **CÂU HỎI** (Tím, `?`)
3.  **BẪY** (Đỏ, Bom)
4.  **CÂU HỎI** (Tím, `?`)
5.  **THƯỞNG** (Xanh lá, Hộp quà)
6.  **CÂU HỎI** (Tím, `?`)
7.  **VÒNG QUAY** (Xanh dương, Bánh xe)
8.  **CÂU HỎI** (Tím, `?`)
9.  **BẪY** (Đỏ, Bẫy chông)
10. **CÂU HỎI** (Tím, `?`)
11. **CÂU HỎI** (Cam/vàng góc dưới-phải, `?`)
12. **CÂU HỎI** (Tím, `?`)
13. **CÂU HỎI** (Tím, `?`)
14. **BẪY** (Đỏ, Bẫy chông)
15. **CÂU HỎI** (Tím, `?`)
16. **THƯỞNG** (Xanh lá, Hộp quà)
17. **CÂU HỎI** (Cam/vàng góc trên-phải, `?`)
18. **BẪY** (Đỏ, Bom)
19. **THƯỞNG** (Xanh lá, Hộp quà)
20. **CÂU HỎI** (Tím, `?`)
21. **VÒNG QUAY** (Xanh dương, Bánh xe)
22. **CÂU HỎI** (Tím, `?`)
23. **BẪY** (Đỏ, Bẫy chông)
24. **CÂU HỎI** (Tím, `?`)
25. **THƯỞNG** (Xanh lá, Hộp quà)
26. **CÂU HỎI** (Tím, `?`)
27. **CÂU HỎI** (Cam/vàng góc trên-trái, `?`)
28. **CÂU HỎI** (Tím, `?`)
29. **VÒNG QUAY** (Xanh dương, Bánh xe)
30. **CÂU HỎI** (Tím, `?`)
31. **BẪY** (Đỏ, Bẫy chông)
32. **THƯỞNG** (Xanh lá, Hộp quà)

## 3. Người chơi & Nhân vật

-   Tối đa 50 người.
-   Có thể nhiều người đứng cùng một ô (Cơ chế chồng nhân vật - Multi-character Stacking).
-   10 nhân vật ngựa, chỉ khác ngoại hình (skin) và tông màu chủ đạo, có chung chỉ số và kích thước mini khi di chuyển trên bàn cờ.

### 3.1. Danh sách 10 Nhân vật Ngựa (Horse Characters)
1.  **01: Lộc Phát** - Tông màu Xanh lá chủ đạo. Chest Badge: Ký hiệu tia sét màu vàng trên nền tròn xanh lá đậm.
2.  **02: Bạch Mã** - Tông màu Xanh dương - Trắng chủ đạo. Chest Badge: Ký hiệu bông tuyết màu trắng trên nền tròn xanh dương.
3.  **03: Hỏa Long** - Tông màu Đỏ rực chủ đạo. Chest Badge: Ký hiệu ngọn lửa màu cam trên nền tròn đỏ đậm.
4.  **04: Thiên Phong** - Tông màu Vàng/Cam chủ đạo. Chest Badge: Ký hiệu đám mây/gió màu trắng trên nền tròn vàng đất.
5.  **05: Ánh Sáng** - Tông màu Tím chủ đạo. Chest Badge: Ký hiệu ngôi sao lấp lánh bốn cánh màu trắng trên nền tròn tím.
6.  **06: Hắc Mã** - Tông màu Đen/Xám tối chủ đạo. Chest Badge: Ký hiệu trăng lưỡi liềm màu trắng trên nền tròn đen.
7.  **07: Kim Tướng** - Tông màu Vàng đất mặc giáp bảo vệ chủ đạo. Chest Badge: Ký hiệu khiên bảo vệ màu vàng trên nền tròn cam.
8.  **08: Băng Phong** - Tông màu Xanh lam nhạt (Ice Blue) chủ đạo. Chest Badge: Ký hiệu tinh thể băng màu trắng trên nền tròn xanh lam.
9.  **09: Sakura** - Tông màu Hồng hoa anh đào chủ đạo. Chest Badge: Ký hiệu hoa anh đào 5 cánh màu trắng trên nền tròn hồng.
10. **10: Lôi Thần** - Tông màu Xanh dương đậm chủ đạo. Chest Badge: Ký hiệu tia sét màu xanh trên nền tròn xanh dương đậm.

### 3.2. Trạng thái & Animation nhân vật
-   **Các phiên bản Sprite**:
    -   *Thẻ chọn nhân vật (Artwork)*: Phiên bản vẽ chi tiết dùng trong màn hình Character Select.
    -   *Phiên bản In-game (Mini)*: Phiên bản thu nhỏ, tối giản hóa các chi tiết để dễ hiển thị và di chuyển trên bàn cờ.
-   **Trạng thái Animation**:
    -   *Idle (Đứng yên)*: Nhân vật nhấp nhô nhẹ theo chu kỳ nhịp thở (gồm 4 frame lặp liên tục).
    -   *Di chuyển*: Nhảy hình vòng cung (Jump arc) từ ô này sang ô tiếp theo theo thứ tự di chuyển.
-   **Biểu cảm (Emotes)**: Xuất hiện bong bóng biểu cảm trên đầu nhân vật tùy sự kiện game:
    -   *Vui vẻ (Happy)*: Cười hí hửng mắt híp lại khi có kết quả tốt.
    -   *Thất bại (Failed)*: Khóc tuôn nước mắt khi trả lời sai câu hỏi hoặc trúng bẫy bất lợi.
    -   *Tức giận (Angry)*: Nổi gân tức giận (biểu tượng màu đỏ) khi bị phạt hoặc lùi ô.
    -   *Tự tin (Confident)*: Đeo kính râm đen cực ngầu.
    -   *Bất ngờ (Surprised)*: Mắt mở to ngạc nhiên có dấu chấm than màu vàng.
    -   *Chiến thắng (Victory)*: Vui sướng cầm cúp vàng khi về đích đầu tiên.

### 3.3. Hiệu ứng Kỹ năng/Hình ảnh riêng biệt (Special VFX Themes)
Mặc dù chỉ khác nhau về ngoại hình, 5 nhân vật đầu tiên có các hiệu ứng hình ảnh nguyên tố đặc trưng bao quanh để tạo sự nổi bật:
-   **Lộc Phát**: Hào quang sét xanh lá giật chớp tắt xung quanh.
-   **Bạch Mã**: Hiệu ứng các tia nước bắn tung tóe và luồng nước chảy xiết bao quanh.
-   **Hỏa Long**: Luồng lửa cháy bùng và các đốm lửa bốc lên từ dưới chân.
-   **Băng Phong**: Các mảnh băng nhọn nhô lên từ mặt đất và sương giá tỏa ra xung quanh.
-   **Sakura**: Các cánh hoa anh đào màu hồng bay rơi lơ lửng xung quanh nhân vật.

## 4. Màn hình

### Splash

Logo.

### Main Menu

-   Chơi
-   Tạo phòng
-   Tham gia
-   Bộ câu hỏi
-   Cài đặt

### Lobby

-   2-50 người.
-   Chọn nhân vật.
-   Ready.

### Character Select

10 nhân vật.

### Gameplay

-   Bàn cờ
-   BXH
-   Danh sách lượt
-   Popup câu hỏi
-   Popup thưởng/bẫy/vòng quay.

### Victory

BXH cuối trận.

## 5. MVP

-   Chơi Offline 2-6 người
-   1 map (Bàn cờ thực tế gồm 32 ô được thiết kế ở [map.png](file:///e:/game%20mln/map.png))
-   10 nhân vật ngựa (Lộc Phát, Bạch Mã, Hỏa Long, Thiên Phong, Ánh Sáng, Hắc Mã, Kim Tướng, Băng Phong, Sakura, Lôi Thần)
-   32 ô (bàn cờ thực tế)
-   Xúc xắc
-   Hệ thống câu hỏi JSON
-   Bẫy/Thưởng/Vòng quay
-   Lưu cấu hình

## 6. Roadmap

### V1

Offline.

### V2

Online 50 người.

### V3

Mùa giải, Battle Pass, Skin, nhiều map.

## 7. Công nghệ

### Client

-   Unity 6
-   URP
-   DOTween
-   TextMeshPro
-   Addressables

### Backend

-   ASP.NET Core 9 Web API
-   PostgreSQL
-   Redis
-   SignalR/WebSocket

### Authentication

-   Firebase Auth hoặc JWT.

### Storage

-   Cloudinary/S3.

### DevOps

-   Docker
-   GitHub Actions
-   Nginx

## 8. Cấu trúc

GameManager - TurnManager - DiceManager - BoardManager -
QuestionManager - RewardManager - TrapManager - WheelManager -
UIManager - AudioManager - SaveManager

## 9. Dữ liệu

Player: - id - name - horseId - tile - wrongStreak - shield - skipTurn -
diceModifier

Tile: - id - type - event

Question JSON:

``` json
{
 "question":"2+2=?",
 "answers":["3","4","5","6"],
 "correct":1
}
```
