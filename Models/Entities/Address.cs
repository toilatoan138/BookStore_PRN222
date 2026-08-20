using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Địa chỉ giao hàng — Mapped to [dbo].[Addresses] from script.sql.
    /// </summary>
    [Table("Addresses")]
    public class Address
    {
        [Key]
        [Column("address_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ tên người nhận")]
        [Column("fullname")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        [Column("phone", TypeName = "varchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tỉnh/Thành phố")]
        [Column("city")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Quận/Huyện")]
        [Column("district")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Phường/Xã")]
        [Column("ward")]
        public string Ward { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ chi tiết là bắt buộc")]
        [StringLength(300)]
        [Display(Name = "Địa chỉ chi tiết")]
        [Column("address_detail")]
        public string AddressDetail { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ thanh toán mặc định")]
        [Column("is_default_billing")]
        public bool IsDefaultBilling { get; set; } = false;

        [Display(Name = "Địa chỉ giao hàng mặc định")]
        [Column("is_default_shipping")]
        public bool IsDefaultShipping { get; set; } = false;

        [NotMapped]
        [StringLength(100)]
        [Display(Name = "Quốc gia")]
        public string? Country { get; set; }

        [NotMapped]
        [StringLength(20)]
        [Display(Name = "Mã bưu chính")]
        public string? ZipCode { get; set; }

        [NotMapped]
        public int? DistrictId { get; set; }

        [NotMapped]
        [StringLength(50)]
        public string? WardCode { get; set; }

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
