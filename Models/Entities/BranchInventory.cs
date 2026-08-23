using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    [Table("Branch_Inventory")]
    public class BranchInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        // Navigation properties
        [ForeignKey("BranchId")]
        public Branch Branch { get; set; } = null!;

        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;
    }
}
