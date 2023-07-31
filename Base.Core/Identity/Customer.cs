using Base.Core.Common;
using Base.Core.Entity;
using Microsoft.AspNetCore.Identity;

namespace Base.Core.Identity;

public class Customer : IdentityUser<Guid>, IAuditable
{
    public string? Name { get; set; }
    public string? CitizenId { get; set; }

    //A list of Sales Employee who support the customer
    public IEnumerable<User>? SalesEmployees { get; set; }

    //A list of Booking Services that the customer booked
    public IEnumerable<Booking>? Bookings { get; set; }

    //A list of Voucher that owned by the Customer
    public IEnumerable<Voucher>? Vouchers { get; set; }

    public bool IsBlocked { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public string? FilePath { get; set; }
}

public class CustomerManagerResponse
{
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public Customer? LoginCustomer { get; set; }
    public string? ConfirmEmailUrl { get; set; }
}
