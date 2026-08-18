using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Chi tiết đơn nhập hàng — Mapped to [dbo].[Purchase_Order_Details] from script.sql.
    /// </summary>
    [Table("Purchase_Order_Details")]
    public class PurchaseOrderDetail
    {
        [Key]
        [Column("po_detail_id")]
        public int PoDetailId { get; set; }

        [Required]
        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "SL dự kiến")]
        [Column("expected_quantity")]
        public int ExpectedQuantity { get; set; }

        [Display(Name = "SL thực nhận")]
        [Column("received_quantity")]
        public int ReceivedQuantity { get; set; } = 0;

        [Required]
        [Column("price", TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal Price { get; set; }

        // FK
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
