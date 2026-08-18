using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Bảng nối N-N: Collection ↔ Book — Mapped to [dbo].[Collection_Books] from script.sql.
    /// </summary>
    [Table("Collection_Books")]
    public class CollectionBook
    {
        [Required]
        [Column("collection_id")]
        public int CollectionId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Column("added_at")]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // FK
        [ForeignKey("CollectionId")]
        public virtual Collection Collection { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
