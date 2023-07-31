using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

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
    Task<int> SaveDeletedChangesAsync(CancellationToken cancellationToken = new CancellationToken());
    Task<int> SaveChangesNoLogAsync(CancellationToken cancellationToken = new CancellationToken());
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
        // Get Audit Entry
        var auditEntries = OnBeforeSaveChanges(false);

        // Save current entity
        var result = await base.SaveChangesAsync(cancellationToken);

        //Save audit entries
        await OnAfterSaveChangesAsync(auditEntries);
        return result;
    }

    public async Task<int> SaveDeletedChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var auditEntries = OnBeforeSaveChanges(true);

        var result = await base.SaveChangesAsync(cancellationToken);

        await OnAfterSaveChangesAsync(auditEntries);

        return result;
    }

    public async Task<int> SaveChangesNoLogAsync(CancellationToken cancellationToken = default)
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

        // Define a custom value comparer for Dictionary<string,object>
        var dictionaryValueComparer = new ValueComparer<Dictionary<string, object>>(
            (c1, c2) => JsonConvert.SerializeObject(c1) == JsonConvert.SerializeObject(c2),
            c => c == null ? 0 : JsonConvert.SerializeObject(c).GetHashCode(),
            c => JsonConvert.DeserializeObject<Dictionary<string,object>>(JsonConvert.SerializeObject(c))!);

        builder.Entity<Log>(entity =>
        {
            entity.Property(l => l.Changes).HasConversion(
                value => JsonConvert.SerializeObject(value),
                serializedValue => JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedValue))
                .Metadata.SetValueComparer(dictionaryValueComparer);
        });

        builder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Roles).WithMany(r => r.Users)
                .UsingEntity<IdentityUserRole<Guid>>();
            entity.Property(u => u.PathFromRootManager)
                .HasDefaultValueSql("hierarchyid::Parse('/')");
            entity.HasIndex(e => e.CitizenId)
                .IsUnique();
            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        builder.Entity<Role>().HasIndex(r => r.NormalizedName).IsUnique(false);

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

    private List<Log> OnBeforeSaveChanges(bool IsDeleted)
    {
        string userId;
        string userName;
        try
        {
            userId = _currentUserService.UserId.ToString();
            userName = _currentUserService.UserName;
        }
        catch(InvalidOperationException)
        {
            userId = "Unauthenticated User";
            userName = "Unauthenticated User";
        }

        ChangeTracker.DetectChanges();
        var entries = new List<Log>();
        var dateTime = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || !(entry.Entity is IAuditable))
            {
                continue;
            }
            else
            {
                int actionType = 0;
                Dictionary<string, object> changes = new();

                // Check if it is added
                if (entry.State == EntityState.Added)
                {
                    actionType = (int)AuditType.Create;
                    changes = entry.Properties.Select(p => new { p.Metadata.Name, p.CurrentValue }).ToDictionary(i => i.Name, i => i.CurrentValue)!;

                    // Check if create new user
                    if (changes.Any(c => c.Key == "PasswordHash"))
                    {
                        changes["PasswordHash"] = "*****************";
                    }
                }

                // Check if it is deleted
                if (entry.State == EntityState.Deleted)
                {
                    actionType = (int)AuditType.Delete;
                    changes = entry.Properties.Select(p => new { p.Metadata.Name, p.CurrentValue }).ToDictionary(i => i.Name, i => i.CurrentValue)!;
                }

                // Check if it is soft deleted
                if (entry.State == EntityState.Modified && IsDeleted)
                {
                    actionType = (int)AuditType.Delete;
                    changes = entry.Properties.Where(p => !p.IsModified).Select(p => new { p.Metadata.Name, p.CurrentValue }).ToDictionary(i => i.Name, i => i.CurrentValue)!;
                }

                // Check if it is updated
                if (entry.State == EntityState.Modified && !IsDeleted)
                {
                    actionType = (int)AuditType.Update;
                    changes = entry.Properties.Where(p => p.IsModified).Select(p => new { p.Metadata.Name, p.CurrentValue }).ToDictionary(i => i.Name, i => i.CurrentValue)!;

                    // Check if it is password update
                    var updatePassword = changes.Where(c => c.Key == "PasswordHash");
                    if (!updatePassword.IsNullOrEmpty())
                    {
                        changes = new Dictionary<string, object>();
                        changes.Add("Password", "**********************");
                    }
                }

                var auditEntry = new Log
                {
                    Type = actionType,
                    CreatedBy = userId,
                    UserName = userName,
                    CreatedAt = dateTime,
                    PrimaryKey = entry.Properties.Single(p => p.Metadata.IsPrimaryKey()).CurrentValue!.ToString(),
                    TableName = entry.Metadata.ClrType.Name,
                    Changes = changes,
                    TempProperties = entry.Properties.Where(p => p.IsTemporary).ToList(),
                };

                entries.Add(auditEntry);
            }
        }

        return entries;
    }

    private Task OnAfterSaveChangesAsync(List<Log> auditEntries)
    {
        if (auditEntries.IsNullOrEmpty())
        {
            return Task.CompletedTask;
        }

        foreach(var entry in auditEntries)
        {
            foreach(var prop in entry.TempProperties!)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.PrimaryKey = prop.CurrentValue!.ToString();
                    entry.Changes![prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.Changes![prop.Metadata.Name] = prop.CurrentValue!;
                }
            }
        }

        Logs.AddRange(auditEntries);
        return SaveChangesAsync();
    }
}
