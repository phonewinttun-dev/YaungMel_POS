using Microsoft.EntityFrameworkCore;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Shared.Responses;
using YaungMel_POS.Domain.Features.Audit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YaungMel_POS.Domain.Features.Sale;

public class SaleService : ISaleService
{
    private readonly POSDbContext _db;
    private readonly IAuditService _auditService;
    private readonly JsonSerializerOptions _jsonOptions = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

    public SaleService(POSDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    private IQueryable<Tbl_Product> ActiveProduct => _db.Products.Where(p => !p.DeleteFlag);

    #region Create Sale
    public async Task<Result<SaleDTO>> CreateSaleAsync(CreateSaleDTO reqSale, int userId)
    {
        if (!ValidateSale(reqSale))
            return Result<SaleDTO>.SystemError("Invalid sale data.");

        if (reqSale.Items == null || !reqSale.Items.Any())
            return Result<SaleDTO>.SystemError("Sale must contain at least one item.");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var productIds = reqSale.Items.Select(x => x.ProductId).Distinct().ToList();

            var products = await _db.Products
                                   .Where(p => productIds.Contains(p.Id) && !p.DeleteFlag)
                                   .ToDictionaryAsync(p => p.Id);

            // check product exists & sufficient quantity
            foreach (var item in reqSale.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                    return Result<SaleDTO>.SystemError($"Product with ID: {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    return Result<SaleDTO>.SystemError($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
            }

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
                product.UpdatedAt = DateTime.UtcNow;

                return new Tbl_SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
            }).ToList();

            _db.Sales.Add(saveModel);
            await _db.SaveChangesAsync();

            await _auditService.LogCreateAsync(new
            {
                saveModel.Id,
                saveModel.TotalPrice,
                saveModel.VoucherCode,
                saveModel.CreatedAt,
                saveModel.CreatedBy,
                SaleItems = saveModel.SaleItems.Select(si => new
                {
                    si.Id,
                    si.ProductId,
                    si.Quantity,
                    si.Price
                })
            }, userId, "Sale");

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

            return Result<SaleDTO>.Success(resModel);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<SaleDTO>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Get All Sale Paignation
    public async Task<Result<SaleListResponseDTO>> GetSalesAsync(int pageNo, int pageSize)
    {
        try
        {
            if (pageSize <= 0) return Result<SaleListResponseDTO>.SystemError("Page size must be greater than 0.");

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

            return Result<SaleListResponseDTO>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SaleListResponseDTO>.SystemError($"Error: {ex.Message}");
        }
    }
    #endregion

    #region Get Sale By Voucher code
    public async Task<Result<SaleDTO>> GetSaleByVoucherCodeAsync(string voucherCode)
    {
        try
        {
            var sale = await _db.Sales
               .Include(s => s.SaleItems)
               .ThenInclude(si => si.Product) // Fix: Include product to get names efficiently
               .AsNoTracking()
               .FirstOrDefaultAsync(s => s.VoucherCode == voucherCode);

            if (sale is null)
                return Result<SaleDTO>.NotFound("Sale not found.");

            var resModel = new SaleDTO
            {
                Id = sale.Id,
                TotalPrice = sale.TotalPrice,
                TotalPriceFormatted = sale.TotalPrice.ToString("N0"),
                VoucherCode = sale.VoucherCode,
                SaleItems = sale.SaleItems.Select(x => new SaleItemDTO
                {
                    ProductName = x.Product?.Name ?? "Unknown Product",
                    Quantity = x.Quantity,
                    Price = x.Price,
                    PriceFormatted = x.Price.ToString("N0")
                }).ToList()
            };
            return Result<SaleDTO>.Success(resModel);
        }
        catch (Exception ex)
        {
            return Result<SaleDTO>.SystemError(ex.Message);
        }
    }
    #endregion

    #region Sale Validation
    public bool ValidateSale(CreateSaleDTO sale)
    {
        if (sale == null)
            return false;

        if (sale.Items == null || !sale.Items.Any())
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
