using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Voucher đã lưu của người dùng — Mapped to [dbo].[User_Vouchers] from script.sql.
    /// </summary>
    [Table("User_Vouchers")]
    public class UserVoucher
    {
        [Required]
        [StringLength(128)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column("voucher_id")]
        public int VoucherId { get; set; }

        [Display(Name = "Đã sử dụng")]
        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Display(Name = "Ngày lưu")]
        [Column("saved_date")]
        public DateTime SavedDate { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; } = null!;
    }
}
