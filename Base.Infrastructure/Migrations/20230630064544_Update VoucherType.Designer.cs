﻿// <auto-generated />
using System;
using Base.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Base.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230630064544_Update VoucherType")]
    partial class UpdateVoucherType
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Base.Core.Entity.Booking", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("BookingStatus")
                        .HasColumnType("int");

                    b.Property<string>("BookingTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Descriptions")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PriceDetails")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SalesEmployeeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ServicePackageId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDateTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("money");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("SalesEmployeeId");

                    b.HasIndex("ServicePackageId");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("Base.Core.Entity.ExpiredDateExtension", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExtendedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NewExpiredDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("OldExpiredDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Price")
                        .HasColumnType("money");

                    b.Property<Guid>("SalesEmployeeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("VoucherId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SalesEmployeeId");

                    b.HasIndex("VoucherId");

                    b.ToTable("ExpiredDateExtensions");
                });

            modelBuilder.Entity("Base.Core.Entity.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Base.Core.Entity.Service", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ServiceName")
                        .IsUnique();

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Base.Core.Entity.ServicePackage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ServicePackageName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ServicePackages");
                });

            modelBuilder.Entity("Base.Core.Entity.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("ManagerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ManagerId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Base.Core.Entity.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Base.Core.Entity.Voucher", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ActualPrice")
                        .HasColumnType("money");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ExpiredDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("IssuedDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SalesEmployeeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("UsedValueDiscount")
                        .HasColumnType("money");

                    b.Property<int>("VoucherStatus")
                        .HasColumnType("int");

                    b.Property<int>("VoucherTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("SalesEmployeeId");

                    b.HasIndex("VoucherTypeId");

                    b.ToTable("Vouchers");
                });

            modelBuilder.Entity("Base.Core.Entity.VoucherType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AvailableNumberOfVouchers")
                        .HasColumnType("int");

                    b.Property<decimal>("CommonPrice")
                        .HasColumnType("money");

                    b.Property<string>("ConditionsAndPolicies")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<decimal?>("MaximumValueDiscount")
                        .HasColumnType("money");

                    b.Property<int?>("PercentageDiscount")
                        .HasColumnType("int");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("ValueDiscount")
                        .HasColumnType("money");

                    b.HasKey("Id");

                    b.ToTable("VoucherTypes");
                });

            modelBuilder.Entity("Base.Core.Identity.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("CitizenId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CitizenId")
                        .IsUnique()
                        .HasFilter("[CitizenId] IS NOT NULL");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique()
                        .HasFilter("[PhoneNumber] IS NOT NULL");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("BookingVoucher", b =>
                {
                    b.Property<int>("BookingsId")
                        .HasColumnType("int");

                    b.Property<int>("VouchersId")
                        .HasColumnType("int");

                    b.HasKey("BookingsId", "VouchersId");

                    b.HasIndex("VouchersId");

                    b.ToTable("BookingVoucher");
                });

            modelBuilder.Entity("CustomerUser", b =>
                {
                    b.Property<Guid>("CustomersId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SalesEmployeesId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CustomersId", "SalesEmployeesId");

                    b.HasIndex("SalesEmployeesId");

                    b.ToTable("CustomerUser");
                });

            modelBuilder.Entity("SeviceInPackages", b =>
                {
                    b.Property<int>("ServiceId")
                        .HasColumnType("int");

                    b.Property<int>("ServicePackageId")
                        .HasColumnType("int");

                    b.HasKey("ServiceId", "ServicePackageId");

                    b.HasIndex("ServicePackageId");

                    b.ToTable("SeviceInPackages");
                });

            modelBuilder.Entity("VoucherTypeServicePackage", b =>
                {
                    b.Property<int>("ServicePackageId")
                        .HasColumnType("int");

                    b.Property<int>("VoucherTypeId")
                        .HasColumnType("int");

                    b.HasKey("ServicePackageId", "VoucherTypeId");

                    b.HasIndex("VoucherTypeId");

                    b.ToTable("VoucherTypeServicePackage");
                });

            modelBuilder.Entity("Base.Core.Entity.Booking", b =>
                {
                    b.HasOne("Base.Core.Identity.Customer", "Customer")
                        .WithMany("Bookings")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.User", "SalesEmployee")
                        .WithMany("Bookings")
                        .HasForeignKey("SalesEmployeeId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.ServicePackage", "ServicePackage")
                        .WithMany("Bookings")
                        .HasForeignKey("ServicePackageId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("SalesEmployee");

                    b.Navigation("ServicePackage");
                });

            modelBuilder.Entity("Base.Core.Entity.ExpiredDateExtension", b =>
                {
                    b.HasOne("Base.Core.Entity.User", "SalesEmployee")
                        .WithMany("expiredDateExtensions")
                        .HasForeignKey("SalesEmployeeId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.Voucher", "Voucher")
                        .WithMany("VoucherExtensions")
                        .HasForeignKey("VoucherId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("SalesEmployee");

                    b.Navigation("Voucher");
                });

            modelBuilder.Entity("Base.Core.Entity.User", b =>
                {
                    b.HasOne("Base.Core.Entity.User", "SalesManager")
                        .WithMany("SalesEmployee")
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("SalesManager");
                });

            modelBuilder.Entity("Base.Core.Entity.UserRole", b =>
                {
                    b.HasOne("Base.Core.Entity.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Base.Core.Entity.Voucher", b =>
                {
                    b.HasOne("Base.Core.Identity.Customer", "Customer")
                        .WithMany("Vouchers")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.User", "SalesEmployee")
                        .WithMany("Vouchers")
                        .HasForeignKey("SalesEmployeeId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.VoucherType", "VoucherType")
                        .WithMany("Vouchers")
                        .HasForeignKey("VoucherTypeId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("SalesEmployee");

                    b.Navigation("VoucherType");
                });

            modelBuilder.Entity("BookingVoucher", b =>
                {
                    b.HasOne("Base.Core.Entity.Booking", null)
                        .WithMany()
                        .HasForeignKey("BookingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.Voucher", null)
                        .WithMany()
                        .HasForeignKey("VouchersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CustomerUser", b =>
                {
                    b.HasOne("Base.Core.Identity.Customer", null)
                        .WithMany()
                        .HasForeignKey("CustomersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("SalesEmployeesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SeviceInPackages", b =>
                {
                    b.HasOne("Base.Core.Entity.Service", null)
                        .WithMany()
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.ServicePackage", null)
                        .WithMany()
                        .HasForeignKey("ServicePackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VoucherTypeServicePackage", b =>
                {
                    b.HasOne("Base.Core.Entity.ServicePackage", null)
                        .WithMany()
                        .HasForeignKey("ServicePackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Base.Core.Entity.VoucherType", null)
                        .WithMany()
                        .HasForeignKey("VoucherTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Base.Core.Entity.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Base.Core.Entity.ServicePackage", b =>
                {
                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("Base.Core.Entity.User", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("SalesEmployee");

                    b.Navigation("UserRoles");

                    b.Navigation("Vouchers");

                    b.Navigation("expiredDateExtensions");
                });

            modelBuilder.Entity("Base.Core.Entity.Voucher", b =>
                {
                    b.Navigation("VoucherExtensions");
                });

            modelBuilder.Entity("Base.Core.Entity.VoucherType", b =>
                {
                    b.Navigation("Vouchers");
                });

            modelBuilder.Entity("Base.Core.Identity.Customer", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("Vouchers");
                });
#pragma warning restore 612, 618
        }
    }
}
