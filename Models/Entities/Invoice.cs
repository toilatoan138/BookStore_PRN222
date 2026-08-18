using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Hóa đơn (bán hàng / nhập hàng) — Mapped to [dbo].[Invoices] from script.sql.
    /// </summary>
    [Table("Invoices")]
    public class Invoice
    {
        [Key]
        [Column("invoice_id")]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại hóa đơn")]
        [Column("invoice_type")]
        public string InvoiceType { get; set; } = string.Empty; // SALE, PURCHASE

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Column("purchase_order_id")]
        public int? PurchaseOrderId { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("total_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        [Column("status")]
        public string Status { get; set; } = string.Empty;

        // FK
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        // Helper
        [NotMapped]
        public int RelatedOrderId =>
            InvoiceType == "SALE" && OrderId.HasValue ? OrderId.Value :
            InvoiceType == "PURCHASE" && PurchaseOrderId.HasValue ? PurchaseOrderId.Value : -1;
    }
}
