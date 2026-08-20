using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Ảnh chi tiết sách — Mapped to [dbo].[BookImages] from script.sql.
    /// </summary>
    [Table("BookImages")]
    public class BookImage
    {
        [Key]
        [Column("image_id")]
        public int Id { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "URL ảnh")]
        [Column("image_url", TypeName = "varchar(255)")]
        public string ImageUrl { get; set; } = string.Empty;

        // FK
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
