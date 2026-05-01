using System.ComponentModel.DataAnnotations;

namespace YaungMel_POS.domain.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}

public class UpdateCategoryDTO
{
    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }
    [MaxLength(250)]
    public string? Description { get; set; }
}

public class CategoryListResponseModel
{
    public List<CategoryDTO> Items { get; set; } = null!;
    public PageSettingDTO PageSetting { get; set; } = null!;

} 