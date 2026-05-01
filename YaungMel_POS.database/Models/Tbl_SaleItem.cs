namespace YaungMel_POS.database.Models;

public class Tbl_SaleItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int SaleId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public Tbl_Product Product { get; set; } = null!;

    public Tbl_Sale Sale { get; set; } = null!;
}
