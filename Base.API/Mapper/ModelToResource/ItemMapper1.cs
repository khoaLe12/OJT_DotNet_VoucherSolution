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

            CreateMap<ServiceVM, Service>();

            CreateMap<ServicePackageVM, ServicePackage>();

            CreateMap<VoucherTypeVM, VoucherType>()
                .ForMember(dest => dest.CommonPrice, opt => opt.MapFrom(src => src.GeneralPurchasePrice))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsActiveNow));

            CreateMap<VoucherVM, Voucher>()
                .ForMember(dest => dest.ActualPrice, opt => opt.MapFrom(src => src.ActualPurchasePrice));

            CreateMap<ExpiredDateExtensionVM, ExpiredDateExtension>();
        }
    }
}
