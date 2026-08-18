namespace BookStore.Models.Enums
{
    /// <summary>
    /// Trạng thái đơn hàng — tương ứng OrderStatus.java
    /// </summary>
    public enum OrderStatus
    {
        Pending = 1,            // Chờ duyệt
        Processing = 2,         // Đang xử lý (Sẵn sàng lấy hàng)
        Packed = 3,             // Đã đóng gói xong
        Shipping = 4,           // Đang giao (Đã xuất kho)
        Delivered = 5,          // Giao thành công
        Cancelled = 6,          // Đã hủy
        ReturnRequested = 7,    // Khách gửi yêu cầu trả hàng
        ReturnApproved = 8,     // Admin đã duyệt
        ReturnReceived = 9,     // Kho đã nhận hàng trả
        ReturnCompleted = 10    // Đã kiểm hàng & nhập kho xong
    }
}
