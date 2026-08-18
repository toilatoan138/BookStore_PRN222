using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Mã giảm giá — Mapped to [dbo].[Vouchers] from script.sql.
    /// </summary>
    [Table("Vouchers")]
    public class Voucher
    {
        [Key]
        [Column("voucher_id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã voucher là bắt buộc")]
        [StringLength(50)]
        [Display(Name = "Mã voucher")]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("discount_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền giảm")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, 100)]
        [Display(Name = "Phần trăm giảm")]
        [Column("discount_percent")]
        public int DiscountPercent { get; set; } = 0;

        [Column("min_order_value", TypeName = "decimal(18,2)")]
        [Display(Name = "Giá trị đơn tối thiểu")]
        public decimal MinOrderValue { get; set; } = 0;

        [Column("max_discount", TypeName = "decimal(18,2)")]
        [Display(Name = "Giảm tối đa")]
        public decimal? MaxDiscount { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Giới hạn sử dụng")]
        [Column("usage_limit")]
        public int UsageLimit { get; set; } = 0;

        [Display(Name = "Trạng thái")]
        [Column("status")]
        public int Status { get; set; } = 1; // 1=Active, 0=Inactive

        // Navigation
        public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
