using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Thông báo cho người dùng — Mapped to [dbo].[Notifications] from script.sql.
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [Column("notification_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Nội dung")]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Liên kết")]
        [Column("link", TypeName = "varchar(255)")]
        public string? Link { get; set; }

        [Display(Name = "Đã đọc")]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
