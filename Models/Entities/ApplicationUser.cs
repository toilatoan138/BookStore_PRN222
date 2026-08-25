using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Người dùng hệ thống — extends IdentityUser (đã có sẵn Email, UserName, PhoneNumber, PasswordHash).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public bool Status { get; set; } = true; // true=Active, false=Banned

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Tổng chi tiêu")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpend { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "F-Points")]
        [Range(0, int.MaxValue)]
        public int FPoints { get; set; } = 0;

        [Display(Name = "Số dư ví")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 0;

        // --- Navigation Properties ---
        public int? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual Branch? Branch { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
        public virtual ICollection<FPointHistory> FPointHistories { get; set; } = new List<FPointHistory>();
        public virtual ICollection<CustomerNote> CustomerNotes { get; set; } = new List<CustomerNote>();

        // --- Business Logic ---
        [NotMapped]
        public string RankName
        {
            get
            {
                if (FPoints >= 5000) return "Kim Cương";
                if (FPoints >= 2000) return "Vàng";
                if (FPoints >= 500) return "Bạc";
                return "Đồng";
            }
        }

        [NotMapped]
        public decimal DiscountRate
        {
            get
            {
                return RankName switch
                {
                    "Kim Cương" => 0.15m,
                    "Vàng" => 0.10m,
                    "Bạc" => 0.05m,
                    _ => 0.0m
                };
            }
        }
    }
}
