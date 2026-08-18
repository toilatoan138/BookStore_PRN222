# 📚 MindBook - Hệ Thống Nhà Sách Trực Tuyến (BookStore PRN222)

> **MindBook** là ứng dụng web thương mại điện tử chuyên về sách và ấn phẩm giáo dục, được xây dựng theo kiến trúc **ASP.NET Core Razor Pages (.NET 8)** với đầy đủ quy trình bán hàng, thanh toán trực tuyến, quản lý kho bãi, chăm sóc khách hàng thời gian thực (Real-time Chat) và phân quyền quản trị đa cấp.

---

## 🌟 Công Nghệ Sử Dụng (Tech Stack)

- **Framework**: ASP.NET Core 8.0 (.NET 8) Razor Pages
- **ORM / Database**: Entity Framework Core 8.0, Microsoft SQL Server
- **Bảo mật & Phân quyền**: ASP.NET Core Identity (Role-based: `Admin`, `Staff`, `Warehouse`, `Customer`)
- **Giao tiếp Real-time**: Microsoft SignalR Hub (`ChatHub`) cho tư vấn viên & khách hàng
- **Thanh toán trực tuyến**: Tích hợp Cổng thanh toán VNPay Sandbox (HmacSHA512)
- **Email Service**: Tích hợp gửi mã OTP và thông báo đơn hàng qua SMTP
- **Giao diện & Trải nghiệm**: Bootstrap 5.3, FontAwesome 6, Custom Modern CSS (Glassmorphism & Responsive Design)

---

## 👥 Phân Hệ & Tính Năng Chính

### 1. Phân hệ Khách hàng (Customer)
- **Trang chủ & Khám phá**: Banner slider, sách bán chạy, Flash Sale theo giờ, bộ sưu tập Manga/Comic (Conan, One Piece), danh mục sách nổi bật.
- **Tìm kiếm & Lọc nâng cao**: Tìm kiếm theo từ khóa, lọc theo danh mục, khoảng giá, sắp xếp theo giá/đánh giá/mới nhất.
- **Chi tiết sản phẩm**: Thông tin xuất bản, hình ảnh, đánh giá & bình luận của người mua, sách cùng thể loại.
- **Giỏ hàng & Đặt hàng**: Quản lý số lượng, áp dụng mã giảm giá (Voucher), chọn địa chỉ nhận hàng.
- **Thanh toán linh hoạt**: COD (Thanh toán khi nhận hàng), Ví điện tử F-Wallet, Cổng thanh toán trực tuyến VNPay.
- **Tài khoản cá nhân**: Hồ sơ người dùng, đổi mật khẩu, quản lý ví F-Wallet (nạp tiền/lịch sử giao dịch), kho voucher, lịch sử đơn hàng và theo dõi trạng thái giao hàng.
- **Tương tác & Hỗ trợ**: Live Chat trực tuyến với tư vấn viên thông qua SignalR Hub, gửi phiếu yêu cầu hỗ trợ (Support Ticket).

### 2. Phân hệ Nhân viên CSKH & Vận hành (Staff)
- **Tổng quan công việc**: Bảng điều khiển theo dõi đơn hàng chờ xử lý, tin nhắn khách và ticket mới.
- **Quản lý đơn hàng**: Xem chi tiết đơn hàng, duyệt đơn, chuyển trạng thái đóng gói/giao hàng, hủy đơn.
- **Trò chuyện trực tuyến (Live Chat)**: Nhận tin nhắn và phản hồi khách hàng theo thời gian thực (SignalR).
- **Hỗ trợ khách hàng (Tickets)**: Tiếp nhận, phản hồi và cập nhật trạng thái yêu cầu hỗ trợ.
- **Hóa đơn & Chứng từ**: Tra cứu danh sách hóa đơn bán lẻ, in hóa đơn.

### 3. Phân hệ Thủ kho (Warehouse)
- **Tổng quan kho**: Thống kê số lượng tồn, cảnh báo sách sắp hết hàng (Low Stock), cảnh báo vị trí quá tải.
- **Quản lý vị trí lưu kho (Locations)**: Sắp xếp vị trí theo kệ/tầng/ngăn, kiểm tra sức chứa tối đa và sức chứa còn lại.
- **Đơn đặt hàng nhập kho (Purchase Orders)**: Lập đơn nhập từ nhà cung cấp, kiểm đếm và nhận hàng vào kho.
- **Lấy hàng (Picking List)**: Tạo danh sách lấy hàng theo từng đơn phục vụ đóng gói nhanh chóng.
- **Quản lý trả hàng (Returns)**: Tiếp nhận sản phẩm trả lại, kiểm tra chất lượng (Inspect) và quyết định nhập lại kho hoặc thanh lý.
- **Nhà cung cấp (Suppliers)**: Quản lý danh mục nhà cung cấp, thông tin liên hệ và trạng thái hợp tác.

### 4. Phân hệ Quản trị viên (Admin)
- **Bảng điều khiển (Dashboard)**: Thống kê doanh thu, số lượng đơn hàng, sách bán chạy và tăng trưởng khách hàng.
- **Quản lý danh mục (Categories)**: Thêm/Sửa/Xóa và quản lý trạng thái danh mục sách.
- **Quản lý người dùng (Users)**: Danh sách tài khoản, phân quyền vai trò (Admin, Staff, Warehouse, Customer), kích hoạt/khóa tài khoản.
- **Quản lý Sách (Books)**: Quản lý đầu sách, giá bán, giảm giá, hình ảnh, vị trí lưu kho.

---

## 🔑 Tài Khoản Mặc Định (Default Accounts)

Hệ thống đã cấu hình tự động Seed tài khoản và vai trò khi khởi động ứng dụng:

| Vai trò | Tên đăng nhập | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `Admin@123` | Toàn quyền quản trị hệ thống, quản lý tài khoản & báo cáo |
| **Staff** | `staff` | `Staff@123` | Xử lý đơn hàng, live chat hỗ trợ, phản hồi ticket |
| **Warehouse** | `warehouse` | `Warehouse@123` | Quản lý kho bãi, vị trí lưu trữ, nhập hàng và kiểm hàng hoàn |
| **Customer** | `customer` | `Customer@123` | Mua hàng, thanh toán, nạp ví, chat và đánh giá sản phẩm |

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Dự Án

### Yêu cầu môi trường
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB hoặc SQL Server 2019+)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [Visual Studio Code](https://code.visualstudio.com/)

### Các bước thực hiện

1. **Clone repository:**
   ```bash
   git clone <URL_REPOSITORY>
   cd BookStore_PRN222
   ```

2. **Cấu hình chuỗi kết nối Database:**
   Mở file `BookStore_PRN222/appsettings.json` và chỉnh sửa `DefaultConnection` phù hợp với máy của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=BookShop;User Id=sa;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

3. **Khởi tạo Database:**
   - Cách 1: Chạy trực tiếp file `script.sql` trong SQL Server Management Studio (SSMS).
   - Cách 2: Ứng dụng tự động kiểm tra và khởi tạo cấu trúc cơ sở dữ liệu (`DbInitializer.InitializeAsync`) cùng dữ liệu mẫu (Roles, Accounts, Categories, Books, Locations, Suppliers) ngay khi chạy lần đầu.

4. **Chạy ứng dụng:**
   - Dùng .NET CLI:
     ```bash
     dotnet run --project BookStore_PRN222
     ```
   - Hoặc mở file `BookStore_PRN222.sln` bằng Visual Studio 2022 và bấm **F5** (hoặc `Ctrl + F5`).

5. **Truy cập ứng dụng:**
   - URL mặc định: `http://localhost:5199` hoặc `https://localhost:7199`
