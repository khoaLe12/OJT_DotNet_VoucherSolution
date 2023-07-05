using Base.Core.Entity;
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
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Confirm Password does not match!!!")]
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
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

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
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";
    [Required]
    [MinLength(5)]
    public string ConfirmPassword { get; set; } = "";

    public Guid? ManagerId { get; set; }

    [Required]
    public List<int> RoleIds { get; set; } = new();
}

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

public class ServiceVM
{
    [Required]
    public string ServiceName { get; set; } = "";
    public string? Description { get; set; }
}

public class ServicePackageVM
{
    [Required]
    public string ServicePackageName { get; set; } = "";
    [Required]
    public List<int> ServicesIds { get; set; } = new();
}

public class VoucherTypeVM
{
    [Required]
    public string TypeName { get; set; } = "";
    [Required]
    public bool IsActiveNow { get; set; } = true;
    public Decimal? GeneralPurchasePrice { get; set; }
    public int? AvailableNumberOfVouchers { get; set; }
    public int? PercentageDiscount { get; set; }
    public Decimal? ValueDiscount { get; set; }
    public Decimal? MaximumValueDiscount { get; set; }
    public string? ConditionsAndPolicies { get; set; }
    public List<int>? ServicePackageIds { get; set; }
}

public class VoucherVM
{
    public Guid CustomerId { get; set; }
    [Required]
    public int VoucherTypeId { get; set; }
    [Required]
    public DateTime ExpiredDate { get; set; }
    [Required]
    public Decimal ActualPurchasePrice { get; set; }
    public IEnumerable<ExpiredDateExtensionVM>? VoucherExtensions { get; set; }
}

public class ExpiredDateExtensionVM
{
    [Required]
    public int VoucherId { get; set; }
    public Decimal Price { get; set; }
    public DateTime NewExpiredDate { get; set; }
}