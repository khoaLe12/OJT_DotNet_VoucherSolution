using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Base.Infrastructure.Data;

public interface IApplicationDbContext
{
    public DbSet<Log> Logs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<VoucherType> VoucherTypes { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<ExpiredDateExtension> ExpiredDateExtensions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, RoleClaim, IdentityUserToken<Guid>>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
	{
        _currentUserService = currentUserService;
	}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    public DbSet<Log> Logs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<VoucherType> VoucherTypes { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<ExpiredDateExtension> ExpiredDateExtensions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Roles).WithMany(r => r.Users)
                .UsingEntity<IdentityUserRole<Guid>>();
            entity.Property(u => u.PathFromRootManager)
                .HasDefaultValueSql("hierarchyid::Parse('/')");
        });

        builder.Entity<RoleClaim>(entity =>
        {
            /*Using Fluent API to create new shadow property that hold information of update DateTime
            This Property will not be showed on model
            But we can change it value by using ChangeTracker
            entity.Property<DateTime>("lastUpdate");*/
            entity.HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.CitizenId)
                .IsUnique();
            entity.HasIndex(e => e.PhoneNumber)
                .IsUnique();
            entity.HasIndex(e => e.Email)
                .IsUnique();
            entity.Property(e => e.Name)
                .IsRequired();
            entity.Property(e => e.UserName)
                .IsRequired(false);
        });

        builder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Customer).WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(b => b.SalesEmployee).WithMany(u => u.Bookings)
                .HasForeignKey(b => b.SalesEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(b => b.ServicePackage).WithMany(sp => sp.Bookings)
                .HasForeignKey(b => b.ServicePackageId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Voucher>(entity =>
        {
            entity.HasOne(v => v.SalesEmployee).WithMany(u => u.Vouchers)
                .HasForeignKey(v => v.SalesEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(v => v.Customer).WithMany(c => c.Vouchers)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<VoucherType>(entity =>
        {
            //Can not delete Voucher Type when there are still one or more Voucher reference to it
            entity.HasMany(vt => vt.Vouchers).WithOne(v => v.VoucherType)
                .HasForeignKey(v => v.VoucherTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(vt => vt.UsableServicePackages).WithMany(sp => sp.ValuableVoucherTypes)
                .UsingEntity(
                    "VoucherTypeServicePackage",
                    l => l.HasOne(typeof(ServicePackage)).WithMany().HasForeignKey("ServicePackageId").HasPrincipalKey(nameof(ServicePackage.Id)),
                    r => r.HasOne(typeof(VoucherType)).WithMany().HasForeignKey("VoucherTypeId").HasPrincipalKey(nameof(VoucherType.Id)),
                    j => 
                    {
                        j.HasKey("ServicePackageId", "VoucherTypeId");
                    });
        });

        builder.Entity<ServicePackage>(entity =>
        {
            entity.HasMany(sp => sp.Services).WithMany(s => s.ServicePackages)
                .UsingEntity(
                    "SeviceInPackages",
                    l => l.HasOne(typeof(Service)).WithMany().HasForeignKey("ServiceId").HasPrincipalKey(nameof(Service.Id)),
                    r => r.HasOne(typeof(ServicePackage)).WithMany().HasForeignKey("ServicePackageId").HasPrincipalKey(nameof(ServicePackage.Id)),
                    j => j.HasKey("ServiceId" , "ServicePackageId"));
        });

        builder.Entity<ExpiredDateExtension>(entity =>
        {
            entity.HasOne(e => e.Voucher).WithMany(v => v.VoucherExtensions)
                .HasForeignKey(e => e.VoucherId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.SalesEmployee).WithMany(u => u.expiredDateExtensions)
                .HasForeignKey(e => e.SalesEmployeeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
    }
}
