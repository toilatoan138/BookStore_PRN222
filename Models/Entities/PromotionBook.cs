using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Bảng nối N-N: Promotion ↔ Book — Mapped to [dbo].[Promotion_Books] from script.sql.
    /// </summary>
    [Table("Promotion_Books")]
    public class PromotionBook
    {
        [Required]
        [Column("promo_id")]
        public int PromoId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        // FK
        [ForeignKey("PromoId")]
        public virtual Promotion Promotion { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
