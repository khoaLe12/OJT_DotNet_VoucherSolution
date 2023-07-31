using Base.Core.Common;
using Base.Core.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Base.Core.Entity;

public class Voucher : IAuditable
{
    [Key]
    public int Id { get; set; }

    //A Customer Who own the Voucher
    public Customer? Customer { get; set; }
    public Guid CustomerId { get; set; }


    //A Sale Employees who sale the Voucher
    public User? SalesEmployee { get; set; }
    public Guid SalesEmployeeId { get; set; }


    //Voucher Type
    public VoucherType? VoucherType { get; set; }
    public int VoucherTypeId { get; set; }


    //Vouchers's Informations of each Customer are different
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiredDate { get; set; }

    [Column(TypeName = "money")] //For postgreSQL
    public Decimal ActualPrice { get; set; }

    [Column(TypeName = "money")] //[Column(TypeName = "decimal(7,2)")]
    public Decimal? UsedValueDiscount { get; set; }

    public int VoucherStatus { get; set; }

    //A list of used Bookings
    public IEnumerable<Booking>? Bookings { get; set; }

    //A list of extension information
    public IEnumerable<ExpiredDateExtension>? VoucherExtensions { get; set; }

    public bool IsDeleted { get; set; } = false;
}
