using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Base.Core.Entity;

//User can be Sales Employee, Sales Admin, Super Admin
public class User : IdentityUser<Guid>
{
    [Required]
    public string? Name { get; set; }
    public string? CitizenId { get; set; }

    public IEnumerable<Role>? Roles { get; set; }

    //A list of customers that are supported by the Sales employee
    public IEnumerable<Customer>? Customers { get; set; }
    
    //A list of Bookings that are booked by the Sales Employee
    public IEnumerable<Booking>? Bookings { get; set; }

    //A List of Voucher that the Sales Employee has sold
    public IEnumerable<Voucher>? Vouchers { get; set; }
    public IEnumerable<ExpiredDateExtension>? expiredDateExtensions { get; set; }

    [Required]
    public HierarchyId PathFromRootManager { get; set; } = HierarchyId.Parse("/");

    public bool IsBlocked { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
}

public class AuthenticatedResponse
{
    public string? Token { get; set; }
    public ResponseUserInformationVM? UserInformation { get; set; }
    public ResponseCustomerInformationVM? CustomerInformation { get; set; }
}

public class UserManagerResponse
{
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public User? LoginUser { get; set; }
}
