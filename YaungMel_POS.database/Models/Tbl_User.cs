using System.ComponentModel.DataAnnotations;

namespace YaungMel_POS.database.Models;

public class Tbl_User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MobileNum { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    public enum UserRole
    {
        Admin,
        Staff,
        Customer
    }

    public UserRole Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }

    public bool DeleteFlag { get; set; } = false;

    public ICollection<Tbl_Product>? Products { get; set; } = new List<Tbl_Product>();

    public ICollection<Tbl_Category> Categories { get; set; } = new List<Tbl_Category>();

    public ICollection<Tbl_Sale> Sales { get; set; } = new List<Tbl_Sale>();
    
    public ICollection<Tbl_User_Token> UserToken { get; set; } = new List<Tbl_User_Token>();
}
