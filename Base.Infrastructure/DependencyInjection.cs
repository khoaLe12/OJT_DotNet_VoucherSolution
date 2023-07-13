using Base.Core.Entity;
using Base.Core.Identity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IRepository;
using Base.Infrastructure.IService;
using Base.Infrastructure.Repository;
using Base.Infrastructure.Services;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(option => 
            option.UseSqlServer(configuration.GetConnectionString("MsSQLConnection"), b => b.UseHierarchyId())
            .UseExceptionProcessor());
        
        #region Identity
        services.AddIdentity<User, Role>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;

            //password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 5;
            options.Password.RequiredUniqueChars = 0;

            //Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            //UserName settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        }).AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddIdentityCore<Customer>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;

            //password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 5;
            options.Password.RequiredUniqueChars = 0;

            //Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            //UserName settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        }).AddEntityFrameworkStores<ApplicationDbContext>();


        #endregion

        #region Entity
        services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IVoucherTypeRepository, VoucherTypeRepository>();
        services.AddScoped<IExpiredDateExtensionRepository, ExpiredDateExtensionRepository>();

        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IServicePackageService, ServicePackageService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IVoucherService, VoucherService>();
        services.AddScoped<IVoucherTypeService, VoucherTypeService>();
        services.AddScoped<IExpiredDateExtensionService, ExpiredDateExtensionService>();
        #endregion

        return services;
    }
}
