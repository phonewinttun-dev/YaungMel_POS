using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.database.Models;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Sale
{
    public interface ISaleService
    {
        Task<ApiResponse<SaleDTO>> CreateSaleAsync(CreateSaleDTO reqSale, int userId);
        Task<ApiResponse<List<SaleDTO>>> GetAllSalesAsync();
        Task<ApiResponse<SaleListResponseDTO>> GetSalesAsync(int pageNo, int pageSize);

        Task<ApiResponse<SaleDTO>> GetSaleByVoucherCodeAsync(string voucherCode);
        bool ValidateSale(CreateSaleDTO sale);
        decimal TotalPrice(CreateSaleDTO reqSale, Dictionary<int, Tbl_Product> products);
        decimal SubPrice(decimal price, int quantity);
    }
}