-- ============================================================================
-- script.sql — Auto-generated từ EF Core Migrations
-- Nguồn: Migrations/*, sinh bởi ApplicationDbContext.
-- Đảm bảo 100% khớp Table/Column với các Entity Class trong Models/Entities.
--
-- Script này idempotent — chạy lại nhiều lần trên cùng 1 DB đều an toàn
-- (tự kiểm tra bảng __EFMigrationsHistory trước khi tạo lại).
-- ============================================================================
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AdminNotifications] (
        [notification_id] int NOT NULL IDENTITY,
        [message] nvarchar(255) NOT NULL,
        [is_read] bit NULL,
        [created_at] datetime NULL,
        [link] varchar(255) NULL,
        CONSTRAINT [PK_AdminNotifications] PRIMARY KEY ([notification_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(128) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(128) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Status] bit NOT NULL,
        [CreatedAt] datetime NOT NULL,
        [TotalSpend] decimal(18,2) NOT NULL,
        [Tags] nvarchar(500) NULL,
        [FPoints] int NOT NULL,
        [WalletBalance] decimal(18,2) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [category_id] int NOT NULL IDENTITY,
        [category_name] nvarchar(100) NOT NULL,
        [category_image] varchar(max) NULL,
        [description] nvarchar(max) NULL,
        [parent_id] int NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([category_id]),
        CONSTRAINT [FK_Categories_Categories_parent_id] FOREIGN KEY ([parent_id]) REFERENCES [Categories] ([category_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Promotions] (
        [promo_id] int NOT NULL IDENTITY,
        [promo_name] nvarchar(255) NOT NULL,
        [discount_percent] int NOT NULL,
        [start_date] datetime NOT NULL,
        [end_date] datetime NOT NULL,
        [is_active] bit NOT NULL,
        CONSTRAINT [PK_Promotions] PRIMARY KEY ([promo_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [supplier_id] int NOT NULL IDENTITY,
        [supplier_name] nvarchar(255) NOT NULL,
        [contact_person] nvarchar(100) NULL,
        [phone] varchar(20) NULL,
        [email] varchar(100) NULL,
        [address] nvarchar(max) NULL,
        [is_active] bit NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([supplier_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Vouchers] (
        [voucher_id] int NOT NULL IDENTITY,
        [code] varchar(50) NOT NULL,
        [discount_amount] decimal(18,2) NOT NULL,
        [discount_percent] int NOT NULL,
        [min_order_value] decimal(18,2) NOT NULL,
        [max_discount] decimal(18,2) NULL,
        [start_date] datetime NOT NULL,
        [end_date] datetime NOT NULL,
        [usage_limit] int NOT NULL,
        [status] int NOT NULL,
        CONSTRAINT [PK_Vouchers] PRIMARY KEY ([voucher_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Addresses] (
        [address_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [fullname] nvarchar(100) NOT NULL,
        [phone] varchar(20) NOT NULL,
        [city] nvarchar(100) NOT NULL,
        [district] nvarchar(100) NOT NULL,
        [ward] nvarchar(100) NOT NULL,
        [address_detail] nvarchar(300) NOT NULL,
        [is_default_billing] bit NOT NULL,
        [is_default_shipping] bit NOT NULL,
        CONSTRAINT [PK_Addresses] PRIMARY KEY ([address_id]),
        CONSTRAINT [FK_Addresses_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(128) NOT NULL,
        [RoleId] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(128) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Cart] (
        [cart_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [create_at] datetime NOT NULL,
        CONSTRAINT [PK_Cart] PRIMARY KEY ([cart_id]),
        CONSTRAINT [FK_Cart_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Collections] (
        [collection_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [collection_name] nvarchar(100) NOT NULL,
        [description] nvarchar(500) NULL,
        [is_public] bit NOT NULL,
        [cover_color] varchar(20) NULL,
        [created_at] datetime NOT NULL,
        CONSTRAINT [PK_Collections] PRIMARY KEY ([collection_id]),
        CONSTRAINT [FK_Collections_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Customer_Notes] (
        [note_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [contact_channel] varchar(50) NULL,
        [note_content] nvarchar(2000) NOT NULL,
        [follow_up_date] date NULL,
        [create_at] datetime NOT NULL,
        CONSTRAINT [PK_Customer_Notes] PRIMARY KEY ([note_id]),
        CONSTRAINT [FK_Customer_Notes_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [FPoint_History] (
        [history_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [customer_info] nvarchar(200) NULL,
        [action_type] varchar(10) NOT NULL,
        [amount] int NOT NULL,
        [reason] nvarchar(500) NULL,
        [created_at] datetime NOT NULL,
        CONSTRAINT [PK_FPoint_History] PRIMARY KEY ([history_id]),
        CONSTRAINT [FK_FPoint_History_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [notification_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [message] nvarchar(500) NOT NULL,
        [link] varchar(255) NULL,
        [is_read] bit NOT NULL,
        [created_at] datetime NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([notification_id]),
        CONSTRAINT [FK_Notifications_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Support_Tickets] (
        [ticket_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [issue_type] nvarchar(100) NOT NULL,
        [ticket_subject] nvarchar(200) NOT NULL,
        [ticket_message] nvarchar(2000) NOT NULL,
        [status] nvarchar(50) NOT NULL,
        [admin_reply] nvarchar(2000) NULL,
        [created_at] datetime NOT NULL,
        CONSTRAINT [PK_Support_Tickets] PRIMARY KEY ([ticket_id]),
        CONSTRAINT [FK_Support_Tickets_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Warehouse_Locations] (
        [location_id] int NOT NULL IDENTITY,
        [zone] varchar(10) NOT NULL,
        [rack] varchar(10) NOT NULL,
        [shelf] varchar(10) NOT NULL,
        [location_code] AS (((([zone]+'-')+[rack])+'-')+[shelf]) PERSISTED,
        [category_id] int NULL,
        [description] nvarchar(max) NULL,
        [max_capacity] int NULL,
        CONSTRAINT [PK_Warehouse_Locations] PRIMARY KEY ([location_id]),
        CONSTRAINT [FK_Warehouse_Locations_Categories_category_id] FOREIGN KEY ([category_id]) REFERENCES [Categories] ([category_id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Purchase_Orders] (
        [purchase_order_id] int NOT NULL IDENTITY,
        [supplier_id] int NOT NULL,
        [user_id] nvarchar(128) NULL,
        [approved_by] nvarchar(128) NULL,
        [order_date] datetime NOT NULL,
        [total_quantity] int NOT NULL,
        [total_amount] decimal(18,2) NOT NULL,
        [status] int NOT NULL,
        [status_note] nvarchar(500) NULL,
        CONSTRAINT [PK_Purchase_Orders] PRIMARY KEY ([purchase_order_id]),
        CONSTRAINT [FK_Purchase_Orders_AspNetUsers_approved_by] FOREIGN KEY ([approved_by]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Purchase_Orders_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Purchase_Orders_Suppliers_supplier_id] FOREIGN KEY ([supplier_id]) REFERENCES [Suppliers] ([supplier_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [order_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [order_date] datetime NOT NULL,
        [total_amount] decimal(18,2) NOT NULL,
        [status] int NOT NULL,
        [shipping_address] nvarchar(500) NULL,
        [phone_number] varchar(15) NULL,
        [receiver_name] nvarchar(100) NULL,
        [payment_method] nvarchar(50) NULL,
        [status_note] nvarchar(500) NULL,
        [shipping_fee] decimal(18,2) NOT NULL,
        [discount_amount] decimal(18,2) NOT NULL,
        [voucher_id] int NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([order_id]),
        CONSTRAINT [FK_Orders_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Orders_Vouchers_voucher_id] FOREIGN KEY ([voucher_id]) REFERENCES [Vouchers] ([voucher_id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [User_Vouchers] (
        [user_id] nvarchar(128) NOT NULL,
        [voucher_id] int NOT NULL,
        [is_used] bit NOT NULL,
        [saved_date] datetime NOT NULL,
        CONSTRAINT [PK_User_Vouchers] PRIMARY KEY ([user_id], [voucher_id]),
        CONSTRAINT [FK_User_Vouchers_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_User_Vouchers_Vouchers_voucher_id] FOREIGN KEY ([voucher_id]) REFERENCES [Vouchers] ([voucher_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Books] (
        [book_id] int NOT NULL IDENTITY,
        [title] nvarchar(255) NOT NULL,
        [author] nvarchar(100) NULL,
        [price] decimal(18,2) NOT NULL,
        [cost_price] decimal(18,2) NOT NULL,
        [description] nvarchar(max) NULL,
        [image] varchar(max) NULL,
        [stock_quantity] int NOT NULL,
        [sold_quantity] int NOT NULL,
        [publisher] nvarchar(100) NULL,
        [supplier] nvarchar(100) NULL,
        [ISBN] varchar(20) NULL,
        [yearOfPublish] int NULL,
        [number_page] int NULL,
        [is_active] bit NOT NULL,
        [location_id] int NULL,
        [category_id] int NOT NULL,
        [supplier_id] int NULL,
        CONSTRAINT [PK_Books] PRIMARY KEY ([book_id]),
        CONSTRAINT [FK_Books_Categories_category_id] FOREIGN KEY ([category_id]) REFERENCES [Categories] ([category_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Books_Suppliers_supplier_id] FOREIGN KEY ([supplier_id]) REFERENCES [Suppliers] ([supplier_id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Books_Warehouse_Locations_location_id] FOREIGN KEY ([location_id]) REFERENCES [Warehouse_Locations] ([location_id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [invoice_id] int NOT NULL IDENTITY,
        [invoice_type] varchar(20) NOT NULL,
        [order_id] int NULL,
        [purchase_order_id] int NULL,
        [created_date] datetime NOT NULL,
        [total_amount] decimal(18,2) NOT NULL,
        [status] varchar(50) NOT NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([invoice_id]),
        CONSTRAINT [FK_Invoices_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([order_id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Invoices_Purchase_Orders_purchase_order_id] FOREIGN KEY ([purchase_order_id]) REFERENCES [Purchase_Orders] ([purchase_order_id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Wallet_History] (
        [transaction_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [amount] decimal(18,2) NOT NULL,
        [transaction_type] varchar(50) NOT NULL,
        [description] nvarchar(500) NULL,
        [order_id] int NULL,
        [created_at] datetime NOT NULL,
        CONSTRAINT [PK_Wallet_History] PRIMARY KEY ([transaction_id]),
        CONSTRAINT [FK_Wallet_History_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Wallet_History_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([order_id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [BookImages] (
        [image_id] int NOT NULL IDENTITY,
        [book_id] int NOT NULL,
        [image_url] varchar(255) NOT NULL,
        CONSTRAINT [PK_BookImages] PRIMARY KEY ([image_id]),
        CONSTRAINT [FK_BookImages_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [CartItems] (
        [cart_item_id] int NOT NULL IDENTITY,
        [cart_id] int NOT NULL,
        [book_id] int NOT NULL,
        [quantity] int NOT NULL,
        [add_at] datetime NOT NULL,
        CONSTRAINT [PK_CartItems] PRIMARY KEY ([cart_item_id]),
        CONSTRAINT [FK_CartItems_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartItems_Cart_cart_id] FOREIGN KEY ([cart_id]) REFERENCES [Cart] ([cart_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Collection_Books] (
        [collection_id] int NOT NULL,
        [book_id] int NOT NULL,
        [added_at] datetime NOT NULL,
        CONSTRAINT [PK_Collection_Books] PRIMARY KEY ([collection_id], [book_id]),
        CONSTRAINT [FK_Collection_Books_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Collection_Books_Collections_collection_id] FOREIGN KEY ([collection_id]) REFERENCES [Collections] ([collection_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Inventory_History] (
        [history_id] int NOT NULL IDENTITY,
        [book_id] int NOT NULL,
        [transaction_type] varchar(20) NOT NULL,
        [quantity_changed] int NOT NULL,
        [related_id] int NULL,
        [created_at] datetime NOT NULL,
        [created_by] nvarchar(128) NULL,
        CONSTRAINT [PK_Inventory_History] PRIMARY KEY ([history_id]),
        CONSTRAINT [FK_Inventory_History_AspNetUsers_created_by] FOREIGN KEY ([created_by]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Inventory_History_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderDetails] (
        [order_detail_id] int NOT NULL IDENTITY,
        [order_id] int NOT NULL,
        [book_id] int NOT NULL,
        [quantity] int NOT NULL,
        [price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([order_detail_id]),
        CONSTRAINT [FK_OrderDetails_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderDetails_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([order_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Promotion_Books] (
        [promo_id] int NOT NULL,
        [book_id] int NOT NULL,
        CONSTRAINT [PK_Promotion_Books] PRIMARY KEY ([promo_id], [book_id]),
        CONSTRAINT [FK_Promotion_Books_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Promotion_Books_Promotions_promo_id] FOREIGN KEY ([promo_id]) REFERENCES [Promotions] ([promo_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Purchase_Order_Details] (
        [po_detail_id] int NOT NULL IDENTITY,
        [purchase_order_id] int NOT NULL,
        [book_id] int NOT NULL,
        [expected_quantity] int NOT NULL,
        [received_quantity] int NOT NULL,
        [price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Purchase_Order_Details] PRIMARY KEY ([po_detail_id]),
        CONSTRAINT [FK_Purchase_Order_Details_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Purchase_Order_Details_Purchase_Orders_purchase_order_id] FOREIGN KEY ([purchase_order_id]) REFERENCES [Purchase_Orders] ([purchase_order_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [ReturnRequests] (
        [return_id] int NOT NULL IDENTITY,
        [order_id] int NOT NULL,
        [book_id] int NOT NULL,
        [quantity] int NOT NULL,
        [customer_reason] nvarchar(500) NOT NULL,
        [return_method] nvarchar(50) NULL,
        [refund_preference] nvarchar(50) NULL,
        [status] int NOT NULL,
        [admin_note] nvarchar(500) NULL,
        [created_at] datetime NOT NULL,
        [proof_image] nvarchar(500) NULL,
        [image_mime_type] varchar(50) NULL,
        [bank_name] nvarchar(100) NULL,
        [account_number] varchar(50) NULL,
        [account_owner] nvarchar(100) NULL,
        [approved_at] datetime NULL,
        [evidence_image] varbinary(max) NULL,
        CONSTRAINT [PK_ReturnRequests] PRIMARY KEY ([return_id]),
        CONSTRAINT [FK_ReturnRequests_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ReturnRequests_Orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [Orders] ([order_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Review] (
        [review_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(128) NULL,
        [book_id] int NOT NULL,
        [rating] int NOT NULL,
        [comment] nvarchar(2000) NULL,
        [create_at] datetime NOT NULL,
        [staff_reply] nvarchar(2000) NULL,
        CONSTRAINT [PK_Review] PRIMARY KEY ([review_id]),
        CONSTRAINT [FK_Review_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Review_Books_book_id] FOREIGN KEY ([book_id]) REFERENCES [Books] ([book_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [RefundTransactions] (
        [transaction_id] int NOT NULL IDENTITY,
        [return_id] int NOT NULL,
        [refund_amount] decimal(18,2) NOT NULL,
        [bank_reference] varchar(255) NULL,
        [processed_by] nvarchar(100) NULL,
        [processed_at] datetime NULL,
        [admin_note] nvarchar(max) NULL,
        CONSTRAINT [PK_RefundTransactions] PRIMARY KEY ([transaction_id]),
        CONSTRAINT [FK_RefundTransactions_ReturnRequests_return_id] FOREIGN KEY ([return_id]) REFERENCES [ReturnRequests] ([return_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE TABLE [Reported_Reviews] (
        [report_id] int NOT NULL IDENTITY,
        [review_id] int NULL,
        [user_id] nvarchar(128) NULL,
        [reason] nvarchar(255) NULL,
        [status] nvarchar(50) NULL,
        [created_at] datetime NULL,
        CONSTRAINT [PK_Reported_Reviews] PRIMARY KEY ([report_id]),
        CONSTRAINT [FK_Reported_Reviews_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Reported_Reviews_Review_review_id] FOREIGN KEY ([review_id]) REFERENCES [Review] ([review_id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Addresses_user_id] ON [Addresses] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BookImages_book_id] ON [BookImages] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_category_id] ON [Books] ([category_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_is_active] ON [Books] ([is_active]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Books_ISBN] ON [Books] ([ISBN]) WHERE [ISBN] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_location_id] ON [Books] ([location_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_supplier_id] ON [Books] ([supplier_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Books_title] ON [Books] ([title]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cart_user_id] ON [Cart] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartItems_book_id] ON [CartItems] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CartItems_cart_id_book_id] ON [CartItems] ([cart_id], [book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_category_name] ON [Categories] ([category_name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Categories_parent_id] ON [Categories] ([parent_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Collection_Books_book_id] ON [Collection_Books] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Collections_user_id] ON [Collections] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customer_Notes_user_id] ON [Customer_Notes] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FPoint_History_user_id] ON [FPoint_History] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventory_History_book_id] ON [Inventory_History] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventory_History_created_at] ON [Inventory_History] ([created_at]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Inventory_History_created_by] ON [Inventory_History] ([created_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_invoice_type] ON [Invoices] ([invoice_type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_order_id] ON [Invoices] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_purchase_order_id] ON [Invoices] ([purchase_order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_user_id] ON [Notifications] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_book_id] ON [OrderDetails] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_order_id] ON [OrderDetails] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_order_date] ON [Orders] ([order_date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_status] ON [Orders] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_user_id] ON [Orders] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Orders_voucher_id] ON [Orders] ([voucher_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Promotion_Books_book_id] ON [Promotion_Books] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Order_Details_book_id] ON [Purchase_Order_Details] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Order_Details_purchase_order_id] ON [Purchase_Order_Details] ([purchase_order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Orders_approved_by] ON [Purchase_Orders] ([approved_by]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Orders_status] ON [Purchase_Orders] ([status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Orders_supplier_id] ON [Purchase_Orders] ([supplier_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Orders_user_id] ON [Purchase_Orders] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefundTransactions_return_id] ON [RefundTransactions] ([return_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reported_Reviews_review_id] ON [Reported_Reviews] ([review_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reported_Reviews_user_id] ON [Reported_Reviews] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReturnRequests_book_id] ON [ReturnRequests] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReturnRequests_order_id] ON [ReturnRequests] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Review_book_id] ON [Review] ([book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Review_user_id_book_id] ON [Review] ([user_id], [book_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Suppliers_supplier_name] ON [Suppliers] ([supplier_name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Support_Tickets_user_id] ON [Support_Tickets] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_User_Vouchers_voucher_id] ON [User_Vouchers] ([voucher_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vouchers_code] ON [Vouchers] ([code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Wallet_History_order_id] ON [Wallet_History] ([order_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Wallet_History_user_id] ON [Wallet_History] ([user_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Warehouse_Locations_category_id] ON [Warehouse_Locations] ([category_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouse_Locations_location_code] ON [Warehouse_Locations] ([location_code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260820034608_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260820034608_InitialCreate', N'8.0.14');
END;
GO

COMMIT;
GO

