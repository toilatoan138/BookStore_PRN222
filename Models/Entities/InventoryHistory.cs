using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Lịch sử biến động tồn kho — Mapped to [dbo].[Inventory_History] from script.sql.
    /// </summary>
    [Table("Inventory_History")]
    public class InventoryHistory
    {
        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại giao dịch")]
        [Column("transaction_type", TypeName = "varchar(20)")]
        public string TransactionType { get; set; } = string.Empty; // IMPORT, EXPORT, ADJUSTMENT, RETURN

        [Display(Name = "Số lượng thay đổi")]
        [Column("quantity_changed")]
        public int QuantityChanged { get; set; }

        [Display(Name = "ID liên quan")]
        [Column("related_id")]
        public int? RelatedId { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public string? CreatedById { get; set; }

        // FK
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }
    }
}
