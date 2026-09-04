# TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM (SRS)
## HỆ THỐNG QUẢN LÝ BÃI XE CÓ TÍCH HỢP AI (SMART PARKING SYSTEM WITH AI)

---

## 1. TỔNG QUAN HỆ THỐNG

### 1.1. Giới thiệu
Hệ thống quản lý bãi xe thông minh tích hợp AI là giải pháp phần mềm hỗ trợ số hóa toàn diện quy trình kiểm soát phương tiện ra/vào, quản lý vé gửi xe (lượt và tháng), theo dõi doanh thu và giám sát vận hành bãi đỗ xe. Điểm nổi bật của hệ thống là tích hợp công nghệ Trí tuệ nhân tạo (Google Gemini API) nhằm cung cấp trợ lý ảo hỗ trợ người dùng và phân tích dữ liệu lưu lượng xe để tối ưu hóa điều phối nhân sự.

### 1.2. Mục tiêu dự án
- Tự động hóa và giảm thiểu sai sót trong quy trình soát vé, tính cước gửi xe và ghi nhận phương tiện.
- Hỗ trợ đa dạng đối tượng người dùng: Quản lý bãi xe, Nhân viên trực chốt và Khách gửi xe.
- Cung cấp khả năng phân tích dữ liệu chuyên sâu thông qua AI nhằm dự báo khung giờ cao điểm và gợi ý phân bổ nhân sự hợp lý.
- Tích hợp Chatbot AI trực tuyến 24/7 để hướng dẫn quy chế, tra cứu giá vé và hỗ trợ khách hàng nhanh chóng.

### 1.3. Phạm vi hệ thống
Hệ thống được triển khai trên nền tảng Web Application (ASP.NET Core MVC), kết nối cơ sở dữ liệu SQL Server và dịch vụ AI đám mây (Gemini AI).

---

## 2. CÁC TÁC NHÂN HỆ THỐNG (ACTORS)

| Tác nhân (Actor) | Mô tả vai trò |
| :--- | :--- |
| **Quản trị viên (QuanLy)** | Toàn quyền kiểm soát hệ thống: Quản lý người dùng/nhân viên, quản lý vé tháng, xem báo cáo doanh thu & lưu lượng, sử dụng AI phân tích và điều phối ca trực nhân sự, xem nhật ký hoạt động. |
| **Nhân viên (NhanVien)** | Thực hiện tác vụ tại cổng kiểm soát: Soát vé xe vào (Check-in), soát vé xe ra và thu phí (Check-out), lập biên bản xử lý mất vé, tra cứu xe đang gửi trong bãi. |
| **Khách hàng (KhachHang)** | Sử dụng dịch vụ gửi xe: Đăng ký vé tháng trực tuyến, yêu cầu gia hạn vé tháng, tra cứu thông tin xe, tự gửi/nhận xe tại quầy kiosk tự phục vụ, chat với AI tư vấn. |
| **Hệ thống AI (Gemini API)** | Dịch vụ AI đám mây tiếp nhận context dữ liệu từ hệ thống để phản hồi truy vấn của Chatbot và đưa ra nhận xét, gợi ý phân bổ nhân lực cho Quản trị viên. |

---

## 3. YÊU CẦU CHỨC NĂNG (FUNCTIONAL REQUIREMENTS)

### 3.1. Phân hệ Xác thực & Phân quyền (FR-AUTH)
- **FR-AUTH-01:** Hệ thống phải hỗ trợ đăng nhập bằng Username và Password.
- **FR-AUTH-02:** Hệ thống phải phân quyền truy cập nghiêm ngặt dựa trên vai trò (`QuanLy`, `NhanVien`, `KhachHang`) thông qua Session và BaseController.
- **FR-AUTH-03:** Người dùng chưa đăng nhập khi truy cập các trang nội bộ phải được tự động chuyển hướng về trang Login.
- **FR-AUTH-04:** Hỗ trợ chức năng Đăng xuất an toàn và xóa toàn bộ Session làm việc.

### 3.2. Phân hệ Quản lý Lượt gửi xe (FR-PARKING)
- **FR-PARKING-01 (Check-in cho nhân viên):** Ghi nhận xe vào bãi gồm biển số xe, loại xe (Xe máy / Ô tô), thời gian vào (`CheckInTime`). Kiểm tra nếu biển số đang trong bãi thì không cho phép check-in trùng lặp.
- **FR-PARKING-02 (Check-out cho nhân viên):** Quét hoặc nhập biển số xe ra, kiểm tra khớp dữ liệu xe đang gửi, tính cước phí tự động theo thời gian thực và loại xe, cập nhật thời gian ra (`CheckOutTime`) và chuyển trạng thái sang `DaThanhToan`.
- **FR-PARKING-03 (Khách tự gửi - Customer Check-in):** Cho phép khách hàng tại quầy kiosk tự nhập biển số xe của mình để nhận chỗ gửi xe.
- **FR-PARKING-04 (Khách tự lấy xe - Customer Check-out):** Khách hàng tra cứu và xem kết quả tính phí lượt gửi xe của mình.
- **FR-PARKING-05 (Xử lý mất vé - Lost Ticket):** Cho phép nhân viên xử lý trường hợp khách làm mất thẻ/vé xe, lập biên bản ghi chú lý do, tự động áp dụng mức phụ thu mất vé theo quy định và giải phóng trạng thái xe.
- **FR-PARKING-06 (Danh sách xe hiện tại):** Hiển thị danh sách toàn bộ phương tiện đang đỗ trong bãi với bộ lọc theo loại xe, biển số và thời gian gửi.

### 3.3. Phân hệ Quản lý Vé tháng (FR-TICKET)
- **FR-TICKET-01 (Đăng ký vé tháng):** Nhân viên hoặc Quản trị viên tạo vé tháng cho phương tiện gồm: Biển số, Loại xe, Chủ xe, Số điện thoại, Ngày bắt đầu, Ngày kết thúc và Giá tiền.
- **FR-TICKET-02 (Khách đăng ký vé tháng online):** Khách hàng có tài khoản có thể gửi yêu cầu đăng ký vé tháng trực tuyến cho xe của mình.
- **FR-TICKET-03 (Gia hạn vé tháng):** Cho phép gia hạn thêm thời gian hiệu lực (30 ngày/lượt) cho vé tháng sắp hoặc đã hết hạn.
- **FR-TICKET-04 (Cảnh báo vé sắp hết hạn):** Hệ thống tự động lọc và hiển thị danh sách các vé tháng còn thời hạn dưới 5 ngày để nhân viên kịp thời thông báo cho chủ xe.
- **FR-TICKET-05 (Miễn phí lượt gửi cho vé tháng còn hạn):** Khi phương tiện có vé tháng hợp lệ thực hiện Check-out, hệ thống tự động tính phí gửi xe lượt là 0 VNĐ.

### 3.4. Phân hệ Quản lý Người dùng (FR-USER)
- **FR-USER-01:** Quản trị viên có thể xem danh sách toàn bộ tài khoản người dùng kèm theo vai trò tương ứng.
- **FR-USER-02:** Quản trị viên có thể tạo mới, cập nhật thông tin (Họ tên, Mật khẩu, Vai trò) và xóa tài khoản người dùng.
- **FR-USER-03:** Ngăn chặn việc xóa tài khoản Quản trị viên chính hoặc tài khoản đang đăng nhập hiện tại.

### 3.5. Phân hệ Báo cáo & Thống kê (FR-REPORT)
- **FR-REPORT-01 (Báo cáo doanh thu):** Thống kê tổng doanh thu từ vé lượt và vé tháng theo ngày, theo tháng hoặc khoảng thời gian tùy chọn.
- **FR-REPORT-02 (Thống kê lưu lượng):** Thống kê tổng số lượt xe vào, lượt xe ra và tỷ lệ lấp đầy bãi xe.
- **FR-REPORT-03 (Nhật ký hoạt động - ActivityLog):** Tự động ghi nhận mọi thao tác quan trọng (Đăng nhập, Check-in, Check-out, Gia hạn vé, Báo mất vé, Cập nhật người dùng) kèm Username, Role và thời điểm phát sinh.

### 3.6. Phân hệ Trí tuệ Nhân tạo - AI (FR-AI)
- **FR-AI-01 (Trợ lý Chatbot thông minh):**
  - Cung cấp giao diện chat nổi góc màn hình hỗ trợ người dùng 24/7.
  - Phản hồi ngữ cảnh dựa trên vai trò người đăng nhập: Khách hàng hỏi về cước phí/vé tháng/quy định; Nhân viên hỏi về quy trình xử lý mất vé/soát xe; Quản lý hỏi về tổng quan vận hành.
  - Tự động tích hợp thông tin trạng thái xe của khách vào ngữ cảnh để giải đáp chính xác.
- **FR-AI-02 (AI Phân tích lưu lượng & Điều phối nhân sự):**
  - Hệ thống tổng hợp dữ liệu lịch sử vào/ra theo từng khung giờ trong ngày thành cấu trúc JSON.
  - Gửi dữ liệu tới Gemini AI với System Prompt chuyên biệt để nhận diện các khung giờ cao điểm (Peak hours).
  - AI đưa ra khuyến nghị cụ thể về số lượng nhân viên cần trực tại cổng vào/cổng ra và sắp xếp ca trực tối ưu cho Quản trị viên.

---

## 4. YÊU CẦU PHI CHỨC NĂNG (NON-FUNCTIONAL REQUIREMENTS)

### 4.1. Hiệu năng (Performance)
- Thời gian xử lý tác vụ Check-in / Check-out không quá 1.5 giây để tránh ùn tắc tại cổng bãi xe.
- Thời gian phản hồi của Chatbot AI trung bình từ 2 - 4 giây tùy thuộc vào độ trễ mạng tới máy chủ Google Gemini.

### 4.2. Bảo mật (Security)
- Bảo mật thông tin đăng nhập, xác thực Session trên từng Action của Controller.
- Bảo vệ khóa API key của Gemini qua biến môi trường (`.env` / `appsettings.json`), không để lộ trên client-side JavaScript.
- Lọc dữ liệu đầu vào (Input Validation) để ngăn ngừa các lỗ hổng SQL Injection và XSS.

### 4.3. Tính khả dụng & Giao diện (Usability)
- Giao diện thân thiện, hiện đại, hỗ trợ Responsive hiển thị tốt trên máy tính để bàn, máy tính bảng và màn hình Kiosk.
- Thông báo lỗi và kết quả thao tác bằng tiếng Việt rõ ràng, dễ hiểu.

### 4.4. Tính toàn vẹn dữ liệu (Reliability & Integrity)
- Sử dụng Entity Framework Core với ràng buộc Foreign Key chặt chẽ giữa các bảng `Users`, `Roles`, `Vehicles`, `ParkingSessions`, `MonthlyTickets`.
- Giao dịch thanh toán và cập nhật trạng thái xe phải được bảo toàn tính nhất quán (ACID).

---

## 5. CÁC QUY TẮC NGHIỆP VỤ (BUSINESS RULES - BR)

- **BR-01 (Biểu phí gửi xe lượt theo giờ):**
  - Xe máy: 5,000 VNĐ / lượt (hoặc block ngày), gửi qua đêm: 10,000 VNĐ.
  - Ô tô: 20,000 VNĐ / lượt ban ngày, gửi qua đêm: 50,000 VNĐ.
- **BR-02 (Đơn giá vé tháng):**
  - Vé tháng xe máy: 100,000 VNĐ / tháng (30 ngày).
  - Vé tháng ô tô: 1,000,000 VNĐ / tháng (30 ngày).
- **BR-03 (Ưu tiên vé tháng):** Xe có vé tháng trong hạn sử dụng thì cước phí gửi xe lượt khi Check-out luôn là 0 VNĐ.
- **BR-04 (Quy tắc mất vé):** Khách báo mất vé phải nộp phụ phí mất vé cố định (Xe máy: 50,000 VNĐ; Ô tô: 100,000 VNĐ) cộng với tiền gửi xe thực tế tính từ lúc vào bãi.
- **BR-05 (Kiểm tra xe trong bãi):** Mỗi biển số xe chỉ có duy nhất một phiên gửi xe ở trạng thái `DangGui` tại một thời điểm. Không thể Check-in nếu xe chưa hoàn tất Check-out trước đó.
- **BR-06 (Nguyên tắc gợi ý của AI):** AI chỉ đóng vai trò khuyến nghị và phân tích dữ liệu, không được phép tự ý thay đổi dữ liệu ca làm việc hoặc cấu trúc cơ sở dữ liệu.
