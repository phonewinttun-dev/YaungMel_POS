using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YaungMel_POS.database.Models;

public class Tbl_Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool DeleteFlag { get; set; } = false;

    public ICollection<Tbl_Product>? Products { get; set; }
    
    [ForeignKey("CreatedBy")]
    public Tbl_User CreatedUser { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    public Tbl_User? UpdatedUser { get; set; }
}
