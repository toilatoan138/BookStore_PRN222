using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Thông báo dành cho Admin — Mapped to [dbo].[AdminNotifications] from script.sql.
    /// </summary>
    public class AdminNotification
    {
        [Key]
        [Column("notification_id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Nội dung")]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Đã đọc")]
        [Column("is_read")]
        public bool? IsRead { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        [Display(Name = "Liên kết")]
        [Column("link")]
        public string? Link { get; set; }
    }
}
