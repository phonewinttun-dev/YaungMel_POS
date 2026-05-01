using System.ComponentModel.DataAnnotations;

namespace YaungMel_POS.domain.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public bool DeleteFlag { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductDTO
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public int CategoryId { get; set; }
    // public int CreatedBy { get; set; }
}

public class UpdateProductDTO
{

    [MaxLength(150)]
    public string? Name { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public int? CategoryId { get; set; }

    // public int UpdatedBy { get; set; }
}
public class ProductListResponseDTO
{
    public List<ProductDTO> Items { get; set; } = null!;
    public PageSettingDTO PageSetting { get; set; } = null!;
}
