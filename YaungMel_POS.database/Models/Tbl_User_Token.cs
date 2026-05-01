namespace YaungMel_POS.database.Models;

public class Tbl_User_Token
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tbl_User User { get; set; } = null!;
}
