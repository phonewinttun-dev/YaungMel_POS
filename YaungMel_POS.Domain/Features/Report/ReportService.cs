using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Domain.Features.Summary;

namespace YaungMel_POS.Domain.Features.Report
{
    public class ReportService : IReportService
    {
        private readonly ISummaryService _summaryService;
        private readonly IConverter _pdfConverter;

        public ReportService(ISummaryService summaryService, IConverter pdfConverter)
        {
            _summaryService = summaryService;
            _pdfConverter = pdfConverter;
        }

        public async Task<byte[]> GenerateDetailedDailyPdfAsync(DateTime date)
        {
            var result = await _summaryService.GetSummaryByDateAsync(date);
            if (!result.IsSuccess) return Array.Empty<byte>();

            var html = GenerateDailyHtml(result.Data!);
            return ConvertHtmlToPdf(html, $"Daily Report - {date:yyyy-MM-dd}");
        }

        private byte[] ConvertHtmlToPdf(string html, string title)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    DocumentTitle = title,
                    Margins = { Top = 15, Bottom = 15, Left = 10, Right = 10 }
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 5 }
                    }
                }
            };

            return _pdfConverter.Convert(doc);
        }

        private string GetCommonStyles()
        {
            return @"
            <style>
                @font-face { 
                    font-family: 'Arial'; 
                    src: local('Arial'); 
                }
                body {
                    font-family: 'Arial', sans-serif;
                    background-color: #ffffff;
                    color: #000000;
                    margin: 0;
                    padding: 20px;
                    font-size: 9pt;
                    line-height: 1.4;
                }
                .header-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 10px;
                }
                .company-name {
                    font-size: 11pt;
                    font-weight: bold;
                    text-transform: uppercase;
                    letter-spacing: 1px;
                }
                .report-title {
                    text-align: center;
                    font-size: 14pt;
                    font-weight: bold;
                    margin: 15px 0;
                    border-top: 2px solid #000;
                    border-bottom: 2px solid #000;
                    padding: 8px 0;
                    text-transform: uppercase;
                }
                .meta-info {
                    text-align: right;
                    font-size: 8pt;
                }
                .data-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 5px;
                }
                .data-table th {
                    border: 1px solid #444;
                    padding: 6px 4px;
                    font-size: 9pt;
                    font-weight: bold;
                    text-transform: uppercase;
                    background-color: #f9f9f9;
                }
                .data-table td {
                    padding: 5px 4px;
                    font-size: 9pt;
                    vertical-align: top;
                    border: 1px solid #eee;
                }
                .voucher-section {
                    margin-bottom: 25px;
                    page-break-inside: avoid;
                }
                .voucher-header {
                    font-weight: bold;
                    background-color: #333;
                    color: white;
                    padding: 6px 10px;
                    font-size: 9pt;
                    margin-top: 15px;
                    border-radius: 4px 4px 0 0;
                }
                .col-doc { width: 120px; }
                .col-date { width: 100px; text-align: center; }
                .col-desc { width: auto; text-align: left; }
                .col-qty { width: 60px; text-align: center; }
                .col-price { width: 100px; text-align: right; }
                .col-total { width: 110px; text-align: right; }
                
                .text-right { text-align: right; }
                .text-center { text-align: center; }
                .bold { font-weight: bold; }
                
                .total-row td {
                    border: 1px solid #444;
                    font-weight: bold;
                    font-size: 10pt;
                    padding: 8px 4px;
                    background-color: #f0f0f0;
                }
                .summary-box {
                    margin-top: 30px;
                    padding: 15px;
                    border: 2px solid #333;
                    width: 320px;
                    float: right;
                    background-color: #fafafa;
                }
                .summary-item {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 5px;
                }
                .clearfix::after {
                    content: '';
                    clear: both;
                    display: table;
                }
            </style>";
        }
        private string GenerateDailyHtml(SummaryDetailDto detail)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append(GetCommonStyles());
            sb.Append("</head><body>");

            // Header info
            sb.Append($@"
            <table class='header-table'>
                <tr>
                    <td class='company-name'>YAUNG MEL POS SOLUTIONS</td>
                    <td class='meta-info'>PRINTED : {DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>");

            sb.Append("<div class='report-title'>Daily Sales Summary Report</div>");

            sb.Append($@"
            <div style='margin-bottom: 20px; font-size: 10pt;'>
                <span class='bold'>REPORT DATE:</span> {detail.Summary.Date:dd MMMM yyyy}
            </div>");

            foreach (var sale in detail.Sales)
            {
                sb.Append("<div class='voucher-section'>");
                sb.Append($"<div class='voucher-header'>VOUCHER: {sale.VoucherCode}</div>");
                sb.Append("<table class='data-table'>");
                sb.Append(@"
                <thead>
                    <tr>
                        <th class='col-desc'>Item Description</th>
                        <th class='col-qty'>Qty</th>
                        <th class='col-price'>Price</th>
                        <th class='col-total'>Total</th>
                    </tr>
                </thead>
                <tbody>");

                foreach (var item in sale.SaleItems)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td class='col-desc'>{item.ProductName}</td>");
                    sb.Append($"<td class='col-qty'>{item.Quantity}</td>");
                    sb.Append($"<td class='col-price'>{item.Price:N0}</td>");
                    sb.Append($"<td class='col-total'>{(item.Price * item.Quantity):N0}</td>");
                    sb.Append("</tr>");
                }

                // Subtotal for the voucher
                sb.Append($@"
                <tr>
                    <td colspan='3' class='text-right bold'>Voucher Total:</td>
                    <td class='col-total bold'>{sale.TotalPrice:N0}</td>
                </tr>");
                sb.Append("</tbody></table>");
                sb.Append("</div>");
            }

            // Grand Total at the end
            sb.Append($@"
            <div class='clearfix'>
                <div class='summary-box'>
                    <div class='summary-item'>
                        <span class='bold'>TOTAL VOUCHERS:</span>
                        <span>{detail.Sales.Count}</span>
                    </div>
                    <div class='summary-item' style='margin-top: 10px; border-top: 1px solid #000; padding-top: 5px;'>
                        <span class='bold' style='font-size: 11pt;'>GRAND TOTAL:</span>
                        <span class='bold' style='font-size: 11pt;'>{detail.Summary.TotalAmount:N0} MMK</span>
                    </div>
                </div>
            </div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }

        public async Task<byte[]> GenerateDetailedRangePdfAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _summaryService.GetSummaryByDateRangeAsync(startDate, endDate);
            if (!result.IsSuccess) return Array.Empty<byte>();

            var html = GenerateRangeHtml(result.Data!, startDate, endDate);
            return ConvertHtmlToPdf(html, $"Range Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        }

        private string GenerateRangeHtml(List<SummaryDTO> summaries, DateTime start, DateTime end)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append(GetCommonStyles());
            sb.Append("</head><body>");

            // Header info
            sb.Append($@"
            <table class='header-table'>
                <tr>
                    <td class='company-name'>YAUNG MEL POS SOLUTIONS</td>
                    <td class='meta-info'>PRINTED : {DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>");

            sb.Append("<div class='report-title'>Periodic Sales Summary Report</div>");

            sb.Append($@"
            <div style='margin-bottom: 20px; font-size: 10pt;'>
                <span class='bold'>REPORT PERIOD:</span> {start:dd/MM/yyyy} to {end:dd/MM/yyyy}
            </div>");

            // Table Header
            sb.Append(@"
            <table class='data-table'>
                <thead>
                    <tr>
                        <th class='col-date'>DATE</th>
                        <th class='col-desc'>TOP SELLING PRODUCT</th>
                        <th class='col-qty'>SALES</th>
                        <th class='col-total'>TOTAL AMOUNT</th>
                    </tr>
                </thead>
                <tbody>");

            decimal grandTotal = 0;
            int totalSalesCount = 0;

            foreach (var summary in summaries.OrderBy(s => s.Date))
            {
                sb.Append("<tr>");
                sb.Append($"<td class='col-date'>{summary.Date:dd/MM/yyyy}</td>");
                sb.Append($"<td class='col-desc'>{summary.TopSaleProductName ?? "N/A"}</td>");
                sb.Append($"<td class='col-qty'>{summary.TotalSale}</td>");
                sb.Append($"<td class='col-total'>{summary.TotalAmount:N0}</td>");
                sb.Append("</tr>");

                grandTotal += summary.TotalAmount;
                totalSalesCount += summary.TotalSale;
            }

            // Grand Total Row
            sb.Append($@"
                <tr class='total-row'>
                    <td colspan='2' class='text-right'>TOTAL</td>
                    <td class='col-qty'>{totalSalesCount}</td>
                    <td class='col-total'>{grandTotal:N0} MMK</td>
                </tr>");

            sb.Append("</tbody></table>");

            sb.Append($@"
            <div class='clearfix'>
                <div class='summary-box'>
                    <div class='summary-item'>
                        <span class='bold'>DAYS PROCESSED:</span>
                        <span>{summaries.Count}</span>
                    </div>
                    <div class='summary-item'>
                        <span class='bold'>AVG DAILY SALES:</span>
                        <span>{(summaries.Count > 0 ? (grandTotal / summaries.Count).ToString("N0") : "0")} MMK</span>
                    </div>
                </div>
            </div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}