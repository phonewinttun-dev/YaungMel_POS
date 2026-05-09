using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Domain.Features.Report
{
    public interface IReportService
    {
        Task<byte[]> GenerateDetailedDailyPdfAsync(DateTime date);
        Task<byte[]> GenerateDetailedRangePdfAsync(DateTime startDate, DateTime endDate);
    }
}
