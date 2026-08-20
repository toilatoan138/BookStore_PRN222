using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Lịch sử điểm thưởng F-Point — Mapped to [dbo].[FPoint_History] from script.sql.
    /// </summary>
    [Table("FPoint_History")]
    public class FPointHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [StringLength(200)]
        [Display(Name = "Thông tin khách hàng")]
        [Column("customer_info")]
        public string? CustomerInfo { get; set; } // VD: "@quang (#1011)"

        [Required]
        [StringLength(10)]
        [Display(Name = "Loại")]
        [Column("action_type", TypeName = "varchar(10)")]
        public string ActionType { get; set; } = string.Empty; // "add" hoặc "sub"

        [Required]
        [Display(Name = "Số điểm")]
        [Column("amount")]
        public int Amount { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do")]
        [Column("reason")]
        public string? Reason { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
