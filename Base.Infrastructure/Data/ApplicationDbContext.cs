using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Data;

public interface IApplicationDbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<VoucherType> VoucherTypes { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<ExpiredDateExtension> ExpiredDateExtensions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options)
	{
        _currentUserService = currentUserService;
	}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if(entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = _currentUserService.UserId;
                entry.Entity.CreatedAt = DateTime.Now;
            }
        }
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<User> Users { get; set; }
	public DbSet<Role> Roles { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ServicePackage> ServicePackages { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<VoucherType> VoucherTypes { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<ExpiredDateExtension> ExpiredDateExtensions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
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

        builder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.SalesManager).WithMany(u => u.SalesEmployee)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(u => u.Roles).WithMany(r => r.Users)
                .UsingEntity(
                    "UserRoles",
                    l => l.HasOne(typeof(Role)).WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                    r => r.HasOne(typeof(User)).WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)),
                    j => j.HasKey("UserId", "RoleId")
                );
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
                    j => j.HasKey("ServicePackageId", "VoucherTypeId"));
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

        builder.Entity<Service>(entity =>
        {
            entity.HasIndex(e => e.ServiceName)
                .IsUnique();
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

        base.OnModelCreating(builder);
    }
}
