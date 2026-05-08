using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared.Responses;

namespace YaungMel_POS.Domain.Features.Summary
{
    public interface ISummaryService
    {
        Task<Result<SummaryDTO>> CreateSummaryAsync();
        Task<Result<SummaryDetailDto>> GetSummaryByDateAsync(DateTime date);
        Task<Result<SummaryListResponseModel>> GetSummaryByPagination(int pageNo = 1, int pageSize = 10);
    }
}