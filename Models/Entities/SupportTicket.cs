using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Ticket hỗ trợ khách hàng — Mapped to [dbo].[Support_Tickets] from script.sql.
    /// </summary>
    [Table("Support_Tickets")]
    public class SupportTicket
    {
        [Key]
        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Loại vấn đề là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Loại vấn đề")]
        [Column("issue_type")]
        public string IssueType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        [Column("ticket_subject")]
        public string TicketSubject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [StringLength(2000)]
        [Display(Name = "Nội dung")]
        [Column("ticket_message")]
        public string TicketMessage { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        [Column("status")]
        public string Status { get; set; } = "Open"; // Open, In Progress, Closed

        [StringLength(2000)]
        [Display(Name = "Phản hồi admin")]
        [Column("admin_reply")]
        public string? AdminReply { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
