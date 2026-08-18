using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Ghi chú khách hàng (Staff note) — Mapped to [dbo].[Customer_Notes] from script.sql.
    /// </summary>
    [Table("Customer_Notes")]
    public class CustomerNote
    {
        [Key]
        [Column("note_id")]
        public int NoteId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [StringLength(100)]
        [Display(Name = "Kênh liên hệ")]
        [Column("contact_channel")]
        public string? ContactChannel { get; set; }

        [Required(ErrorMessage = "Nội dung ghi chú là bắt buộc")]
        [StringLength(2000)]
        [Display(Name = "Nội dung ghi chú")]
        [Column("note_content")]
        public string NoteContent { get; set; } = string.Empty;

        [Display(Name = "Ngày theo dõi")]
        [DataType(DataType.Date)]
        [Column("follow_up_date")]
        public DateTime? FollowUpDate { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("create_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
