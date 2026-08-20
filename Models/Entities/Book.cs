using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Sách — Mapped to [dbo].[Books] from script.sql.
    /// </summary>
    [Table("Books")]
    public class Book
    {
        [Key]
        [Column("book_id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không quá 255 ký tự")]
        [Display(Name = "Tiêu đề")]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Tác giả")]
        [Column("author")]
        public string? Author { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
        [Column("price", TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Column("cost_price", TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal ImportPrice { get; set; } = 0;

        [Display(Name = "Mô tả")]
        [Column("description")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh bìa")]
        [Column("image", TypeName = "varchar(max)")]
        public string? ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "Ảnh bìa phụ")]
        public string? CoverImage { get => ImageUrl; set => ImageUrl = value; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Tồn kho")]
        [Column("stock_quantity")]
        public int StockQuantity { get; set; } = 0;

        [Range(0, int.MaxValue)]
        [Display(Name = "Đã bán")]
        [Column("sold_quantity")]
        public int SoldQuantity { get; set; } = 0;

        [StringLength(100)]
        [Display(Name = "Nhà xuất bản")]
        [Column("publisher")]
        public string? Publisher { get; set; }

        [StringLength(100)]
        [Display(Name = "Nhà cung cấp")]
        [Column("supplier")]
        public string? Supplier { get; set; }

        [StringLength(20)]
        [Display(Name = "ISBN")]
        [Column("ISBN", TypeName = "varchar(20)")]
        public string? Isbn { get; set; }

        [Display(Name = "Năm xuất bản")]
        [Range(1800, 2100)]
        [Column("yearOfPublish")]
        public int? YearOfPublish { get; set; }

        [Display(Name = "Số trang")]
        [Range(1, int.MaxValue)]
        [Column("number_page")]
        public int? NumberOfPages { get; set; }

        [Display(Name = "Kích hoạt")]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Vị trí kho")]
        [Column("location_id")]
        public int? LocationId { get; set; }

        [NotMapped]
        [Display(Name = "Mã vị trí kho")]
        public string? LocationCode
        {
            get => Location?.LocationCode;
            set { }
        }

        // FK → Category
        [Display(Name = "Danh mục")]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier? SupplierEntity { get; set; }

        // Navigation Properties
        public virtual ICollection<BookImage> DetailImages { get; set; } = new List<BookImage>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
        public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();
        public virtual ICollection<CollectionBook> CollectionBooks { get; set; } = new List<CollectionBook>();
        public virtual ICollection<PromotionBook> PromotionBooks { get; set; } = new List<PromotionBook>();
    }
}
