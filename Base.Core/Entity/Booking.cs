using Base.Core.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Base.Core.Entity;

public class Booking
{
    [Key]
    public int Id { get; set; }

    //Customer who book a service package
    public Customer? Customer { get; set; }
    public Guid CustomerId { get; set; }


    //Saler Employee who provide that service package
    public User? SalesEmployee { get; set; }
    public Guid SalesEmployeeId { get; set; }


    //A list of used Vouchers
    public IEnumerable<Voucher>? Vouchers { get; set; }


    //A Used Service Package
    public ServicePackage? ServicePackage { get; set; }
    public int ServicePackageId { get; set; }


    //More information about booking
    public string BookingTitle { get; set; } = "";
    public DateTime BookingDate { get; set; }
    public int BookingStatus { get; set; }
    [Column(TypeName = "money")]
    public Decimal TotalPrice { get; set; }
    public string? PriceDetails { get; set; }
    public string? Note { get; set; }
    public string? Descriptions { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}