using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Summary
{
    public interface ISummaryService
    {
        Task<PagedResult<SummaryDTO>> CreateSummaryAsync();
        Task<PagedResult<SummaryDetailDto>> GetSummaryByDateAsync(DateTime date);
        Task<PagedResult<SummaryListResponseModel>> GetSummaryByPagination(int pageNo = 1, int pageSize = 10);
        Task<PagedResult<List<SummaryDTO>>> GetSummaryByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}