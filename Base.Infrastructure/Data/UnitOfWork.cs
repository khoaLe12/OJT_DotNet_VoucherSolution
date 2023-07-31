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
    IUserRepository Users { get; }
    IExpiredDateExtensionRepository ExpiredDateExtensions { get; }
    IRoleRepository Roles { get; }
    IRoleClaimRepository RoleClaims { get; }
    IAuditRepository AuditLogs { get; }
    Task<bool> SaveChangesAsync();
    Task<bool> SaveDeletedChangesAsync();
    Task<bool> SaveChangesNoLogAsync();
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
    public IUserRepository Users { get; private set; }
    public IExpiredDateExtensionRepository ExpiredDateExtensions { get; private set; }
    public IRoleRepository Roles { get; private set; }
    public IRoleClaimRepository RoleClaims { get; private set; }
    public IAuditRepository AuditLogs { get; private set; }

    public UnitOfWork(ApplicationDbContext applicationDbContext,
        //ILogger logger,
        IServicePackageRepository servicePackages, 
        IServiceRepository services, 
        IVoucherRepository vouchers, 
        IVoucherTypeRepository voucherTypes, 
        IBookingRepository bookings, 
        ICustomerRepository customers, 
        IUserRepository users, 
        IExpiredDateExtensionRepository expiredDateExtensionRepository,
        IRoleRepository roleRepository,
        IRoleClaimRepository roleClaimRepository,
        IAuditRepository auditRepository)
    {
        _applicationDbContext = applicationDbContext;
        //_logger = logger
        ServicePackages = servicePackages;
        Services = services;
        Vouchers = vouchers;
        VoucherTypes = voucherTypes;
        Bookings = bookings;
        Customers = customers;
        Users = users;
        ExpiredDateExtensions = expiredDateExtensionRepository;
        Roles = roleRepository;
        RoleClaims = roleClaimRepository;
        AuditLogs = auditRepository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _applicationDbContext.SaveChangesAsync()) > 0;
    }

    public async Task<bool> SaveDeletedChangesAsync()
    {
        return (await _applicationDbContext.SaveDeletedChangesAsync() > 0);
    }

    public async Task<bool> SaveChangesNoLogAsync()
    {
        return (await _applicationDbContext.SaveChangesNoLogAsync() > 0);
    }

    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }
}
