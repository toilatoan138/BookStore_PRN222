# Hướng dẫn tạo dữ liệu mẫu (Seed Data)

Hệ thống đã được cấu hình tự động sinh dữ liệu mồi thông qua DbInitializer.cs. Tuy nhiên, nếu bạn muốn can thiệp thủ công vào Database thông qua SSMS, thư mục này cung cấp file SQL dự phòng.

## Phương pháp 1: Tự động qua Code (Khuyên dùng)
Bạn **không cần** chạy bất kỳ file SQL nào. 
1. Mở Visual Studio.
2. Đảm bảo Connection String trong ppsettings.json đã đúng.
3. Bấm **F5** để chạy project.
4. Hệ thống sẽ tự động tạo các Chi nhánh và các User (Admin, Warehouse, Staff) tương ứng.

## Phương pháp 2: Chạy Script SQL thủ công
Trong trường hợp Database của bạn đã bị lỗi hoặc bạn muốn tạo nhanh không qua code:
1. Đảm bảo Database BookShop đã được tạo và các bảng Branches, AspNetUsers đang trống (để tránh lỗi trùng ID hoặc tên đăng nhập).
2. Mở file seed_db.sql trong SQL Server Management Studio (SSMS).
3. Bấm **Execute**.

## Danh sách Tài khoản
Tất cả đều dùng chung mật khẩu: **Password@123**

| Cấp bậc | Hà Nội (BranchId = 1) | Đà Nẵng (BranchId = 2) | TP.HCM (BranchId = 3) |
| --- | --- | --- | --- |
| **Admin** | dmin_hn | dmin_dn | dmin_hcm |
| **Warehouse** | warehouse_hn | warehouse_dn | warehouse_hcm |
| **Staff** | staff_hn | staff_dn | staff_hcm |

**Đặc biệt:** superadmin (Không thuộc chi nhánh nào, toàn quyền quản trị).
