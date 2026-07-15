# 🏁 Cuộc Đua Kỳ Thú - Party Board Game & Quiz

Chào mừng bạn đến với **Cuộc Đua Kỳ Thú**! Một trò chơi cờ tỷ phú phong cách party kết hợp với hệ thống trả lời câu hỏi trắc nghiệm nhanh đầy kịch tính, hỗ trợ từ 2 đến 50 người chơi cùng lúc. Trả lời đúng câu hỏi, vượt qua các bẫy nguy hiểm và tận dụng các phần thưởng để về đích đầu tiên!

---

## 🎮 Tổng Quan Trò Chơi

- **Thể loại**: Party Board Game + Trắc nghiệm kiến thức (Quiz).
- **Số lượng người chơi**: 2 - 50 người.
- **Mục tiêu**: Đổ xúc xắc di chuyển quanh bàn cờ, vượt qua các thử thách và là người đầu tiên hoàn thành 1 vòng đua quay về ô **START/FINISH**.
- **Cơ chế chơi**: 
  - **Offline (Pass & Play)**: Chơi cùng nhau trên một thiết bị (2 - 6 người).
  - **Online Multiplayer**: Kết nối qua phòng (Room Code) sử dụng SignalR để thi đấu thời gian thực.

---

## 🗺️ Bản Đồ & Các Loại Ô Trên Đường Đua
Bản đồ game là một đường đua khép kín (racetrack loop) gồm **32 ô** với các sự kiện ngẫu nhiên:

1. **START/FINISH (1 ô - Màu xanh lá, cờ caro)**: Điểm xuất phát và là đích đến cuối cùng để giành chiến thắng.
2. **Ô Câu hỏi (17 ô - Màu tím/cam, ký hiệu `?`)**: 
   - Trả lời đúng câu hỏi trắc nghiệm trong 30 giây để hoàn thành lượt.
   - Trả lời sai hoặc hết giờ sẽ nhận hình phạt ngẫu nhiên (lùi ô, đứng chờ, mất lượt) và tăng chuỗi sai (cộng dồn thời gian phạt).
3. **Ô Bẫy (6 ô - Màu đỏ, hình Bom/Chông)**: Kích hoạt các hình phạt ngẫu nhiên (lùi 2/3/5 ô, mất lượt, đổi vị trí với người chơi khác, quay về Start).
4. **Ô Thưởng (5 ô - Màu xanh lá, hình Hộp quà)**: Nhận các lợi ích ngẫu nhiên (tiến 2/3/5 ô, nhận Lá Chắn chặn bẫy, nhận thêm lượt đi, xúc xắc nhân đôi).
5. **Ô Vòng quay (3 ô - Màu xanh dương, biểu tượng Vòng quay)**: Xoay vòng quay kỳ bí để nhận 1 trong 12 kết quả ngẫu nhiên bao gồm cả phạt và thưởng.

---

## 🐴 Hệ Thống Nhân Vật
Game cung cấp **10 nhân vật ngựa** ngộ nghĩnh sở hữu các hiệu ứng hào quang nguyên tố (VFX) độc đáo:
1. **Lộc Phát** (Xanh lá - VFX Sét xanh lá)
2. **Bạch Mã** (Xanh dương/Trắng - VFX Tia nước)
3. **Hỏa Long** (Đỏ - VFX Lửa cháy)
4. **Thiên Phong** (Vàng/Cam - VFX Gió cát)
5. **Ánh Sáng** (Tím - VFX Ánh sao)
6. **Hắc Mã** (Đen - VFX Bóng tối)
7. **Kim Tướng** (Vàng mặc giáp - VFX Khiên hộ thể)
8. **Băng Phong** (Xanh băng - VFX Băng giá)
9. **Sakura** (Hồng - VFX Cánh hoa đào bay)
10. **Lôi Thần** (Xanh dương đậm - VFX Sấm sét)

---

## 🛠️ Cấu Trúc Dự Án

Dự án được chia làm 3 phần chính:
- **`Backend/`**: ASP.NET Core 9 Web API, kết nối cơ sở dữ liệu PostgreSQL (Supabase) và dịch vụ WebSocket qua SignalR để điều phối phòng chơi và đồng bộ trạng thái game.
- **`frontend/`**: Bản Web Client xây dựng bằng HTML5, CSS3 và Javascript thuần (VanillaJS), sử dụng thư viện SignalR client. Giao diện thiết kế theo phong cách hoạt hình (Cartoon) với các hiệu ứng kính mờ (Glassmorphism) hiện đại.
- **`UnityClient/`**: Game client chạy trên nền tảng Unity 6 sử dụng URP (Universal Render Pipeline), DOTween và Addressables, sẵn sàng để build cho PC, Mobile hoặc WebGL.

---

## 🚀 Hướng Dẫn Chạy Game

### 1. Yêu Cầu Hệ Thống
* Cài đặt [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (đối với Backend).
* Cài đặt [Unity Editor 6](https://unity.com/download) (nếu muốn chạy client Unity).
* Một trình duyệt web hiện đại (Chrome, Edge, Firefox, Safari) để chơi bản Web.

---

### 2. Khởi Chạy Backend Server
Server API đóng vai trò làm trung gian kết nối chơi mạng qua SignalR.
1. Mở Terminal/Command Prompt trong thư mục gốc của dự án.
2. Di chuyển vào thư mục `Backend` hoặc chạy trực tiếp lệnh:
   ```bash
   dotnet run --project Backend/Backend.csproj
   ```
3. Sau khi chạy, máy chủ sẽ lắng nghe kết nối tại:
   - REST API & Swagger: `http://localhost:5089` (Có thể truy cập `/swagger` để xem tài liệu API).
   - SignalR Hub: `http://localhost:5089/gameHub`.
   
> **Lưu ý**: Connection String của cơ sở dữ liệu PostgreSQL đã được cấu hình sẵn tới cơ sở dữ liệu cloud Supabase trong file `Backend/appsettings.json`.

---

### 3. Khởi Chạy Frontend (Web Client)
Bản Web Client được viết bằng HTML/CSS/JS thuần, không cần biên dịch phức tạp.

#### Cách 1: Chạy trực tiếp (Chế độ Offline)
* Bạn chỉ cần nhấp đúp (Double-click) vào file `frontend/index.html` để mở trò chơi trực tiếp trên trình duyệt.

#### Cách 2: Chạy thông qua Web Server cục bộ (Khuyên Dùng cho Multiplayer)
Để SignalR kết nối ổn định và tránh lỗi phân quyền (CORS) trên một số trình duyệt:
- **Sử dụng VS Code**: Cài đặt extension **Live Server**, nhấn chuột phải vào `frontend/index.html` và chọn **Open with Live Server**.
- **Sử dụng Python**: Mở terminal tại thư mục `frontend` và gõ:
  ```bash
  python -m http.server 8000
  ```
  Sau đó mở trình duyệt truy cập địa chỉ `http://localhost:8000`.

---

### 4. Khởi Chạy Unity Client
1. Mở **Unity Hub**, chọn **Add project from disk** và trỏ đến thư mục `UnityClient/`.
2. Mở dự án bằng phiên bản **Unity 6**.
3. Nhấp đúp vào Scene chính trong thư mục `Assets/Scenes` và nhấn nút **Play** để chạy thử, hoặc chọn **Build Settings** để đóng gói game thành sản phẩm hoàn chỉnh (.apk, .exe, WebGL).

---

## ✨ Tính Năng Đặc Biệt Đã Hoàn Thiện
- **Chống bấm nhầm tải lại trang (F5/Reload)**: Khi người chơi đang trong trận đấu hoặc phòng chờ, nếu vô tình bấm F5, reload trang hoặc đóng tab, trình duyệt sẽ hiển thị hộp thoại cảnh báo để tránh việc vô tình thoát trận và mất tiến trình.
- **Lưu cấu hình thông minh**: Tự động lưu thiết lập âm thanh (Music/SFX), hiệu ứng nguyên tố (VFX), tốc độ di chuyển và tên của người chơi đã nhập ở lượt chơi trước vào `localStorage` tiện lợi.
