using Base.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Base.Core.Entity;

public class ServicePackage : IAuditable
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ServicePackageName { get; set; } = "";
    public string? Description { get; set; }

    //A list of Voucher Type that can be applied to the Service Package
    public IEnumerable<VoucherType>? ValuableVoucherTypes { get; set; }
    public IEnumerable<Booking>? Bookings { get; set; }
    public IEnumerable<Service>? Services { get; set; }
    public bool IsDeleted { get; set; } = false;
}
