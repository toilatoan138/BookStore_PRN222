BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260821152747_InitialIdentity', N'8.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Orders] ADD [branch_id] int NULL;
GO

ALTER TABLE [Orders] ADD [order_group_id] nvarchar(50) NULL;
GO

CREATE TABLE [Branches] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Address] nvarchar(500) NULL,
    [City] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Branch_Inventory] (
    [Id] int NOT NULL IDENTITY,
    [BranchId] int NOT NULL,
    [BookId] int NOT NULL,
    [StockQuantity] int NOT NULL,
    CONSTRAINT [PK_Branch_Inventory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Branch_Inventory_Books_BookId] FOREIGN KEY ([BookId]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Branch_Inventory_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Orders_branch_id] ON [Orders] ([branch_id]);
GO

CREATE INDEX [IX_Branch_Inventory_BookId] ON [Branch_Inventory] ([BookId]);
GO

CREATE INDEX [IX_Branch_Inventory_BranchId] ON [Branch_Inventory] ([BranchId]);
GO

ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Branches_branch_id] FOREIGN KEY ([branch_id]) REFERENCES [Branches] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260823075428_AddMultiBranch', N'8.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Purchase_Orders] ADD [branch_id] int NOT NULL DEFAULT 1;
GO

CREATE INDEX [IX_Purchase_Orders_branch_id] ON [Purchase_Orders] ([branch_id]);
GO

ALTER TABLE [Purchase_Orders] ADD CONSTRAINT [FK_Purchase_Orders_Branches_branch_id] FOREIGN KEY ([branch_id]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260823084239_AddBranchToPO', N'8.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [BranchId] int NULL;
GO

CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
GO

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260825020627_AddBranchIdToUser', N'8.0.14');
GO

COMMIT;
GO

-- ====================================================================
-- SEED DATA SCRIPT: BOOKSTORE PRN222
-- ====================================================================
-- Mục đích: Chạy Script này nếu bạn muốn insert data mẫu trực tiếp vào Database 
--           (Thay vì chạy project để EF Core tự tạo).
-- Lưu ý: Hãy đảm bảo các bảng Branches và AspNetUsers đã trống để tránh lỗi trùng lặp.
-- ====================================================================

USE [BookShop];
GO

-- 1. Insert Branches (Chi nhánh)
SET IDENTITY_INSERT [Branches] ON;
INSERT INTO [Branches] ([Id], [Name], [Address], [City], [IsActive]) VALUES 
(1, N'Chi nhánh Hà Nội', N'Đống Đa, Hà Nội', N'Hà Nội', 1),
(2, N'Chi nhánh Đà Nẵng', N'Sơn Trà, Đà Nẵng', N'Đà Nẵng', 1),
(3, N'Chi nhánh TP.HCM', N'Quận 1, TP.HCM', N'TP.HCM', 1);
SET IDENTITY_INSERT [Branches] OFF;
GO

-- 1.5. Insert Roles (Nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Admin')
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES 
    (NEWID(), N'Admin', N'ADMIN', NEWID()),
    (NEWID(), N'Staff', N'STAFF', NEWID()),
    (NEWID(), N'Warehouse', N'WAREHOUSE', NEWID()),
    (NEWID(), N'Customer', N'CUSTOMER', NEWID());
END
GO

-- 2. Variables for standard password hash (Password@123)
DECLARE @Hash NVARCHAR(MAX) = N'AQAAAAIAAYagAAAAEKEXfsO0Hp7xbVftfaMzF3nQ5/U6xbL9lpBD/FUXXBNMixVEVbSCVmgds5HY0LN8AQ==';
DECLARE @SecStamp NVARCHAR(MAX) = N'DJUGDOZAFUWKT4SUQ6DEI6DR4753NKRZ';
DECLARE @ConStamp NVARCHAR(MAX) = N'c3963ca0-5e1c-4cc1-ad98-a77ca3df9edd';

-- 3. Insert Users (Admin, Warehouse, Staff)
-- Lưu ý: Id được sinh bằng NEWID()
INSERT INTO [AspNetUsers] 
([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], [FullName], [Status], [FPoints], [WalletBalance], [BranchId], [CreatedAt])
VALUES 
-- ADMIN
(NEWID(), N'superadmin', N'SUPERADMIN', N'superadmin@bookstore.com', N'SUPERADMIN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Nguyễn Nhật Anh (Tổng GĐ)', 1, 0, 0, NULL, GETDATE()),
(NEWID(), N'admin_hn', N'ADMIN_HN', N'admin_hn@bookstore.com', N'ADMIN_HN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Trưởng Khoa (Admin HN)', 1, 0, 0, 1, GETDATE()),
(NEWID(), N'admin_dn', N'ADMIN_DN', N'admin_dn@bookstore.com', N'ADMIN_DN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Lê Hải (Admin ĐN)', 1, 0, 0, 2, GETDATE()),
(NEWID(), N'admin_hcm', N'ADMIN_HCM', N'admin_hcm@bookstore.com', N'ADMIN_HCM@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Trần Khắc (Admin HCM)', 1, 0, 0, 3, GETDATE()),
-- WAREHOUSE
(NEWID(), N'warehouse_hn', N'WAREHOUSE_HN', N'warehouse_hn@bookstore.com', N'WAREHOUSE_HN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Nguyễn Văn Kho (Kho HN)', 1, 0, 0, 1, GETDATE()),
(NEWID(), N'warehouse_dn', N'WAREHOUSE_DN', N'warehouse_dn@bookstore.com', N'WAREHOUSE_DN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Lê Văn Kho (Kho ĐN)', 1, 0, 0, 2, GETDATE()),
(NEWID(), N'warehouse_hcm', N'WAREHOUSE_HCM', N'warehouse_hcm@bookstore.com', N'WAREHOUSE_HCM@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Trần Văn Kho (Kho HCM)', 1, 0, 0, 3, GETDATE()),
-- STAFF
(NEWID(), N'staff_hn', N'STAFF_HN', N'staff_hn@bookstore.com', N'STAFF_HN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Phạm Thị Bán (Staff HN)', 1, 0, 0, 1, GETDATE()),
(NEWID(), N'staff_dn', N'STAFF_DN', N'staff_dn@bookstore.com', N'STAFF_DN@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Ngô Thị Bán (Staff ĐN)', 1, 0, 0, 2, GETDATE()),
(NEWID(), N'staff_hcm', N'STAFF_HCM', N'staff_hcm@bookstore.com', N'STAFF_HCM@BOOKSTORE.COM', 1, @Hash, @SecStamp, @ConStamp, 0, 0, 1, 0, N'Vũ Thị Bán (Staff HCM)', 1, 0, 0, 3, GETDATE());
GO

-- 4. Assign Roles in AspNetUserRoles
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] FROM [AspNetUsers] u, [AspNetRoles] r
WHERE u.[UserName] IN (N'superadmin', N'admin_hn', N'admin_dn', N'admin_hcm') AND r.[Name] = N'Admin';

INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] FROM [AspNetUsers] u, [AspNetRoles] r
WHERE u.[UserName] IN (N'warehouse_hn', N'warehouse_dn', N'warehouse_hcm') AND r.[Name] = N'Warehouse';

INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] FROM [AspNetUsers] u, [AspNetRoles] r
WHERE u.[UserName] IN (N'staff_hn', N'staff_dn', N'staff_hcm') AND r.[Name] = N'Staff';
GO

PRINT N'Tạo dữ liệu Seed thành công!';
