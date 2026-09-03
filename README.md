# Hệ thống quản lý bãi xe có tích hợp AI

## 1. Giới thiệu

Hệ thống quản lý bãi xe được xây dựng nhằm hỗ trợ quản lý phương tiện ra vào, vé tháng, người dùng và hoạt động của bãi xe. Hệ thống được phát triển theo kiến trúc ASP.NET Core MVC và sử dụng SQL Server để lưu trữ dữ liệu.

Hệ thống có phân quyền người dùng theo vai trò Nhân viên và Quản lý. Ngoài các chức năng quản lý nghiệp vụ, hệ thống định hướng tích hợp trợ lý AI nhằm hỗ trợ phân tích dữ liệu và đưa ra nhận xét, đề xuất phục vụ công tác quản lý.

## 2. Công nghệ sử dụng

- ASP.NET Core MVC
- C#
- Entity Framework Core
- SQL Server
- HTML, CSS, JavaScript
- Git
- Gemini API

## 3. Cấu trúc project

Các thành phần chính của project:

- `Controllers/`: xử lý request và nghiệp vụ điều khiển
- `Models/`: các lớp dữ liệu của hệ thống
- `ViewModels/`: dữ liệu trung gian phục vụ giao diện
- `Views/`: giao diện người dùng
- `Data/`: cấu hình DbContext và kết nối cơ sở dữ liệu
- `Migrations/`: các migration của Entity Framework Core
- `wwwroot/`: CSS, JavaScript và tài nguyên giao diện
- `Program.cs`: cấu hình và khởi chạy ứng dụng

## 4. Yêu cầu môi trường

- Windows
- .NET SDK phù hợp với project
- SQL Server / SQL Server Express
- Visual Studio
- Git

## 5. Cài đặt project

### Bước 1: Clone source code

```bash
git clone <repository-url>
cd "QuanLyBaiXe (1)"