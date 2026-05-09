using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Domain.Features.Audit
{
    public interface IAuditService
    {
        Task LogAsync<T>(T entity, string action, int userId, string? oldValues = null, string? entityName = null);

        Task LogCreateAsync<T>(T entity, int userId, string? entityName = null);
        Task LogUpdateAsync<T>(T entity, int userId, string? oldValues = null, string? entityName = null);
        Task LogDeleteAsync<T>(T entity, int userId, string? entityName = null);
    }
}
