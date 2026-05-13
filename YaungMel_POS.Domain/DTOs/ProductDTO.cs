using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string PriceFormatted { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public bool DeleteFlag { get; set; }
    public bool IsActive { get; set; }
    public uint Version { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageId { get; set; }
}

public class CreateProductDTO
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageId { get; set; }
    public string? ImagePublicId
    {
        get => ImageId;
        set => ImageId = value;
    }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public int CategoryId { get; set; }

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

    /// <summary>
    /// Must be supplied by the client (from a previous GET/list response) to detect concurrent update conflicts.
    /// </summary>
    [Required]
    public uint Version { get; set; }
}
//public class ProductListResponseDTO
//{
//    public List<ProductDTO> Items { get; set; } = null!;
//    public PaginationRequest PageSetting { get; set; } = null!;
//}
