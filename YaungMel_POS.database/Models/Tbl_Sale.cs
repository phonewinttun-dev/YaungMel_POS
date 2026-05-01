using System.ComponentModel.DataAnnotations.Schema;

namespace YaungMel_POS.database.Models;

public class Tbl_Sale
{
    public int Id { get; set; }

    public decimal TotalPrice { get; set; }

    public string VoucherCode { get; set; } = string.Empty;

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<Tbl_SaleItem> SaleItems { get; set; } = new List<Tbl_SaleItem>();

    [ForeignKey("CreatedBy")]
    public Tbl_User CreatedUser { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    public Tbl_User? UpdatedUser { get; set; }
}
