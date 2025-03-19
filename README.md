# 🌟 WorkFinder - Nền tảng tìm kiếm việc làm trực tuyến

WorkFinder là một ứng dụng web hiện đại hỗ trợ người tìm việc và nhà tuyển dụng kết nối hiệu quả. Được xây dựng trên nền tảng **ASP.NET Core**, WorkFinder mang đến trải nghiệm thân thiện với người dùng cùng các tính năng tìm kiếm và lọc việc làm nâng cao.

---

## 📌 Tổng quan

WorkFinder cung cấp:

- Giao diện người dùng trực quan, responsive.
- Tìm kiếm việc làm theo từ khóa, vị trí và nhiều tiêu chí nâng cao.
- Quản lý công việc, công ty và ứng viên dễ dàng.
- Nhiều tiện ích hỗ trợ cho cả người tìm việc và nhà tuyển dụng.

---

## ⚙️ Công nghệ sử dụng

| Thành phần           | Công nghệ                                   |
| -------------------- | ------------------------------------------- |
| Framework            | ASP.NET Core MVC                            |
| ORM                  | Entity Framework Core                       |
| Cơ sở dữ liệu        | PostgreSQL                                  |
| Giao diện người dùng | Bootstrap 5, jQuery, Font Awesome           |
| Authentication       | ASP.NET Core Identity                       |
| Repository Pattern   | Quản lý dữ liệu theo mô hình design pattern |
| UI Components        | View Components để module hóa giao diện     |

---

## 🚀 Tính năng chính

### 🔍 Tìm kiếm và lọc việc làm

- Tìm kiếm theo **từ khóa và địa điểm**
- Lọc nâng cao theo:
  - Kinh nghiệm: _Freshers, 1-2 Years, 2-4 Years..._
  - Mức lương: _$50-$1000, $1000-$2000..._
  - Loại công việc: _Full Time, Part Time, Remote..._
  - Thời gian đăng: _24 giờ, 7 ngày, 30 ngày_
  - Cấp độ công việc: _Entry Level, Mid Level, Expert Level_
- Phân trang kết quả
- Chế độ hiển thị **Grid/List**

### 📂 Quản lý công việc

- Danh sách công việc chi tiết
- Trang chi tiết việc làm
- Việc làm nổi bật (Featured jobs)

### 🏢 Công ty

- Hiển thị thông tin công ty liên quan đến tin tuyển dụng
- Logo và mô tả công ty

### 🎨 Giao diện người dùng

- Thiết kế responsive
- Modal lọc nâng cao
- Breadcrumb điều hướng
- Filter tags hiển thị điều kiện lọc

---

## 🌱 Tính năng có thể phát triển

### 👤 Người dùng

- Đăng ký, đăng nhập, quản lý hồ sơ
- Quản lý CV, theo dõi trạng thái ứng tuyển
- Lưu việc làm yêu thích

### 🧑‍💼 Nhà tuyển dụng

- Đăng tin tuyển dụng
- Quản lý ứng viên
- Bảng điều khiển dành riêng cho nhà tuyển dụng
- Gói dịch vụ tuyển dụng (Miễn phí, Premium)

### 📄 Ứng tuyển

- Ứng tuyển trực tiếp & tải CV
- Theo dõi trạng thái và nhận thông báo

### ✨ Tiện ích nâng cao

- Gợi ý việc làm phù hợp
- Email thông báo việc làm mới
- Đánh giá công ty
- Blog chia sẻ kiến thức
- Diễn đàn cộng đồng

### 🔐 Quản trị viên

- Quản lý người dùng, công ty, tin tuyển dụng
- Phê duyệt tin đăng
- Thống kê và báo cáo

---

## 🛠️ Tính năng kỹ thuật

- Tích hợp API cho ứng dụng di động
- Tối ưu SEO
- Tích hợp thanh toán (Premium jobs)
- Xác thực hai lớp (2FA)
- Import/Export dữ liệu (CSV, Excel)
- Tích hợp LinkedIn & nền tảng khác

---

## 🧭 Hướng dẫn cài đặt

```bash
# 1. Cài đặt .NET Core SDK (6.0 hoặc cao hơn)
# 2. Cài đặt PostgreSQL (12.0 hoặc cao hơn)
# 3. Clone repository:
git clone https://github.com/pth2003/WorkFinder.git
cd workfinder

# 4. Cập nhật chuỗi kết nối trong file `appsettings.json`

# 5. Chạy migration để tạo database:
dotnet ef database update

# 6. Chạy ứng dụng:
dotnet run
```
