using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Common;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}

public enum AuditType
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3,
}
