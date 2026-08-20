using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models.Entities
{
    /// <summary>
    /// Vị trí kho lưu trữ — Mapped to [dbo].[Warehouse_Locations] from script.sql.
    /// </summary>
    [Table("Warehouse_Locations")]
    public class Location
    {
        [Key]
        [Column("location_id")]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Khu vực")]
        [Column("zone", TypeName = "varchar(10)")]
        public string Zone { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Kệ")]
        [Column("rack", TypeName = "varchar(10)")]
        public string Rack { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Tầng")]
        [Column("shelf", TypeName = "varchar(10)")]
        public string Shelf { get; set; } = string.Empty;

        [Display(Name = "Mã vị trí")]
        [Column("location_code", TypeName = "varchar(32)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string LocationCode { get; set; } = string.Empty; // Computed persisted NOT NULL: zone-rack-shelf (xem HasComputedColumnSql trong ApplicationDbContext)

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Display(Name = "Mô tả")]
        [Column("description")]
        public string? Description { get; set; }

        [Display(Name = "Sức chứa tối đa")]
        [Column("max_capacity")]
        public int? MaxCapacity { get; set; }

        // FK
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Navigation
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
