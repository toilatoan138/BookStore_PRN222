using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    [Table("Branches")]
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<BranchInventory> Inventories { get; set; } = new List<BranchInventory>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
