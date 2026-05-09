using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Database.Models
{
    public class Tbl_AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string EntityName { get; set; } = string.Empty;   // e.g., "User", "Order"
        public string Action { get; set; } = string.Empty;       // Create, Update, Delete, etc.
        public int EntityId { get; set; }
        public string? OldValues { get; set; }                    // JSON
        public string? NewValues { get; set; }                    // JSON
        public int? ChangedBy { get; set; }                    // UserId or Username
    }
}
