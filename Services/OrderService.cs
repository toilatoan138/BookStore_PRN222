using BookStore.Data;
using BookStore.Models.Entities;
using BookStore.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, IUserService userService, ICartService cartService)
        {
            _context = context;
            _userService = userService;
            _cartService = cartService;
        }

        public async Task<CheckoutCalculationResult> CalculateCheckoutAsync(
            string userId,
            List<int> selectedBookIds,
            string? voucherCode,
            string paymentMethod,
            decimal shippingFee = 30000)
        {
            var result = new CheckoutCalculationResult
            {
                ShippingFee = shippingFee
            };

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return result;

            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.Cart.UserId == userId && selectedBookIds.Contains(ci.BookId))
                .ToListAsync();

            result.SubTotal = cartItems.Sum(ci => ci.Quantity * ci.Book.Price);

            // Free shipping if subtotal >= 299,000đ
            if (result.SubTotal >= 299000)
            {
                result.ShippingFee = 0;
            }

            // Voucher calculation
            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                var code = voucherCode.Trim().ToUpper();
                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code.ToUpper() == code && v.Status == 1 &&
                                              v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow);

                if (voucher != null)
                {
                    var isUsed = await _context.UserVouchers
                        .AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucher.Id && uv.IsUsed);

                    if (isUsed)
                    {
                        result.VoucherMessage = "Voucher này đã được bạn sử dụng.";
                    }
                    else if (result.SubTotal < voucher.MinOrderValue)
                    {
                        result.VoucherMessage = $"Đơn hàng chưa đạt giá trị tối thiểu {voucher.MinOrderValue:N0}đ để áp dụng voucher.";
                    }
                    else
                    {
                        decimal discount = 0;
                        if (voucher.DiscountPercent > 0)
                        {
                            discount = result.SubTotal * (voucher.DiscountPercent / 100.0m);
                        }
                        else if (voucher.DiscountAmount > 0)
                        {
                            discount = voucher.DiscountAmount;
                        }

                        if (voucher.MaxDiscount.HasValue && discount > voucher.MaxDiscount.Value)
                        {
                            discount = voucher.MaxDiscount.Value;
                        }

                        result.VoucherDiscount = Math.Min(discount, result.SubTotal);
                        result.AppliedVoucherId = voucher.Id;
                        result.VoucherMessage = "Áp dụng mã giảm giá thành công!";
                    }
                }
                else
                {
                    result.VoucherMessage = "Mã giảm giá không tồn tại hoặc đã hết hạn.";
                }
            }

            // F-Point rank discount (5% for Silver, 10% for Gold, 15% for Diamond)
            if (paymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase))
            {
                result.FPointDiscount = result.SubTotal * user.DiscountRate;
            }

            decimal totalAfterDiscount = result.SubTotal - result.TotalDiscount;
            if (totalAfterDiscount < 0) totalAfterDiscount = 0;

            result.FinalTotal = totalAfterDiscount + result.ShippingFee;
            return result;
        }

        public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(CreateOrderRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return (false, "Người dùng không tồn tại.", 0);

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId);
            if (address == null) return (false, "Vui lòng chọn địa chỉ nhận hàng.", 0);

            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.Cart.UserId == request.UserId && request.SelectedBookIds.Contains(ci.BookId))
                .ToListAsync();

            if (!cartItems.Any()) return (false, "Không có sản phẩm nào được chọn để thanh toán.", 0);

            // Stock check across branches
            var bookIds = cartItems.Select(ci => ci.BookId).ToList();
            var allInventories = await _context.BranchInventories
                .Where(bi => bookIds.Contains(bi.BookId))
                .ToListAsync();

            foreach (var item in cartItems)
            {
                int totalAvailable = allInventories.Where(bi => bi.BookId == item.BookId).Sum(bi => bi.StockQuantity);
                if (totalAvailable < item.Quantity)
                {
                    return (false, $"Sản phẩm '{item.Book.Title}' chỉ còn {totalAvailable} quyển trên toàn hệ thống.", 0);
                }
            }

            // Calculate totals
            var calc = await CalculateCheckoutAsync(request.UserId, request.SelectedBookIds, request.VoucherCode, request.PaymentMethod, request.ShippingFee);

            // Check Wallet balance if payment method is WALLET
            if (request.PaymentMethod == "WALLET")
            {
                if (user.WalletBalance < calc.FinalTotal)
                {
                    return (false, "Số dư Ví BookStore không đủ để thanh toán đơn hàng này.", 0);
                }
            }

            // Determine initial order status
            int totalQty = cartItems.Sum(ci => ci.Quantity);
            var initialStatus = OrderStatus.Pending;
            if (request.PaymentMethod == "WALLET" || request.PaymentMethod == "VNPAY")
            {
                initialStatus = OrderStatus.Processing; // Online prepaid
            }
            else if (request.PaymentMethod == "COD" && totalQty <= 10 && calc.FinalTotal <= 2000000)
            {
                initialStatus = OrderStatus.Processing; // Auto-approved
            }

            string shippingAddressStr = $"{address.FullName} | {address.Phone} | {address.AddressDetail}, {address.Ward}, {address.District}, {address.City}";

            string orderGroupId = "G-" + DateTime.UtcNow.Ticks.ToString();
            var createdOrders = new List<Order>();

            // Convert cart items to a tracking list (so we can mutate quantities)
            var remainingItems = cartItems.Select(ci => new { 
                BookId = ci.BookId, 
                Quantity = ci.Quantity, 
                Price = ci.Book.Price,
                Book = ci.Book
            }).ToList();
            var tracker = remainingItems.Select(x => new { BookId = x.BookId, Quantity = x.Quantity, Price = x.Price, Book = x.Book }).ToDictionary(x => x.BookId, x => x.Quantity);

            while (tracker.Values.Any(q => q > 0))
            {
                // Find best branch (can fulfill the most items)
                var branchScores = allInventories.GroupBy(bi => bi.BranchId)
                    .Select(g => new
                    {
                        BranchId = g.Key,
                        Fulfillable = g.Sum(bi => Math.Min(bi.StockQuantity, tracker.ContainsKey(bi.BookId) ? tracker[bi.BookId] : 0))
                    })
                    .Where(x => x.Fulfillable > 0)
                    .OrderByDescending(x => x.Fulfillable)
                    .ToList();

                if (!branchScores.Any()) break;

                int bestBranchId = branchScores.First().BranchId;
                
                var subOrder = new Order
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.UtcNow,
                    Status = initialStatus,
                    ShippingAddress = shippingAddressStr,
                    PhoneNumber = address.Phone,
                    FullName = address.FullName,
                    PaymentMethod = request.PaymentMethod,
                    BranchId = bestBranchId,
                    OrderGroupId = orderGroupId
                };

                decimal subTotalAmount = 0;

                foreach (var remainingBookId in tracker.Keys.Where(k => tracker[k] > 0).ToList())
                {
                    var inv = allInventories.FirstOrDefault(bi => bi.BranchId == bestBranchId && bi.BookId == remainingBookId);
                    if (inv != null && inv.StockQuantity > 0)
                    {
                        int fulfillQty = Math.Min(inv.StockQuantity, tracker[remainingBookId]);
                        var bookInfo = remainingItems.First(r => r.BookId == remainingBookId);
                        
                        subOrder.Details.Add(new OrderDetail
                        {
                            BookId = remainingBookId,
                            Quantity = fulfillQty,
                            Price = bookInfo.Price
                        });

                        subTotalAmount += fulfillQty * bookInfo.Price;

                        // Deduct from branch inventory
                        inv.StockQuantity -= fulfillQty;
                        // Deduct from tracking dictionary
                        tracker[remainingBookId] -= fulfillQty;
                        
                        // Deduct main book stock (backward compatibility for views that rely on Book.StockQuantity)
                        bookInfo.Book.StockQuantity -= fulfillQty;
                        bookInfo.Book.SoldQuantity += fulfillQty;

                        // Log EXPORT
                        _context.InventoryHistories.Add(new InventoryHistory
                        {
                            BookId = remainingBookId,
                            CreatedById = request.UserId,
                            TransactionType = "EXPORT",
                            QuantityChanged = -fulfillQty,
                            RelatedId = subOrder.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                if (createdOrders.Count == 0)
                {
                    subOrder.ShippingFee = calc.ShippingFee;
                    subOrder.DiscountAmount = calc.TotalDiscount;
                    subOrder.VoucherId = calc.AppliedVoucherId;
                    subOrder.TotalAmount = subTotalAmount + calc.ShippingFee - calc.TotalDiscount;
                }
                else
                {
                    subOrder.ShippingFee = 0;
                    subOrder.DiscountAmount = 0;
                    subOrder.TotalAmount = subTotalAmount;
                }
                if (subOrder.TotalAmount < 0) subOrder.TotalAmount = 0;

                _context.Orders.Add(subOrder);
                createdOrders.Add(subOrder);
            }

            await _context.SaveChangesAsync();

            // Automatically create SALE Invoices for created orders
            foreach(var o in createdOrders)
            {
                _context.Invoices.Add(new Invoice
                {
                    InvoiceType = "SALE",
                    OrderId = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = (request.PaymentMethod == "WALLET" || request.PaymentMethod == "VNPAY") ? "Paid" : "Pending",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Mark voucher as used if applied
            if (calc.AppliedVoucherId.HasValue)
            {
                var userVoucher = await _context.UserVouchers
                    .FirstOrDefaultAsync(uv => uv.UserId == request.UserId && uv.VoucherId == calc.AppliedVoucherId.Value);

                if (userVoucher != null)
                {
                    userVoucher.IsUsed = true;
                }
                else
                {
                    _context.UserVouchers.Add(new UserVoucher
                    {
                        UserId = request.UserId,
                        VoucherId = calc.AppliedVoucherId.Value,
                        IsUsed = true,
                        SavedDate = DateTime.UtcNow
                    });
                }
            }

            // Award F-Points (1 point per 10,000 VND spent)
            int earnedFPoints = (int)(calc.FinalTotal / 10000);
            if (earnedFPoints > 0)
            {
                user.FPoints += earnedFPoints;
                user.TotalSpend += calc.FinalTotal;

                _context.FPointHistories.Add(new FPointHistory
                {
                    UserId = request.UserId,
                    Amount = earnedFPoints,
                    ActionType = "add",
                    Reason = "Tích điểm khi đặt hàng",
                    CustomerInfo = user.FullName,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var firstOrder = createdOrders.First();

            // Deduct Wallet balance if WALLET payment
            if (request.PaymentMethod == "WALLET")
            {
                await _userService.UpdateWalletBalanceAsync(
                    user.Id,
                    -calc.FinalTotal,
                    "PAYMENT",
                    $"Thanh toán đơn hàng {(createdOrders.Count > 1 ? orderGroupId : "#" + firstOrder.Id)} bằng Ví BookStore",
                    firstOrder.Id
                );
            }

            // Remove purchased items from cart
            await _cartService.RemovePurchasedItemsAsync(request.UserId, request.SelectedBookIds);

            // Add notification
            string msg = createdOrders.Count > 1 
                ? $"Giỏ hàng đã được tách thành {createdOrders.Count} kiện (Mã nhóm {orderGroupId}) do nằm ở nhiều kho. Tổng tiền: {calc.FinalTotal:N0}đ"
                : $"Đơn hàng #{firstOrder.Id} đã được đặt thành công. Tổng tiền: {calc.FinalTotal:N0}đ";

            _context.Notifications.Add(new Notification
            {
                UserId = request.UserId,
                Message = msg,
                Link = $"/Orders",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return (true, createdOrders.Count > 1 ? "Giỏ hàng đã được tách làm nhiều đơn do hết hàng ở kho gần nhất." : "Đặt hàng thành công.", firstOrder.Id);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId, string? userId = null)
        {
            var query = _context.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .Include(o => o.Voucher)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.UserId == userId);
            }

            return await query.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId, OrderStatus? status = null)
        {
            var query = _context.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId, string reason)
        {
            var order = await _context.Orders
                .Include(o => o.Details)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing))
            {
                return false; // Can only cancel if Pending or Processing
            }

            order.Status = OrderStatus.Cancelled;
            order.StatusNote = $"Khách hàng hủy đơn: {reason}";

            // Restore book stock & log inventory return
            foreach (var detail in order.Details)
            {
                detail.Book.StockQuantity += detail.Quantity;
                detail.Book.SoldQuantity = Math.Max(0, detail.Book.SoldQuantity - detail.Quantity);

                if (order.BranchId.HasValue)
                {
                    var branchInv = await _context.BranchInventories
                        .FirstOrDefaultAsync(bi => bi.BranchId == order.BranchId.Value && bi.BookId == detail.BookId);
                    if (branchInv != null)
                    {
                        branchInv.StockQuantity += detail.Quantity;
                    }
                }

                _context.InventoryHistories.Add(new InventoryHistory
                {
                    BookId = detail.BookId,
                    CreatedById = userId,
                    TransactionType = "ADJUSTMENT",
                    QuantityChanged = detail.Quantity,
                    RelatedId = order.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Refund to wallet if paid via WALLET or VNPAY
            if (order.PaymentMethod == "WALLET" || order.PaymentMethod == "VNPAY")
            {
                await _userService.UpdateWalletBalanceAsync(
                    userId,
                    order.TotalAmount,
                    "REFUND",
                    $"Hoàn tiền đơn hàng #{order.Id} do hủy đơn",
                    order.Id
                );
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? note = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            if (!string.IsNullOrEmpty(note))
            {
                order.StatusNote = note;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
