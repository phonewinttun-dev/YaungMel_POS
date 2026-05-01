using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.domain.DTOs;

namespace YaungMel_POS.domain.Features.Search
{
    public interface ISearchService
    {
        Task<List<ProductDTO>> SearchProductsAsync(SearchProductRequestDTO searchRequest);
        
        Task<List<CategoryDTO>> SearchCategoryAsync(SearchCategoryRequestDTO searchRequest);
    }
}
