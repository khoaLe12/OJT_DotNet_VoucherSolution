using System.ComponentModel.DataAnnotations;

namespace Base.Core.Entity;

public class Service
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ServiceName { get; set; } = "";
    public string? Description { get; set; }
    public IEnumerable<ServicePackage>? ServicePackages { get; set; }
}
