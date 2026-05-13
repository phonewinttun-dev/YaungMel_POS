namespace YaungMel_POS.Domain.DTOs;

public class SaleDTO
{
    public long Id { get; set; }

    public decimal TotalPrice { get; set; }

    public string TotalPriceFormatted { get; set; } = string.Empty;

    public string VoucherCode { get; set; } = null!;

    public List<SaleItemDTO> SaleItems { get; set; } = new List<SaleItemDTO>();
}
public class SaleItemDTO
{
    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string PriceFormatted { get; set; } = string.Empty;

}

public class CreateSaleDTO
{
    public List<CreateSaleItemDTO> Items { get; set; } = null!;
}

public class CreateSaleItemDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

