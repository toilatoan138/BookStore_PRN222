using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Nhà cung cấp — Mapped to [dbo].[Suppliers] from script.sql.
    /// </summary>
    public class Supplier
    {
        [Key]
        [Column("supplier_id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
        [StringLength(255)]
        [Display(Name = "Tên nhà cung cấp")]
        [Column("supplier_name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Người liên hệ")]
        [Column("contact_person")]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        [Column("phone")]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        [Column("email")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        [Column("address")]
        public string? Address { get; set; }

        [Display(Name = "Hoạt động")]
        [Column("is_active")]
        public bool? IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
