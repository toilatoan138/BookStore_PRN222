using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Bộ sưu tập sách của người dùng — Mapped to [dbo].[Collections] from script.sql.
    /// </summary>
    [Table("Collections")]
    public class Collection
    {
        [Key]
        [Column("collection_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Tên bộ sưu tập là bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Tên bộ sưu tập")]
        [Column("collection_name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        [Column("description")]
        public string? Description { get; set; }

        [Display(Name = "Công khai")]
        [Column("is_public")]
        public bool IsPublic { get; set; } = false;

        [StringLength(20)]
        [Display(Name = "Màu bìa")]
        [Column("cover_color")]
        public string? CoverColor { get; set; }

        [Display(Name = "Ngày tạo")]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation (N-N qua CollectionBook)
        public virtual ICollection<CollectionBook> CollectionBooks { get; set; } = new List<CollectionBook>();

        // Computed property — không lưu DB
        [NotMapped]
        public int TotalBooks => CollectionBooks?.Count ?? 0;
    }
}
