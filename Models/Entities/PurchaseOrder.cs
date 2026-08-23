using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Đơn nhập hàng — Mapped to [dbo].[Purchase_Orders] from script.sql.
    /// </summary>
    [Table("Purchase_Orders")]
    public class PurchaseOrder
    {
        [Key]
        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }

        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Required]
        [Column("branch_id")]
        [Display(Name = "Chi nhánh nhập hàng")]
        public int BranchId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; } // Người tạo (Warehouse)

        [Column("approved_by")]
        public string? ApprovedById { get; set; } // Người duyệt (Admin)

        [Display(Name = "Ngày đặt")]
        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Tổng số lượng")]
        [Column("total_quantity")]
        public int TotalQuantity { get; set; } = 0;

        [Required]
        [Column("total_amount", TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái")]
        [Column("status")]
        public int Status { get; set; } = 0; // 0=Pending, 1=Approved, 2=Received, 3=Cancelled

        [StringLength(500)]
        [Display(Name = "Ghi chú trạng thái")]
        [Column("status_note")]
        public string? StatusNote { get; set; }

        // FK
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual ApplicationUser? ApprovedBy { get; set; }

        // Navigation
        public virtual ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    }
}
