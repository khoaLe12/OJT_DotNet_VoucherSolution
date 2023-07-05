using Base.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.ViewModel;

public class ResponseCustomerInformationVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public string? CitizenId { get; set; }
    public IEnumerable<ResponseBookingVM>? Bookings { get; set; }
    public IEnumerable<ResponseVoucherVM>? Vouchers { get; set; }
}

public class ResponseUserInformationVM
{
    public Guid Id { get; set; }
    public string? CitizenId { get; set; }
    public string? UserName { get; set; }
    public string? NormalizedUserName { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public ResponseUserVM? SalesManager { get; set; }
    public IEnumerable<ResponseCustomerVM>? Customers { get; set; }
    public IEnumerable<ResponseRoleVM>? Roles { get; set; }
}

public class ResponseRoleVM
{
    public int? Id { get; set; }
    public string? NormalizedName { get; set; }
}

public class ResponseCustomerVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public class ResponseUserVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public class ResponseServiceVM
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = "";
    public string? Description { get; set; }
    public IEnumerable<ResponseServicePackageVM>? ServicePackages { get; set; }
}

public class ResponseServicePackageVM
{
    public int Id { get; set; }
    public string ServicePackageName { get; set; } = "";
    public IEnumerable<ResponseServiceVM> Services { get; set; } = new List<ResponseServiceVM>();
    public IEnumerable<ResponseVoucherTypeVM>? ValuableVoucherTypes { get; set; }
}

public class ResponseBookingVM
{
    public int Id { get; set; }
    public ResponseCustomerVM Customer { get; set; } = new();
    public ResponseUserVM SalesEmployee { get; set; } = new();
    public IEnumerable<ResponseVoucherForBookingVM>? Vouchers { get; set; }
    public ResponseServicePackageVM ServicePackage { get; set; } = new();
    public string BookingTitle { get; set; } = "";
    public DateTime BookingDate { get; set; }
    public string BookingStatus { get; set; } = "";
    public Decimal TotalPrice { get; set; }
    public string? PriceDetails { get; set; }
    public string? Note { get; set; }
    public string? Descriptions { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}

//Used for Booking entity
public class ResponseVoucherForBookingVM
{
    public int Id { get; set; }
    public ResponseVoucherTypeForBookingVM VoucherType { get; set; } = new();
    public Decimal? UsedValueDiscount { get; set; }
    public string VoucherStatus { get; set; } = "";
}

public class ResponseVoucherTypeForBookingVM
{
    public int Id { get; set; }
    public string TypeName { get; set; } = "";
    public int? PercentageDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
}
//=============================

public class ResponseVoucherTypeVM
{
    public int Id { get; set; }
    public string TypeName { get; set; } = "";
    public bool IsAvailable { get; set; }
    public Decimal CommonPrice { get; set; }
    public int AvailableNumberOfVouchers { get; set; }
    public int? PercentageDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
    public IEnumerable<ResponseVoucherVM>? Vouchers { get; set; }
    public IEnumerable<ResponseServicePackageVM>? UsableServicePackages { get; set; }
}

public class ResponseVoucherVM
{
    public int Id { get; set; }
    public ResponseCustomerVM Customer { get; set; } = new();
    public ResponseUserVM SalesEmployee { get; set; } = new();
    public ResponseVoucherTypeVM VoucherType { get; set; } = new();
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    public Decimal ActualPrice { get; set; }
    public Decimal? UsedValueDiscount { get; set; }
    public string VoucherStatus { get; set; } = "";
    public IEnumerable<ResponseBookingVM>? Bookings { get; set; }
    public IEnumerable<ResponseExpiredDateExtensionVM>? VoucherExtensions { get; set; }
}

public class ResponseExpiredDateExtensionVM
{
    public int Id { get; set; }
    public ResponseVoucherForExtensionVM Voucher { get; set; } = new();
    public ResponseUserVM SalesEmployee { get; set; } = new();
    public Decimal Price { get; set; }
    public DateTime ExtendedDateTime { get; set; }
    public DateTime OldExpiredDate { get; set; }
    public DateTime NewExpiredDate { get; set; }
}

//Used for ExpiredDateExtension entity
public class ResponseVoucherForExtensionVM
{
    public int Id { get; set; }
    public ResponseCustomerVM Customer { get; set; } = new();
    public ResponseVoucherTypeForExtensionVM VoucherType { get; set; } = new();
    public Decimal? UsedValueDiscount { get; set; }
    public string VoucherStatus { get; set; } = "";
}

public class ResponseVoucherTypeForExtensionVM
{
    public int Id { get; set; }
    public string TypeName { get; set; } = "";
    public string? ConditionsAndPolicies { get; set; }
}
//===================================