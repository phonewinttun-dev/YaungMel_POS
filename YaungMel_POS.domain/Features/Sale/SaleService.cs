using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using YaungMel_POS.domain.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.database.Data;
using YaungMel_POS.database.Models;
using YaungMel_POS.shared.Responses;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace YaungMel_POS.domain.Features.Sale;

public class SaleService : ISaleService
{
    private readonly POSDbContext _db;

    public SaleService(POSDbContext db)
    {
        _db = db;
    }

    private IQueryable<Tbl_Product> ActiveProduct => _db.Products.Where(p => !p.DeleteFlag);

    #region Create Sale
    public async Task<ApiResponse<SaleDTO>> CreateSaleAsync(CreateSaleDTO reqSale, int userId)
    {
        if (!ValidateSale(reqSale))
            return ApiResponse<SaleDTO>.Fail("Invalid sale data.");

        var productIds = reqSale.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await ActiveProduct
                               .Where(p => productIds.Contains(p.Id))
                               .ToDictionaryAsync(p => p.Id);

        // check product exists & sufficient quantity
        foreach (var item in reqSale.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
                return ApiResponse<SaleDTO>.Fail($"Product with ID: {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                return ApiResponse<SaleDTO>.Fail($"Insufficient stock for {product.Name}.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            
            decimal totalPrice = TotalPrice(reqSale, products);
            var saveModel = new Tbl_Sale
            {
                TotalPrice = totalPrice,
                VoucherCode = "YM-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };

            saveModel.SaleItems = reqSale.Items.Select(item =>
            {
                var product = products[item.ProductId];

                // Stock deduction
                product.StockQuantity -= item.Quantity;

                return new Tbl_SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
            }).ToList();

            _db.Sales.Add(saveModel);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            var resModel = new SaleDTO
            {
                Id = saveModel.Id,
                TotalPrice = saveModel.TotalPrice,
                TotalPriceFormatted = saveModel.TotalPrice.ToString("N0"),
                VoucherCode = saveModel.VoucherCode,
                SaleItems = saveModel.SaleItems.Select(x => new SaleItemDTO
                {
                    // Safely get the name from the dictionary we built earlier
                    ProductName = products[x.ProductId].Name,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    PriceFormatted = x.Price.ToString("N0")
                }).ToList()
            };

            return ApiResponse<SaleDTO>.Success(resModel);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<SaleDTO>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Get All Sales
    public async Task<ApiResponse<List<SaleDTO>>> GetAllSalesAsync()
    {
        try
        {
            var resModel = await _db.Sales
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .Select(sale => new SaleDTO
            {
                Id = sale.Id,
                TotalPrice = sale.TotalPrice,
                TotalPriceFormatted = sale.TotalPrice.ToString("N0"),
                VoucherCode = sale.VoucherCode,
                SaleItems = sale.SaleItems.Select(item => new SaleItemDTO
                {
                    ProductName = item.Product.Name ?? "Unknown Product",
                    Quantity = item.Quantity,
                    Price = item.Price,
                    PriceFormatted = item.Price.ToString("N0")
                }).ToList()
            })
            .ToListAsync();
            return ApiResponse<List<SaleDTO>>.Success(resModel);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SaleDTO>>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Get All Sale Paignation 
    public async Task<ApiResponse<SaleListResponseDTO>> GetSalesAsync(int pageNo, int pageSize)
    {
        try
        {
            // double time hitting to database 
            var totalItems = await _db.Sales.CountAsync();

            var pageCount = totalItems / pageSize;
            if(totalItems % pageSize > 0) pageCount++;

            var sales = await _db.Sales
                .AsNoTracking()
                .OrderByDescending(s => s.Id)
                .Skip((pageNo - 1) * pageSize) 
                .Take(pageSize)              
                .Select(sale => new SaleDTO
                {
                    Id = sale.Id,
                    TotalPrice = sale.TotalPrice,
                    TotalPriceFormatted = sale.TotalPrice.ToString("N0"),
                    VoucherCode = sale.VoucherCode,
                    SaleItems = sale.SaleItems.Select(item => new SaleItemDTO
                    {
                        ProductName = item.Product.Name ?? "Unknown Product",
                        Quantity = item.Quantity,
                        Price = item.Price,
                        PriceFormatted = item.Price.ToString("N0")
                    }).ToList()
                })
                .ToListAsync();

            var result = new SaleListResponseDTO
            {
                Items = sales,
                PageSetting = new PageSettingDTO(pageNo, pageSize, pageCount)
            };

            return ApiResponse<SaleListResponseDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<SaleListResponseDTO>.Fail($"Error: {ex.Message}");
        }
    }
    #endregion
    #region Get Sale By Voucher code
    public async Task<ApiResponse<SaleDTO>> GetSaleByVoucherCodeAsync(string  voucherCode)
    {
        var sale = await _db.Sales
            .Include(s => s.SaleItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.VoucherCode == voucherCode);

        if (sale is null)
            return ApiResponse<SaleDTO>.Fail("Sale not found.");

        var product = await ActiveProduct.ToDictionaryAsync(p => p.Id, p => p.Name);

        var resModel = new SaleDTO
        {
            Id = sale.Id,
            TotalPrice = sale.TotalPrice,
            TotalPriceFormatted = sale.TotalPrice.ToString("N0"),
            VoucherCode = sale.VoucherCode,
            SaleItems = sale.SaleItems.Select(x => new SaleItemDTO
            {
                ProductName = product.TryGetValue(x.ProductId, out var name) ? name : "Unknown Product",
                Quantity = x.Quantity,
                Price = x.Price,
                PriceFormatted = x.Price.ToString("N0")
            }).ToList()
        };
        return ApiResponse<SaleDTO>.Success(resModel);
    }
    #endregion

    #region Sale Validation
    public bool ValidateSale(CreateSaleDTO sale)
    {
        if (sale == null)
            return false;
        foreach (var item in sale.Items)
        {
            if (item == null)
                return false;
            if (item.Quantity <= 0 )
                return false;
        }
        return true;
    }
    #endregion

    #region total price
    public decimal TotalPrice(CreateSaleDTO reqSale, Dictionary<int, Tbl_Product> products)
    {
        decimal totalPrice = 0;
        foreach (var item in reqSale.Items)
        {
            // Get the price from our pre-loaded dictionary
            if (products.TryGetValue(item.ProductId, out var product))
            {
                totalPrice += SubPrice(product.Price, item.Quantity);
            }
        }
        return totalPrice;
    }
    #endregion

    #region sub price
    public decimal SubPrice(decimal price, int quantity)
    {
        return price * quantity;
    }
    #endregion

}

