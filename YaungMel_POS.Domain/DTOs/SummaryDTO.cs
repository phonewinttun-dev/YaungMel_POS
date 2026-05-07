using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Domain.DTOs;

public class SummaryDTO
{
    public DateTime Date { get; set; }
    public int TotalSale { get; set; }
    public decimal TotalAmount { get; set; }
    public string? TopSaleProductName { get; set; }
}

public class SummaryListResponseModel
{
    public List<SummaryDTO> Items { get; set; } = null!;
    public PageSettingDTO PageSetting { get; set; } = null!;
}

public class SummaryDetailDto
{
    public SummaryDTO Summary { get; set; } = new();
    public List<SaleDTO> Sales { get; set; } = new();
}

