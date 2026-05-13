using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Sale
{
    public interface ISaleService
    {
        Task<Result<SaleDTO>> CreateSaleAsync(CreateSaleDTO reqSale, int userId);
        Task<PagedResult<SaleDTO>> GetSalesAsync(PaginationRequest request);
        Task<Result<SaleDTO>> GetSaleByVoucherCodeAsync(string voucherCode);
        bool ValidateSale(CreateSaleDTO sale);
        decimal TotalPrice(CreateSaleDTO reqSale, Dictionary<int, Tbl_Product> products);
        decimal SubPrice(decimal price, int quantity);
    }
}