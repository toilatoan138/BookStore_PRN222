# Kế Hoạch Đồng Bộ Giao Diện & Tính Năng Warehouse & Staff Vào BookStoreRazor

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuyển đổi và hiện thực hóa toàn bộ giao diện và logic nghiệp vụ của 2 vai trò **Warehouse (Quản lý kho)** và **Staff (Nhân viên vận hành & CSKH)** từ codebase tham chiếu `BookStore-main` (JSP/Servlet) sang chuẩn ASP.NET Core Razor Pages trong `BookStoreRazor` với giao diện thẩm mỹ cao, đúng chuẩn CSS/Bootstrap 5, icons FontAwesome và đầy đủ chức năng.

**Architecture:** Sử dụng kiến trúc ASP.NET Core Razor Pages (.NET 8/9), Entity Framework Core với cơ sở dữ liệu `ApplicationDbContext`, tích hợp bảo mật Role-based Authorization (`[Authorize(Roles = "Warehouse,Admin")]` và `[Authorize(Roles = "Staff,Admin")]`), SignalR cho Live Chat và Bootstrap 5 + FontAwesome 6 cho giao diện người dùng.

**Tech Stack:** C#, ASP.NET Core Razor Pages, Entity Framework Core, SQL Server, Bootstrap 5.3, FontAwesome 6.5, jQuery, DataTables.

---

### Task 1: Cập nhật Navigation Sidebar cho Warehouse và Staff
**Files:**
- Modify: `Pages/Shared/_WarehouseSidebarPartial.cshtml`
- Modify: `Pages/Shared/_StaffSidebarPartial.cshtml`

- [ ] **Step 1: Cập nhật `_WarehouseSidebarPartial.cshtml`**
  Cập nhật đầy đủ 10 link phân hệ kho: Tổng quan, Tồn kho, Sắp hết hàng, Vị trí kho, Nhà cung cấp, Tạo PO, Danh sách PO, Đơn xuất kho & Picking, Đơn trả hàng & QC, Hóa đơn chứng từ, Lịch sử biến động.

- [ ] **Step 2: Cập nhật `_StaffSidebarPartial.cshtml`**
  Cập nhật đầy đủ 10 link phân hệ Staff: Tổng quan ca trực, Quản lý đơn hàng, Mã giảm giá, Khách hàng & CRM, Điểm thưởng F-Points, Flash Sale & Sách KM, Đánh giá, Khiếu nại & Duyệt trả hàng, Live Chat CSKH, Báo cáo doanh thu ca trực.

- [ ] **Step 3: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 2: Warehouse Dashboard & Inventory & Low Stock Pages
**Files:**
- Modify: `Pages/Warehouse/Index.cshtml` & `Index.cshtml.cs`
- Modify: `Pages/Warehouse/Inventory/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Warehouse/Inventory/LowStock.cshtml` & `LowStock.cshtml.cs`

- [ ] **Step 1: Cập nhật `Pages/Warehouse/Index.cshtml` & `.cs`**
  Hiện thực lưới 10 thẻ chức năng viền màu đặc trưng của `BookStore-main` (`warehouse/dashboard.jsp`).

- [ ] **Step 2: Cập nhật `Pages/Warehouse/Inventory/Index.cshtml` & `.cs`**
  Hiện thực bộ lọc 4 tiêu chí (Keyword, Category, Author, Publisher), cột STT, Ảnh bìa, Tên sách, Vị trí kệ, Tác giả, NXB, Giá, Tồn kho, Badges trạng thái, phân trang và modal kiểm kê nhanh.

- [ ] **Step 3: Tạo mới `Pages/Warehouse/Inventory/LowStock.cshtml` & `.cs`**
  Hiện thực trang cảnh báo sách sắp hết hàng tồn $\le$ 5 với bộ lọc và nút "Nhập hàng ngay" dẫn tới tạo PO.

- [ ] **Step 4: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 3: Warehouse Suppliers, Locations & Invoices & Inventory History
**Files:**
- Modify: `Pages/Warehouse/Suppliers/Index.cshtml` & `Index.cshtml.cs`
- Modify: `Pages/Warehouse/Locations/Index.cshtml` & `Index.cshtml.cs`
- Modify: `Pages/Warehouse/Invoices/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Warehouse/Inventory/History.cshtml` & `History.cshtml.cs`

- [ ] **Step 1: Cập nhật `Pages/Warehouse/Suppliers/Index.cshtml` & `.cs`**
  Thêm Modal Kho lưu trữ khôi phục NCC đã xóa mềm, Modal Thêm mới và Modal Sửa NCC.

- [ ] **Step 2: Cập nhật `Pages/Warehouse/Locations/Index.cshtml` & `.cs`**
  Thêm bộ lọc theo Khu vực (Zone), Modal Thêm vị trí và Modal Sửa vị trí.

- [ ] **Step 3: Cập nhật `Pages/Warehouse/Invoices/Index.cshtml` & `.cs`**
  Hiện thực bộ lọc Phân loại hóa đơn (Bán SALE vs Nhập PURCHASE), xem chi tiết modal hóa đơn.

- [ ] **Step 4: Tạo mới `Pages/Warehouse/Inventory/History.cshtml` & `.cs`**
  Hiện thực bảng theo dõi lịch sử biến động kho (+/- số lượng) với bộ lọc Nhập / Xuất / Khách trả.

- [ ] **Step 5: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 4: Warehouse Orders, Picking List, Returns & QC Inspection
**Files:**
- Create: `Pages/Warehouse/Orders/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Warehouse/Orders/Picking.cshtml` & `Picking.cshtml.cs`
- Create: `Pages/Warehouse/Returns/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Warehouse/Returns/Inspect.cshtml` & `Inspect.cshtml.cs`

- [ ] **Step 1: Tạo mới `Pages/Warehouse/Orders/Index.cshtml` & `.cs`**
  Danh sách đơn hàng cần xuất kho với lọc trạng thái Processing / Picked, nút "Lấy hàng (Picking)", nút "Giao hàng / Đóng gói", modal xem chi tiết đơn.

- [ ] **Step 2: Tạo mới `Pages/Warehouse/Orders/Picking.cshtml` & `.cs`**
  Giao diện lấy hàng Picking List: thông tin đơn hàng, danh sách sách cần lấy kèm vị trí kho và số lượng, checkbox từng dòng + Check all, chuyển màu xanh khi tick, nút xác nhận hoàn thành picking.

- [ ] **Step 3: Tạo mới `Pages/Warehouse/Returns/Index.cshtml` & `.cs`**
  Danh sách đơn hàng hoàn trả, lọc trạng thái, nút "Kiểm tra hàng (QC Inspect)".

- [ ] **Step 4: Tạo mới `Pages/Warehouse/Returns/Inspect.cshtml` & `.cs`**
  Giao diện kiểm hàng hoàn trả (Checklist QC, đếm số lượng Đã kiểm X/Y, nút "Đạt QC & Nhập kho" hoàn nhập số lượng tồn).

- [ ] **Step 5: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 5: Staff Dashboard & Customer Management & F-Points
**Files:**
- Modify: `Pages/Staff/Index.cshtml` & `Index.cshtml.cs`
- Modify: `Pages/Staff/Customers/Index.cshtml` & `Detail.cshtml`
- Modify: `Pages/Staff/FPoints/Index.cshtml` & `Index.cshtml.cs`

- [ ] **Step 1: Cập nhật `Pages/Staff/Index.cshtml` & `.cs`**
  Báo cáo ca làm việc hôm nay (Tổng đơn, Chờ xử lý, Doanh thu thực) và 9 thẻ chức năng nghiệp vụ Staff phong cách `staff-dashboard.jsp`.

- [ ] **Step 2: Cập nhật `Pages/Staff/Customers/Index.cshtml` & `Detail.cshtml`**
  Bộ lọc VIP (Hạng thành viên, khoảng F-Point), Bulk tag/note/marketing modals, bảng khách hàng và trang Chi tiết khách hàng đa tab.

- [ ] **Step 3: Cập nhật `Pages/Staff/FPoints/Index.cshtml` & `.cs`**
  Thẻ Lệnh thực thi cộng/trừ F-Point và Bảng lịch sử giao dịch điểm thưởng.

- [ ] **Step 4: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 6: Staff Promotions, Promo Books, Reviews & Tickets
**Files:**
- Modify: `Pages/Staff/Promotions/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Staff/Promotions/Books.cshtml` & `Books.cshtml.cs`
- Modify: `Pages/Staff/Reviews/Index.cshtml` & `Index.cshtml.cs`
- Modify: `Pages/Staff/Tickets/Index.cshtml` & `Index.cshtml.cs`

- [ ] **Step 1: Cập nhật `Pages/Staff/Promotions/Index.cshtml` & `.cs`**
  Danh sách đợt Flash Sale, modal tạo mới/sửa đợt sale, nút liên kết tới trang quản lý sách khuyến mãi.

- [ ] **Step 2: Tạo mới `Pages/Staff/Promotions/Books.cshtml` & `.cs`**
  Quản lý danh sách sách tham gia Flash Sale, thêm sách mới vào đợt sale và xóa sách khỏi khuyến mãi.

- [ ] **Step 3: Cập nhật `Pages/Staff/Reviews/Index.cshtml` & `.cs`**
  Lọc số sao 1-5 sao, tìm kiếm theo tên sách, modal nhân viên phản hồi đánh giá công khai và ẩn/hiện đánh giá.

- [ ] **Step 4: Cập nhật `Pages/Staff/Tickets/Index.cshtml` & `.cs`**
  Tab 1 Quản lý khiếu nại (xem chi tiết & phản hồi ticket), Tab 2 Duyệt yêu cầu trả hàng từ khách (Duyệt trả hàng / Hoàn tiền / Từ chối).

- [ ] **Step 5: Kiểm tra biên dịch**
  Run: `dotnet build`

---

### Task 7: Staff Live Chat, Orders, Vouchers & Sales Reports
**Files:**
- Modify: `Pages/Staff/Chat.cshtml` & `Chat.cshtml.cs`
- Create: `Pages/Staff/Orders/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Staff/Vouchers/Index.cshtml` & `Index.cshtml.cs`
- Create: `Pages/Staff/Reports/Index.cshtml` & `Index.cshtml.cs`

- [ ] **Step 1: Cập nhật `Pages/Staff/Chat.cshtml` & `.cs`**
  Live Chat CSKH với danh sách khách hàng đang chờ, khung hội thoại 2 chiều, câu trả lời mẫu nhanh và gửi tin nhắn thời gian thực.

- [ ] **Step 2: Tạo mới `Pages/Staff/Orders/Index.cshtml` & `.cs`**
  Quản lý đơn hàng phía Staff: Lọc trạng thái, xem chi tiết, duyệt đơn hàng và in hóa đơn.

- [ ] **Step 3: Tạo mới `Pages/Staff/Vouchers/Index.cshtml` & `.cs`**
  Quản lý mã giảm giá: Danh sách voucher, modal tạo voucher mới, bật/tắt voucher.

- [ ] **Step 4: Tạo mới `Pages/Staff/Reports/Index.cshtml` & `.cs`**
  Báo cáo doanh thu ca trực: Thống kê doanh thu theo ngày/khoảng ngày, phân tích đơn hàng và xuất dữ liệu.

- [ ] **Step 5: Kiểm tra biên dịch và toàn bộ hệ thống**
  Run: `dotnet build`
