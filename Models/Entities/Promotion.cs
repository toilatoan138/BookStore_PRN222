using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Chương trình khuyến mãi — Mapped to [dbo].[Promotions] from script.sql.
    /// </summary>
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        [Column("promo_id")]
        public int PromoId { get; set; }

        [Required(ErrorMessage = "Tên chương trình là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Tên chương trình")]
        [Column("promo_name")]
        public string PromoName { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá từ 1 đến 100")]
        [Display(Name = "Phần trăm giảm")]
        [Column("discount_percent")]
        public int DiscountPercent { get; set; }

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Ngày kết thúc")]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Kích hoạt")]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation (N-N qua PromotionBook)
        public virtual ICollection<PromotionBook> PromotionBooks { get; set; } = new List<PromotionBook>();
    }
}
