﻿using Base.Core.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Core.ViewModel;


public class UpdateInformationVM
{
    public string? Name { get; set; }
    public string? CitizenId { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ResetPasswordVM
{
    [Required]
    public string? OldPassword { get; set; }

    [Required]
    public string? NewPassword { get; set; }

    [Required]
    public string? ConfirmPassword { get; set; }
}

public class LoginUserVM
{
    [Required]
    public string UserName { get; set; } = "";

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";
}

public class LoginCustomerVM
{
    [Required]
    public string AccountInformation { get; set; } = "";

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";
}

public class CustomerVM
{
    [Required]
    public string? Name { get; set; }
    public string? CitizenId { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? PhoneNumberConfirmed { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public bool? IsBlocked { get; set; }

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";
    [Required]
    [MinLength(5)]
    public string ConfirmPassword { get; set; } = "";

    public IEnumerable<Guid>? SalesEmployeeIds { get; set; }
}

public class UserVM
{
    public string? Name { get; set; }
    [Required]
    public string? UserName { get; set; }
    public string? CitizenId { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? PhoneNumberConfirmed { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public bool? IsBlocked { get; set; }

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";
    [Required]
    [MinLength(5)]
    public string ConfirmPassword { get; set; } = "";

    public Guid? ManagerId { get; set; }

    [Required]
    public List<Guid> RoleIds { get; set; } = new();
}

//=========================================================

public class RoleVM
{
    [Required]
    public string? RoleName { get; set; }
    public bool IsManager { get; set; } = false;
    public IEnumerable<ClaimVM>? Claims { get; set; }
}

public class UpdatedRoleVM
{
    [Required]
    public string? RoleName { get; set; }
    public bool IsManager { get; set; } = false;
}

public class ClaimVM
{
    [Required]
    public string? Resource { get; set; }
    public bool Read { get; set; } = false;
    public bool Write { get; set; } = false;
    public bool Update { get; set; } = false;
    public bool Delete { get; set; } = false;
}

public class UpdatedClaimVM
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string? Resource { get; set; }
    public bool Read { get; set; } = false;
    public bool Write { get; set; } = false;
    public bool Update { get; set; } = false;
    public bool Delete { get; set; } = false;
}

//===========================================================

public class BookingVM
{
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public int ServicePackageId { get; set; }
    public string BookingTitle { get; set; } = "";
    public int BookingStatus { get; set; }
    public Decimal TotalPrice { get; set; }
    public string? PriceDetails { get; set; }
    public string? Note { get; set; }
    public string? Descriptions { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public List<int>? VoucherIds { get; set; }
}

public class UpdatedBookingVM
{
    [Required]
    public string? BookingTitle { get; set; }
    public int BookingStatus { get; set; }
    public Decimal TotalPrice { get; set; }
    public string? PriceDetails { get; set; }
    public string? Note { get; set; }
    public string? Descriptions { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
}

public class ServiceVM
{
    [Required]
    public string? ServiceName { get; set; }
    public string? Description { get; set; }
}

public class ServicePackageVM
{
    [Required]
    public string? ServicePackageName { get; set; }
    public string? Description { get; set; }
    [Required]
    public List<int> ServicesIds { get; set; } = new();
}

public class UpdatedServicePackageVM
{
    [Required]
    public string? ServicePackageName { get; set; }
    public string? Description { get; set; }
}

public class UpdatedServicesInPackageVM
{
    public int Id { get; set; }
    public bool IsDelete { get; set; }
}

//============================================================

public class VoucherTypeVM
{
    [Required]
    public string? TypeName { get; set; }
    public bool IsActiveNow { get; set; } = false;
    [Required]
    public Decimal CommonPrice { get; set; }
    public int AvailableNumberOfVouchers { get; set; } = 0;
    public int? PercentageDiscount { get; set; }
    public Decimal? ValueDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
    public List<int>? ServicePackageIds { get; set; }
}

public class UpdatedVoucherTypeVM
{
    [Required]
    public string? TypeName { get; set; }
    public bool IsAvailable { get; set; } = false;
    [Required]
    public Decimal CommonPrice { get; set; }
    public int AvailableNumberOfVouchers { get; set; } = 0;
    public int? PercentageDiscount { get; set; }
    public Decimal? ValueDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
}

public class VoucherVM
{
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public int VoucherTypeId { get; set; }
    [Required]
    public DateTime ExpiredDate { get; set; }
    [Required]
    public Decimal ActualPurchasePrice { get; set; }
}

public class UpdatedVoucherVM
{
    public DateTime ExpiredDate { get; set; }
    public Decimal ActualPrice { get; set; }
    public Decimal? UsedValueDiscount { get; set; }
    public int VoucherStatus { get; set; }
}

public class ExpiredDateExtensionVM
{
    [Required]
    public int VoucherId { get; set; }
    public Decimal Price { get; set; }
    [Required]
    public DateTime NewExpiredDate { get; set; }
}

public class UpdatedExpiredDateExtensionVM
{
    public Decimal Price { get; set; }
    [Required]
    public DateTime NewExpiredDate { get; set; }
}