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
                body {
                    font-family: Arial, sans-serif;
                    background-color: #ffffff;
                    color: #000000;
                    margin: 0;
                    padding: 20px;
                    font-size: 10pt;
                }
                .header-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-bottom: 20px;
                }
                .company-name {
                    font-size: 12pt;
                    font-weight: bold;
                    text-transform: uppercase;
                }
                .report-title {
                    text-align: center;
                    font-size: 16pt;
                    font-weight: bold;
                    margin: 20px 0;
                    border-top: 1px solid #000;
                    border-bottom: 1px solid #000;
                    padding: 10px 0;
                }
                .meta-info {
                    text-align: right;
                    font-size: 9pt;
                }
                .data-table {
                    width: 100%;
                    border-collapse: collapse;
                    table-layout: fixed;
                    margin-top: 0;
                }
                .data-table thead {
                    display: table-header-group;
                }
                .voucher-container {
                    page-break-inside: avoid !important;
                    display: block;
                    width: 100%;
                }
                .data-table th, .data-table td {
                    padding: 5px;
                    font-size: 9pt;
                    vertical-align: top;
                    word-wrap: break-word;
                }
                .col-doc { width: 120px; }
                .col-desc { width: auto; }
                .col-qty { width: 40px; text-align: center; }
                .col-price { width: 90px; text-align: right; }
                .col-total { width: 90px; text-align: right; }
                
                .data-table th {
                    border-top: 1px solid #000;
                    border-bottom: 1px solid #000;
                    text-align: left;
                    font-weight: bold;
                }
                .data-table td {
                    padding: 5px;
                    font-size: 9pt;
                    vertical-align: top;
                }
                .text-right { text-align: right; }
                .text-center { text-align: center; }
                .bold { font-weight: bold; }
                .border-bottom { border-bottom: 1px solid #eee; }
                .footer {
                    margin-top: 30px;
                    font-size: 8pt;
                    border-top: 1px solid #ccc;
                    padding-top: 5px;
                    color: #666;
                }
                .total-row td {
                    border-top: 1px solid #000;
                    font-weight: bold;
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
                    <td class='meta-info'>Date : {DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>");

            sb.Append("<div class='report-title'>YaungMel POS Daily Summary Report</div>");

            sb.Append($@"
            <div style='margin-bottom: 15px;'>
                <span class='bold'>Report Date:</span> {detail.Summary.Date:dd/MM/yyyy}
            </div>");

            // Table Header (Standalone Table)
            sb.Append(@"
            <table class='data-table'>
                <thead>
                    <tr>
                        <th class='col-doc'>Voucher Code</th>
                        <th class='col-desc'>Item Description</th>
                        <th class='col-qty'>Qty</th>
                        <th class='col-price'>Unit Price</th>
                        <th class='col-total'>Total</th>
                    </tr>
                </thead>
            </table>");

            foreach (var sale in detail.Sales)
            {
                sb.Append("<div class='voucher-container'>");
                sb.Append("<table class='data-table'><tbody>");
                bool firstItem = true;
                foreach (var item in sale.SaleItems)
                {
                    sb.Append("<tr>");
                    if (firstItem)
                    {
                        sb.Append($"<td class='col-doc'>{sale.VoucherCode}</td>");
                        firstItem = false;
                    }
                    else
                    {
                        sb.Append("<td class='col-doc'></td>");
                    }

                    sb.Append($"<td class='col-desc'>{item.ProductName}</td>");
                    sb.Append($"<td class='col-qty'>{item.Quantity}</td>");
                    sb.Append($"<td class='col-price'>{item.Price:N0}</td>");
                    sb.Append($"<td class='col-total'>{(item.Price * item.Quantity):N0}</td>");
                    sb.Append("</tr>");
                }

                // Subtotal for the voucher
                sb.Append($@"
                <tr>
                    <td colspan='4' class='text-right col-desc' style='font-size: 8pt; color: #666; font-style: italic; width: calc(100% - 90px);'>Voucher Subtotal:</td>
                    <td class='col-total bold' style='font-size: 8pt; border-bottom: 1px solid #000;'>{sale.TotalPrice:N0}</td>
                </tr>");
                sb.Append("</tbody></table>");
                sb.Append("</div>");
            }

            // Grand Total at the end
            sb.Append($@"
            <table class='data-table'>
                <tbody>
                <tr class='total-row'>
                    <td colspan='4' class='text-right' style='width: calc(100% - 90px);'>GRAND TOTAL (MMK)</td>
                    <td class='text-right col-total'>{detail.Summary.TotalAmount:N0}</td>
                </tr>
                </tbody>
            </table>");

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
                    <td class='meta-info'>Date : {DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>");

            sb.Append("<div class='report-title'>YaungMel POS Date Range Summary Report</div>");

            sb.Append($@"
            <div style='margin-bottom: 15px;'>
                <span class='bold'>Report Period:</span> {start:dd/MM/yyyy} to {end:dd/MM/yyyy}
            </div>");

            // Table Header
            sb.Append(@"
            <table class='data-table'>
                <thead>
                    <tr>
                        <th style='width: 100px;'>Date</th>
                        <th class='col-desc'>Top Selling Product</th>
                        <th class='col-qty'>Sales</th>
                        <th class='col-total'>Total Amount</th>
                    </tr>
                </thead>
                <tbody>");

            decimal grandTotal = 0;
            int totalSalesCount = 0;

            foreach (var summary in summaries.OrderBy(s => s.Date))
            {
                sb.Append("<tr>");
                sb.Append($"<td>{summary.Date:dd/MM/yyyy}</td>");
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
                    <td class='col-total'>{grandTotal:N0}</td>
                </tr>");

            sb.Append("</tbody></table>");

            sb.Append($@"
            <div style='margin-top: 20px; font-size: 9pt;'>
                <p><span class='bold'>Total Days Processed:</span> {summaries.Count}</p>
                <p><span class='bold'>Average Daily Amount:</span> {(summaries.Count > 0 ? (grandTotal / summaries.Count).ToString("N0") : "0")} MMK</p>
            </div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}