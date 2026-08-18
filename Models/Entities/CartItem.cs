using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Mục trong giỏ hàng — Mapped to [dbo].[CartItems] from script.sql.
    /// </summary>
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        [Column("cart_item_id")]
        public int Id { get; set; }

        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải >= 1")]
        [Display(Name = "Số lượng")]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [Column("add_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("BookId")]
        public virtual Book Book { get; set; } = null!;
    }
}
