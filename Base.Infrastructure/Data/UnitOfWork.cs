using Base.Infrastructure.IRepository;
using Base.Infrastructure.Repository;
using Microsoft.Extensions.Logging;

namespace Base.Infrastructure.Data;

public interface IUnitOfWork
{
    IServicePackageRepository ServicePackages { get; }
    IServiceRepository Services { get; }
    IVoucherRepository Vouchers { get; }
    IVoucherTypeRepository VoucherTypes { get; }
    IBookingRepository Bookings { get; }
    ICustomerRepository Customers { get; }
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    IExpiredDateExtensionRepository ExpiredDateExtensions { get; }
    Task<bool> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _applicationDbContext;
    //private readonly ILogger _logger;

    public IServicePackageRepository ServicePackages { get; private set; }
    public IServiceRepository Services { get; private set; }
    public IVoucherRepository Vouchers { get; private set; }
    public IVoucherTypeRepository VoucherTypes { get; private set; }
    public IBookingRepository Bookings { get; private set; }
    public ICustomerRepository Customers { get; private set; }
    public IRoleRepository Roles { get; private set; }
    public IUserRepository Users { get; private set; }
    public IExpiredDateExtensionRepository ExpiredDateExtensions { get; }

    public UnitOfWork(ApplicationDbContext applicationDbContext,
        //ILogger logger,
        IServicePackageRepository servicePackages, 
        IServiceRepository services, 
        IVoucherRepository vouchers, 
        IVoucherTypeRepository voucherTypes, 
        IBookingRepository bookings, 
        ICustomerRepository customers, 
        IRoleRepository roles, 
        IUserRepository users, 
        IExpiredDateExtensionRepository expiredDateExtensionRepository)
    {
        _applicationDbContext = applicationDbContext;
        //_logger = logger
        ServicePackages = servicePackages;
        Services = services;
        Vouchers = vouchers;
        VoucherTypes = voucherTypes;
        Bookings = bookings;
        Customers = customers;
        Roles = roles;
        Users = users;
        ExpiredDateExtensions = expiredDateExtensionRepository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _applicationDbContext.SaveChangesAsync()) > 0;
    }

    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }
}
