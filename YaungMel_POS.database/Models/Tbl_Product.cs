using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YaungMel_POS.database.Models;

public class Tbl_Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool DeleteFlag { get; set; } = false;

    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Tbl_Category Category { get; set; } = null!;

    public ICollection<Tbl_SaleItem> SaleItems { get; set; } = new List<Tbl_SaleItem>();

    [ForeignKey("CreatedBy")]
    public Tbl_User CreatedUser { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    public Tbl_User? UpdatedUser { get; set; }
}
