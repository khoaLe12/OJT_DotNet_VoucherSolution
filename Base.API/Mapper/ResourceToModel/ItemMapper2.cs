using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Enum;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.Routing.Constraints;
using Newtonsoft.Json;

namespace Base.API.Mapper.ResourceToModel;

public class ItemMapper2 : Profile
{
    public ItemMapper2()
    {
        CreateMap<Log, ResponseLogVM>()
            .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => Enum.GetName(typeof(AuditType), src.Type)))
            .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.TableName))
            .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.PrimaryKey))
            .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes));
            //.ForMember(dest => dest.Changes, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Changes)));

        CreateMap<Service, ResponseServiceVM>()
            .ForMember(dest => dest.ServicePackages, opt => opt.MapFrom(src => src.ServicePackages!.Where(sp => !sp.IsDeleted)));

        CreateMap<ServicePackage, ResponseServicePackageVM>()
            .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services!.Where(s => !s.IsDeleted)))
            .ForMember(dest => dest.ValuableVoucherTypes, opt => opt.MapFrom(src => src.ValuableVoucherTypes!.Where(e => !e.IsDeleted)));

        CreateMap<Booking, ResponseBookingVM>()
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer!.IsDeleted ? null : src.Customer))
            .ForMember(dest => dest.SalesEmployee, opt => opt.MapFrom(src => src.SalesEmployee!.IsDeleted ? null : src.SalesEmployee))
            .ForMember(dest => dest.Vouchers, opt => opt.MapFrom(src => src.Vouchers!.Where(v => !v.IsDeleted)))
            .ForMember(dest => dest.ServicePackage, opt => opt.MapFrom(src => src.ServicePackage!.IsDeleted ? null : src.ServicePackage))
            .ForMember(dest => dest.BookingStatus, opt => opt.MapFrom(src => Enum.GetName(typeof(BookingStatus), src.BookingStatus)));

        CreateMap<VoucherType, ResponseVoucherTypeVM>()
            .ForMember(dest => dest.Vouchers, opt => opt.MapFrom(src => src.Vouchers!.Where(v => !v.IsDeleted)))
            .ForMember(dest => dest.UsableServicePackages, opt => opt.MapFrom(src => src.UsableServicePackages!.Where(e => !e.IsDeleted)));

        CreateMap<Voucher, ResponseVoucherVM>()
            .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer!.IsDeleted ? null : src.Customer))
            .ForMember(dest => dest.SalesEmployee, opt => opt.MapFrom(src => src.SalesEmployee!.IsDeleted ? null : src.SalesEmployee))
            .ForMember(dest => dest.VoucherType, opt => opt.MapFrom(src => src.VoucherType!.IsDeleted ? null : src.VoucherType))
            .ForMember(dest => dest.Bookings, opt => opt.MapFrom(src => src.Bookings!.Where(b => !b.IsDeleted)))
            .ForMember(dest => dest.VoucherExtensions, opt => opt.MapFrom(src => src.VoucherExtensions!.Where(e => !e.IsDeleted)))
            .ForMember(dest => dest.VoucherStatus, opt => opt.MapFrom(src => Enum.GetName(typeof(VoucherStatus), src.VoucherStatus)));

        CreateMap<Customer, ResponseCustomerVM>();

        CreateMap<Customer, ResponseCustomerInformationVM>()
            .ForMember(dest => dest.Vouchers, opt => opt.MapFrom(src => src.Vouchers!.Where(v => !v.IsDeleted)))
            .ForMember(dest => dest.Bookings, opt => opt.MapFrom(src => src.Bookings!.Where(b => !b.IsDeleted)));

        CreateMap<User, ResponseUserVM>();

        CreateMap<User, ResponseUserInformationVM>()
            .ForMember(dest => dest.Customers, opt => opt.MapFrom(src => src.Customers!.Where(c => !c.IsDeleted)))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles!.Where(r => !r.IsDeleted)));

        CreateMap<ExpiredDateExtension, ResponseExpiredDateExtensionVM>()
            .ForMember(dest => dest.SalesEmployee, opt => opt.MapFrom(src => src.SalesEmployee!.IsDeleted ? null : src.SalesEmployee))
            .ForMember(dest => dest.Voucher, opt => opt.MapFrom(src => src.Voucher!.IsDeleted ? null : src.Voucher));

        CreateMap<Role, ResponseRoleVM>()
            .ForMember(dest => dest.RoleClaims, opt => opt.MapFrom(src => src.RoleClaims));

        CreateMap<Role, ResponseRoleForUserVM>();

        CreateMap<RoleClaim, ResponseRoleClaimVM>();

        //This mapping is used for Booking entity only
        CreateMap<Voucher, ResponseVoucherForBookingVM>()
            .ForMember(dest => dest.VoucherStatus, opt => opt.MapFrom(src => Enum.GetName(typeof(VoucherStatus), src.VoucherStatus)));

        CreateMap<VoucherType, ResponseVoucherTypeForBookingVM>();
        //=================================


        //This mapping is used for Expired Date Extension entity only
        CreateMap<Voucher, ResponseVoucherForExtensionVM>()
            .ForMember(dest => dest.VoucherStatus, opt => opt.MapFrom(src => Enum.GetName(typeof(VoucherStatus), src.VoucherStatus)));

        CreateMap<VoucherType, ResponseVoucherTypeForExtensionVM>();
        //==================================
    }
}
