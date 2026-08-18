using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Báo cáo đánh giá vi phạm — Mapped to [dbo].[Reported_Reviews] from script.sql.
    /// </summary>
    [Table("Reported_Reviews")]
    public class ReportedReview
    {
        [Key]
        [Column("report_id")]
        public int ReportId { get; set; }

        [Column("review_id")]
        public int? ReviewId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [StringLength(255)]
        [Display(Name = "Lý do báo cáo")]
        [Column("reason")]
        public string? Reason { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        [Column("status")]
        public string? Status { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("ReviewId")]
        public virtual Review? Review { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
