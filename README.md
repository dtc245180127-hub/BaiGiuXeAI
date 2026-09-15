# Hệ thống quản lý bãi xe có tích hợp AI

## 1. Giới thiệu

**Hệ thống quản lý bãi xe có tích hợp AI (Smart Parking System with AI)** là ứng dụng Web hỗ trợ số hóa quy trình quản lý phương tiện ra/vào, vé gửi xe, vé tháng, người dùng, doanh thu và hoạt động vận hành bãi xe.

Hệ thống được xây dựng bằng **ASP.NET Core MVC**, sử dụng **SQL Server** kết hợp **Entity Framework Core**, đồng thời tích hợp **Google Gemini API** để cung cấp Chatbot AI và chức năng phân tích lưu lượng, gợi ý bố trí nhân sự.

## 2. Mục tiêu

- Tự động hóa quy trình ghi nhận phương tiện vào/ra.
- Hỗ trợ tính cước và quản lý vé tháng.
- Quản lý người dùng và phân quyền.
- Theo dõi doanh thu, lưu lượng và nhật ký hoạt động.
- Cung cấp Chatbot AI hỗ trợ người dùng.
- Phân tích lưu lượng và hỗ trợ quản lý bố trí nhân sự bằng AI.

## 3. Vai trò trong hệ thống

| Vai trò | Chức năng chính |
|---|---|
| **Quản lý (`QuanLy`)** | Quản lý người dùng, vé tháng, báo cáo, nhật ký và các chức năng AI |
| **Nhân viên (`NhanVien`)** | Check-in, check-out, thu phí, xử lý mất vé, tra cứu xe |
| **Khách hàng (`KhachHang`)** | Đăng ký/gia hạn vé tháng, tra cứu thông tin và sử dụng Chatbot AI |
| **Gemini AI** | Xử lý hội thoại và phân tích dữ liệu lưu lượng xe |

## 4. Công nghệ sử dụng

- ASP.NET Core MVC
- C#
- Entity Framework Core
- Microsoft SQL Server / SQL Server Express
- HTML / CSS / JavaScript
- Google Gemini API
- Git
- Visual Studio

## 5. Cấu trúc project

```text
QuanLyBaiXe/
├── QuanLyBaiXe.sln
├── QuanLyBaiXe/
│   ├── Controllers/
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── Data/
│   ├── Migrations/
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
├── docs/
│   ├── requirements.md
│   ├── use_case.md
│   ├── database_design.md
│   └── ai_log.md
└── README.md
```

## 6. Yêu cầu môi trường

- Windows
- Visual Studio có hỗ trợ ASP.NET Core/.NET phù hợp với project
- .NET SDK phù hợp với project
- SQL Server hoặc SQL Server Express
- Git nếu lấy source từ repository
- Internet khi sử dụng Gemini AI

## 7. Cài đặt project

### Bước 1: Lấy source code

Có thể clone repository:

```bash
git clone <repository-url>
cd <project-folder>
```

Hoặc giải nén source và mở file:

```text
QuanLyBaiXe.sln
```

bằng Visual Studio.

### Bước 2: Cấu hình SQL Server

Đảm bảo SQL Server/SQL Server Express đang chạy.

Kiểm tra `appsettings.json` và cấu hình Connection String phù hợp với máy:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=QuanLyBaiXe;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Thay `YOUR_SERVER` bằng tên SQL Server thực tế.

> Không đưa mật khẩu hoặc thông tin kết nối nhạy cảm lên GitHub.

### Bước 3: Khôi phục package

Tại thư mục chứa file `.csproj`:

```bash
dotnet restore
dotnet build
```

Hoặc dùng **Restore NuGet Packages** và **Build Solution** trong Visual Studio.

### Bước 4: Cập nhật database

Project sử dụng Entity Framework Core và thư mục `Migrations/`.

Trong Visual Studio Package Manager Console:

```powershell
Update-Database
```

Hoặc:

```bash
dotnet ef database update
```

Sau đó kiểm tra database trên SQL Server/SSMS.

## 8. Cấu hình Gemini AI

Các chức năng AI sử dụng **Google Gemini API**.

API Key phải được lưu ở phía server hoặc biến môi trường và không được commit lên repository.

Ví dụ:

```text
Gemini__ApiKey=YOUR_GEMINI_API_KEY
```

Tên cấu hình phải khớp với cấu trúc configuration được khai báo trong source code hiện tại.

> Không chia sẻ API Key thật trong README hoặc repository công khai.

## 9. Chạy ứng dụng

### Cách 1: Visual Studio

1. Mở `QuanLyBaiXe.sln`.
2. Chọn project Web làm Startup Project.
3. Kiểm tra SQL Server và Connection String.
4. Kiểm tra Gemini API Key nếu cần sử dụng AI.
5. Build Solution.
6. Nhấn **F5** hoặc **Ctrl + F5**.
7. Truy cập địa chỉ localhost được Visual Studio cung cấp.

### Cách 2: .NET CLI

Tại thư mục chứa `.csproj`:

```bash
dotnet restore
dotnet build
dotnet run
```

Mở địa chỉ localhost được hiển thị trong Terminal.

## 10. Các chức năng chính

### 10.1. Xác thực và phân quyền

- Đăng nhập/đăng xuất.
- Phân quyền theo `QuanLy`, `NhanVien`, `KhachHang`.

### 10.2. Quản lý phương tiện

- Check-in xe.
- Check-out xe.
- Tính phí gửi xe.
- Tra cứu xe đang gửi.
- Kiểm tra xe đang tồn tại trong bãi.

### 10.3. Xử lý mất vé

- Tra cứu phiên gửi xe.
- Ghi nhận mất vé.
- Tính phụ phí mất vé.
- Lưu thông tin xử lý vào Activity Log.

### 10.4. Vé tháng

- Đăng ký vé tháng.
- Gia hạn vé tháng.
- Theo dõi thời hạn.
- Cảnh báo vé sắp hết hạn.

### 10.5. Quản lý người dùng

- Thêm/sửa/xóa tài khoản theo quyền.
- Quản lý vai trò người dùng.

### 10.6. Báo cáo và nhật ký

- Theo dõi doanh thu.
- Theo dõi lưu lượng xe.
- Xem Activity Log.

### 10.7. Chatbot AI

Chatbot sử dụng Google Gemini API để hỗ trợ câu hỏi về:

- Cước phí.
- Quy định gửi xe.
- Quy trình gửi/nhận xe.
- Vé tháng.
- Thông tin phương tiện trong phạm vi dữ liệu mà hệ thống cung cấp cho AI.

### 10.8. AI phân tích lưu lượng và điều phối nhân sự

Hệ thống tổng hợp dữ liệu lượt xe theo khung giờ, gửi dữ liệu tới Gemini để:

- Phân tích lưu lượng.
- Xác định giờ cao điểm.
- Đưa ra nhận xét.
- Đề xuất bố trí nhân sự.

Khi Gemini API không khả dụng, hệ thống thông báo lỗi và không làm gián đoạn các chức năng quản lý khác.

## 11. Cơ sở dữ liệu

Hệ thống sử dụng:

**Microsoft SQL Server + Entity Framework Core Code First.**

Các nhóm dữ liệu chính:

- Roles
- Users
- Vehicles
- ParkingSessions
- MonthlyTickets
- ActivityLogs

Chi tiết xem:

```text
docs/database_design.md
```

## 12. Tài liệu dự án

Thư mục `docs/` gồm:

| File | Nội dung |
|---|---|
| `requirements.md` | Đặc tả yêu cầu phần mềm (SRS) |
| `use_case.md` | Đặc tả và sơ đồ Use Case |
| `database_design.md` | Thiết kế cơ sở dữ liệu |
| `ai_log.md` | Nhật ký và đặc tả tích hợp AI |

## 13. Bảo mật

- Không commit Gemini API Key.
- Không commit mật khẩu SQL Server thật.
- Không đưa thông tin tài khoản quản trị thật vào repository.
- Sử dụng biến môi trường hoặc cấu hình local cho thông tin nhạy cảm.

## 14. Kiểm thử trước khi bàn giao

- [ ] Đăng nhập/đăng xuất.
- [ ] Kiểm tra phân quyền.
- [ ] Check-in xe.
- [ ] Không cho check-in trùng xe đang gửi.
- [ ] Check-out xe.
- [ ] Kiểm tra tính phí.
- [ ] Xử lý mất vé.
- [ ] Đăng ký/gia hạn vé tháng.
- [ ] Tra cứu xe đang gửi.
- [ ] Quản lý người dùng.
- [ ] Báo cáo doanh thu/lưu lượng.
- [ ] Activity Log.
- [ ] Chatbot AI.
- [ ] AI phân tích lưu lượng/điều phối nhân sự.
- [ ] Kiểm tra lỗi khi Gemini API không khả dụng.
- [ ] Kiểm tra Migration/database.
- [ ] Kiểm tra project có thể build và chạy sau khi cấu hình trên máy khác.

## 15. Xử lý lỗi thường gặp

### Không kết nối được SQL Server

Kiểm tra:

1. SQL Server Service đang chạy.
2. Server Name trong Connection String chính xác.
3. Database đã được tạo/cập nhật bằng Migration.
4. Tài khoản kết nối có quyền truy cập database.

### Chưa có database

Chạy:

```powershell
Update-Database
```

hoặc:

```bash
dotnet ef database update
```

### Gemini AI không hoạt động

Kiểm tra:

- API Key đã cấu hình.
- Tên cấu hình khớp với source.
- Máy có Internet.
- API Key còn hiệu lực.

### Project không build được

Thử:

```bash
dotnet restore
dotnet clean
dotnet build
```

Sau đó kiểm tra phiên bản .NET SDK và các package NuGet.

## 16. Bàn giao project

Khi nộp project, nên giữ đầy đủ:

```text
QuanLyBaiXe/
├── Source Code
├── Migrations
├── docs/
│   ├── requirements.md
│   ├── use_case.md
│   ├── database_design.md
│   └── ai_log.md
└── README.md
```

Các thông tin phụ thuộc môi trường cá nhân như Connection String, Gemini API Key và tài khoản demo cần được cấu hình riêng trên máy chạy project.

## 17. Thông tin project

**Tên project:** Hệ thống quản lý bãi xe có tích hợp AI

**Technology:** ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, Google Gemini API
