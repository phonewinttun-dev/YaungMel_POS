using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.domain.DTOs;

public class PageSettingDTO
{
    public PageSettingDTO() { }
    public PageSettingDTO(int pageNo, int pageSize, int pageCount)
    {
        PageNo = pageNo;
        PageSize = pageSize;
        PageCount = pageCount;
    }
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
}
