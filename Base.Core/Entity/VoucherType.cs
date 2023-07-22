using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.Entity;

public class VoucherType
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string TypeName { get; set; } = "";
    [Required]
    public bool IsAvailable { get; set; } = false;
    [Column(TypeName = "money")]
    public Decimal CommonPrice { get; set; }
    public int AvailableNumberOfVouchers { get; set; }
    public int? PercentageDiscount { get; set; }
    [Column(TypeName = "money")]
    public Decimal? ValueDiscount { get; set; }
    [Column(TypeName = "money")]
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
    public IEnumerable<Voucher>? Vouchers { get; set; }
    public IEnumerable<ServicePackage>? UsableServicePackages { get; set; }
    public bool IsDeleted { get; set; } = false;
}
