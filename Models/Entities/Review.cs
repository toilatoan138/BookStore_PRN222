using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Đánh giá sách — Mapped to [dbo].[Review] from script.sql.
    /// </summary>
    [Table("Review")]
    public class Review
    {
        [Key]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        [Display(Name = "Số sao")]
        [Column("rating")]
        public int Rating { get; set; } = 5;

        [StringLength(2000)]
        [Display(Name = "Bình luận")]
        [Column("comment")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("create_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(2000)]
        [Display(Name = "Phản hồi nhân viên")]
        [Column("staff_reply")]
        public string? StaffReply { get; set; }

        [NotMapped]
        [Display(Name = "Trạng thái sách khi đánh giá")]
        public int BookStatus { get; set; } = 0;

        [NotMapped]
        [Display(Name = "Số lần bị báo cáo")]
        public int ReportCount { get; set; } = 0;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
