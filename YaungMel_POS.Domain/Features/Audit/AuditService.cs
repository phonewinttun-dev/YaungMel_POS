using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;

namespace YaungMel_POS.Domain.Features.Audit
{
    public class AuditService : IAuditService
    {
        private readonly POSDbContext _db;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(POSDbContext db)
        {
            _db = db;

            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task LogAsync<T>(T entity, string action, int userId,
        string? oldValues = null, string? entityName = null)
        {
            var cleanEntityName = entityName ?? typeof(T).Name.Replace("Tbl_", "");

            // Get EntityId safely
            var entityId = GetEntityId(entity);

            var audit = new Tbl_AuditLog
            {
                EntityName = cleanEntityName,
                Action = action,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = (action != "Delete") ? JsonSerializer.Serialize(entity, _jsonOptions) : null,
                ChangedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.AuditLogs.Add(audit);
            await _db.SaveChangesAsync();
        }

        public Task LogCreateAsync<T>(T entity, int userId, string? entityName = null)
            => LogAsync(entity, "Create", userId, entityName: entityName);

        public Task LogUpdateAsync<T>(T entity, int userId, string? oldValues = null, string? entityName = null)
            => LogAsync(entity, "Update", userId, oldValues, entityName);

        public Task LogDeleteAsync<T>(T entity, int userId, string? entityName = null)
            => LogAsync(entity, "Delete", userId, entityName: entityName);

        // Helper method to extract Id
        private int GetEntityId<T>(T entity)
        {
            if (entity == null) return 0;

            var idProperty = typeof(T).GetProperty("Id");

            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                return value is int intId ? intId : 0;
            }

            return 0;
        }

    }
}
