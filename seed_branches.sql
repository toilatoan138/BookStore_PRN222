-- Create default branches
INSERT INTO Branches (Name, Address, City, IsActive)
VALUES 
    (N'BookStore Hà Nội', N'1 Đại Cồ Việt', N'Hà Nội', 1),
    (N'BookStore TP HCM', N'2 Nguyễn Huệ', N'Hồ Chí Minh', 1);

-- Move all current Book.StockQuantity to Branch 1 (Hà Nội)
INSERT INTO Branch_Inventory (BranchId, BookId, StockQuantity)
SELECT 
    (SELECT TOP 1 Id FROM Branches WHERE Name = N'BookStore Hà Nội'), 
    book_id, 
    stock_quantity
FROM Books;

-- Set up some sample quantities in TP HCM for testing Order Splitting
-- For example, add some books to TP HCM so we can have 2 branches with stock.
-- We'll just randomly assign 5 items to TP HCM for half of the books.
INSERT INTO Branch_Inventory (BranchId, BookId, StockQuantity)
SELECT 
    (SELECT TOP 1 Id FROM Branches WHERE Name = N'BookStore TP HCM'), 
    book_id, 
    5
FROM Books
WHERE book_id % 2 = 0; -- just for testing
