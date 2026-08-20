using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookStore.Models.Enums;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Đơn hàng — Mapped to [dbo].[Orders] from script.sql.
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Display(Name = "Ngày đặt")]
        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("total_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái")]
        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Địa chỉ giao hàng")]
        [Column("shipping_address")]
        public string? ShippingAddress { get; set; }

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        [Column("phone_number", TypeName = "varchar(15)")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        [Column("receiver_name")]
        public string? FullName { get; set; }

        [StringLength(50)]
        [Display(Name = "Phương thức thanh toán")]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; } // COD, VNPAY, WALLET

        [StringLength(500)]
        [Display(Name = "Ghi chú trạng thái")]
        [Column("status_note")]
        public string? StatusNote { get; set; }

        [NotMapped]
        [StringLength(500)]
        [Display(Name = "Ghi chú nhân viên")]
        public string? StaffNote { get; set; }

        [Column("shipping_fee", TypeName = "decimal(18,2)")]
        [Display(Name = "Phí ship")]
        public decimal ShippingFee { get; set; } = 0;

        [Column("discount_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền giảm")]
        public decimal DiscountAmount { get; set; } = 0;

        // FK → Voucher (optional)
        [Column("voucher_id")]
        public int? VoucherId { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }

        // FK → User
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation
        public virtual ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
        public virtual ICollection<ReturnRequest> ReturnRequests { get; set; } = new List<ReturnRequest>();
    }
}
