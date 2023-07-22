using AutoMapper;
using Base.Core.Entity;
using Base.Core.ViewModel;

namespace Base.API.Mapper.ModelToResource
{
    public class ItemMapper1 : Profile
    {
        public ItemMapper1()
        {
            CreateMap<BookingVM, Booking>();

            CreateMap<UpdatedBookingVM, Booking>();

            CreateMap<ServiceVM, Service>();

            CreateMap<ServicePackageVM, ServicePackage>();

            CreateMap<UpdatedServicePackageVM, ServicePackage>();

            CreateMap<VoucherTypeVM, VoucherType>()
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsActiveNow));

            CreateMap<UpdatedVoucherTypeVM, VoucherType>();

            CreateMap<VoucherVM, Voucher>()
                .ForMember(dest => dest.ActualPrice, opt => opt.MapFrom(src => src.ActualPurchasePrice));

            CreateMap<ExpiredDateExtensionVM, ExpiredDateExtension>();

            CreateMap<UpdatedExpiredDateExtensionVM, ExpiredDateExtension>();
        }
    }
}
