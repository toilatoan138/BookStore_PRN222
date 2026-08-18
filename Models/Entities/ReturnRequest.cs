using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Yêu cầu trả hàng — Mapped to [dbo].[ReturnRequests] from script.sql.
    /// </summary>
    [Table("ReturnRequests")]
    public class ReturnRequest
    {
        [Key]
        [Column("return_id")]
        public int ReturnId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Số lượng trả")]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Lý do trả hàng là bắt buộc")]
        [StringLength(500)]
        [Display(Name = "Lý do khách hàng")]
        [Column("customer_reason")]
        public string CustomerReason { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Phương thức trả")]
        [Column("return_method")]
        public string? ReturnMethod { get; set; }

        [StringLength(50)]
        [Display(Name = "Ưu tiên hoàn tiền")]
        [Column("refund_preference")]
        public string? RefundPreference { get; set; }

        [Display(Name = "Trạng thái")]
        [Column("status")]
        public int Status { get; set; } = 0; // 0=Pending, 1=Approved, 2=Rejected, 3=Completed

        [StringLength(500)]
        [Display(Name = "Ghi chú admin")]
        [Column("admin_note")]
        public string? AdminNote { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        [Display(Name = "Ảnh chứng minh")]
        [Column("proof_image")]
        public string? ProofImage { get; set; }

        [StringLength(50)]
        [Column("image_mime_type")]
        public string? ImageMimeType { get; set; }

        [StringLength(100)]
        [Display(Name = "Tên ngân hàng")]
        [Column("bank_name")]
        public string? BankName { get; set; }

        [StringLength(50)]
        [Display(Name = "Số tài khoản")]
        [Column("account_number")]
        public string? AccountNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Chủ tài khoản")]
        [Column("account_owner")]
        public string? AccountOwner { get; set; }

        [Display(Name = "Ngày duyệt")]
        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("evidence_image")]
        public byte[]? EvidenceImage { get; set; }

        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá sản phẩm")]
        public decimal Price { get; set; }

        [NotMapped]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Hoàn tiền tối đa")]
        public decimal MaxRefundableAmount { get; set; }

        // FK
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;

        // Navigation
        public virtual ICollection<RefundTransaction> RefundTransactions { get; set; } = new List<RefundTransaction>();
    }
}
