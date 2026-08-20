using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Giao dịch hoàn tiền — Mapped to [dbo].[RefundTransactions] from script.sql.
    /// </summary>
    [Table("RefundTransactions")]
    public class RefundTransaction
    {
        [Key]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Required]
        [Column("return_id")]
        public int ReturnId { get; set; }

        [Required]
        [Column("refund_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền hoàn")]
        public decimal RefundAmount { get; set; }

        [StringLength(255)]
        [Display(Name = "Mã tham chiếu ngân hàng")]
        [Column("bank_reference", TypeName = "varchar(255)")]
        public string? BankReference { get; set; }

        [StringLength(100)]
        [Display(Name = "Người xử lý")]
        [Column("processed_by", TypeName = "nvarchar(100)")]
        public string? ProcessedBy { get; set; }

        [Display(Name = "Ngày xử lý")]
        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [Display(Name = "Ghi chú admin")]
        [Column("admin_note")]
        public string? AdminNote { get; set; }

        // FK
        [ForeignKey("ReturnId")]
        public virtual ReturnRequest ReturnRequest { get; set; } = null!;
    }
}
