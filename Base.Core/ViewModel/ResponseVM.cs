using Base.Core.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.ViewModel;

public class ResponseLogVM
{
    public int Id { get; set; }
    public string? ActionType { get; set; }
    public string? CreatedBy { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string,object>? Changes { get; set; }
    public bool? IsRestored { get; set; }
}

public class ResponseCustomerInformationVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? CitizenId { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsBlocked { get; set; }
    public IEnumerable<ResponseBookingVM>? Bookings { get; set; }
    public IEnumerable<ResponseVoucherVM>? Vouchers { get; set; }
    public IEnumerable<ResponseUserVM>? Users { get; set; }
    public string? FilePath { get; set; }
}

public class ResponseUserInformationVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? CitizenId { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsBlocked { get; set; }
    public ResponseUserVM? SalesManager { get; set; }
    public IEnumerable<ResponseCustomerVM>? Customers { get; set; }
    public IEnumerable<ResponseRoleForUserVM>? Roles { get; set; }
    public IEnumerable<string>? Permission { get; set; }
    public string? FilePath { get; set; }
    public IEnumerable<ResponseUserVM>? Managers { get; set; }
    public IEnumerable<ResponseUserVM>? ManagedUsers { get; set; }
}

public class ResponseRoleForUserVM
{
    public Guid Id { get; set; }
    public string? NormalizedName { get; set; }
    public bool IsManager { get; set; }
}

public class ResponseRoleVM
{
    public Guid Id { get; set; }
    public string? NormalizedName { get; set; }
    public bool IsManager { get; set; }
    public IEnumerable<ResponseRoleClaimVM>? RoleClaims { get; set; }
}

public class ResponseRoleClaimVM
{
    public int Id { get; set; }
    public string? ClaimValue { get; set; }
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
    public string? PhoneNumber { get; set; }
    public string? FilePath { get; set; }
}

public class ResponseServiceVM
{
    public int Id { get; set; }
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
    public IEnumerable<ResponseServicePackageVM>? ServicePackages { get; set; }
}

public class ResponseServicePackageVM
{
    public int Id { get; set; }
    public string? ServicePackageName { get; set; }
    public string? Description { get; set; }
    public IEnumerable<ResponseServiceVM>? Services { get; set; }
    public IEnumerable<ResponseVoucherTypeVM>? ValuableVoucherTypes { get; set; }
}

public class ResponseBookingVM
{
    public int Id { get; set; }
    public ResponseCustomerVM? Customer { get; set; }
    public ResponseUserVM? SalesEmployee { get; set; }
    public IEnumerable<ResponseVoucherForBookingVM>? Vouchers { get; set; }
    public ResponseServicePackageVM? ServicePackage { get; set; }
    public string? BookingTitle { get; set; }
    public DateTime BookingDate { get; set; }
    public string? BookingStatus { get; set; }
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
    public ResponseVoucherTypeForBookingVM? VoucherType { get; set; }
    public Decimal? UsedValueDiscount { get; set; }
    public string? VoucherStatus { get; set; }
}

public class ResponseVoucherTypeForBookingVM
{
    public int Id { get; set; }
    public string? TypeName { get; set; }
    public int? PercentageDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
}
//=============================

public class ResponseVoucherTypeVM
{
    public int Id { get; set; }
    public string? TypeName { get; set; }
    public bool IsAvailable { get; set; }
    public Decimal CommonPrice { get; set; }
    public int AvailableNumberOfVouchers { get; set; }
    public int? PercentageDiscount { get; set; }
    public Decimal? ValueDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
    public IEnumerable<ResponseVoucherVM>? Vouchers { get; set; }
    public IEnumerable<ResponseServicePackageVM>? UsableServicePackages { get; set; }
}

public class ResponseVoucherVM
{
    public int Id { get; set; }
    public ResponseCustomerVM? Customer { get; set; }
    public ResponseUserVM? SalesEmployee { get; set; }
    public ResponseVoucherTypeVM? VoucherType { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    public Decimal ActualPrice { get; set; }
    public Decimal? UsedValueDiscount { get; set; }
    public string? VoucherStatus { get; set; }
    public IEnumerable<ResponseBookingVM>? Bookings { get; set; }
    public IEnumerable<ResponseExpiredDateExtensionVM>? VoucherExtensions { get; set; }
}

public class ResponseExpiredDateExtensionVM
{
    public int Id { get; set; }
    public ResponseVoucherForExtensionVM? Voucher { get; set; }
    public ResponseUserVM? SalesEmployee { get; set; }
    public Decimal Price { get; set; }
    public DateTime ExtendedDateTime { get; set; }
    public DateTime OldExpiredDate { get; set; }
    public DateTime NewExpiredDate { get; set; }
}

//Used for ExpiredDateExtension entity
public class ResponseVoucherForExtensionVM
{
    public int Id { get; set; }
    public ResponseCustomerVM? Customer { get; set; }
    public ResponseVoucherTypeForExtensionVM? VoucherType { get; set; }
    public Decimal? UsedValueDiscount { get; set; }
    public string? VoucherStatus { get; set; }
}

public class ResponseVoucherTypeForExtensionVM
{
    public int Id { get; set; }
    public string? TypeName { get; set; }
    public string? ConditionsAndPolicies { get; set; }
}
//===================================

public class ResponseConsumptionStatisticVM
{
    public int Year { get; set; }
    public Dictionary<int,Decimal>? MonthlyStatistic { get; set; }
}

public class ResponseServicePackageStatisticByNumberOfBookingsVM
{
    public string? ServicePackageName { get; set; }
    public int BookingNumbers { get; set; }
}

public class ResponseServicePackageStatisticByTotalSpendingVM
{
    public string? ServicePackageName { get; set; }
    public Decimal TotalSpending { get; set; }
}
