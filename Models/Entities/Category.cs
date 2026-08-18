using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Danh mục sách — hỗ trợ cây phân cấp qua parent_id.
    /// Mapped to [dbo].[Categories] from script.sql.
    /// </summary>
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        [Column("category_name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Ảnh danh mục")]
        [Column("category_image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Mô tả")]
        [Column("description")]
        public string? Description { get; set; }

        // Self-referencing FK cho danh mục cha
        [Display(Name = "Danh mục cha")]
        [Column("parent_id")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category? Parent { get; set; }

        // Navigation Properties
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
