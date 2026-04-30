using POSSampleOWN.domain.DTOs;
using POSSampleOWN.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSampleOWN.domain.Features.Search
{
    public interface ISearchService
    {
        Task<List<ProductDTO>> SearchProductsAsync(SearchProductRequestDTO searchRequest);
        
        Task<List<CategoryDTO>> SearchCategoryAsync(SearchCategoryRequestDTO searchRequest);
    }
}
