using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Infrastructure.Data;
using Base.Infrastructure.Interceptors;
using Base.Infrastructure.IRepository;
using Base.Infrastructure.IService;
using Base.Infrastructure.Repository;
using Base.Infrastructure.Services;
using CloudinaryDotNet;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Base.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cloudinaryConfig = configuration.GetSection("CloudinaryConfig").Get<CloudinaryConfig>();
        Account cloudinaryAccount = new Account
        {
            Cloud = cloudinaryConfig.CloudName,
            ApiKey = cloudinaryConfig.ApiKey,
            ApiSecret = cloudinaryConfig.ApiSecret,
        };
        Cloudinary cloudinary = new Cloudinary(cloudinaryAccount);
        services.AddSingleton(cloudinary);

        services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<ApplicationDbContext>( (sp, option) =>
        {
            //var auditableInterceptor = sp.GetService<UpdateAuditableEntitiesInterceptor>();

            option.UseSqlServer(configuration.GetConnectionString("MsSQLConnection"), b =>
            {
                b.UseHierarchyId();
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            })
                .UseExceptionProcessor();
                //.AddInterceptors(auditableInterceptor!);
        });

        #region Identity
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

            options.Tokens.EmailConfirmationTokenProvider = "CustomerTokenProvider";
            options.Tokens.ChangeEmailTokenProvider = "CustomerTokenProvider";
            options.Tokens.PasswordResetTokenProvider = "CustomerTokenProvider";
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddTokenProvider<TokenProvider<Customer>>("CustomerTokenProvider");

        services.AddIdentity<User,Role>(options =>
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

            options.Tokens.EmailConfirmationTokenProvider = "UserTokenProvider";
            options.Tokens.ChangeEmailTokenProvider = "UserTokenProvider";
            options.Tokens.PasswordResetTokenProvider = "UserTokenProvider";
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddTokenProvider<TokenProvider<User>>("UserTokenProvider");
        #endregion

        #region Entity
        services.AddTransient<IApplicationDbContext, ApplicationDbContext>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserTwoFactorTokenProvider<IdentityUser<Guid>>, TokenProvider<IdentityUser<Guid>>>();

        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IVoucherTypeRepository, VoucherTypeRepository>();
        services.AddScoped<IExpiredDateExtensionRepository, ExpiredDateExtensionRepository>();


        services.AddScoped<IStatisticalService, StatisticalService>();
        services.AddScoped<ILogService, LogService>();
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
