using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Lịch sử giao dịch ví — Mapped to [dbo].[Wallet_History] from script.sql.
    /// </summary>
    [Table("Wallet_History")]
    public class WalletHistory
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại giao dịch")]
        [Column("transaction_type", TypeName = "varchar(50)")]
        public string TransactionType { get; set; } = string.Empty; // REFUND, PAYMENT

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        [Column("description")]
        public string? Description { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
