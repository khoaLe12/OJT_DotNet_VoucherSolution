using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Base.Infrastructure.Services;

internal class ServicePackageService : IServicePackageService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServicePackageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> UpdateVoucherTypesOnServicePackage(int servicePackageId, IEnumerable<UpdatedVoucherTypesInPackageVM> model)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == servicePackageId, sp => sp.ValuableVoucherTypes!).FirstOrDefaultAsync();
        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { $"Service Package with id '{servicePackageId}' not found" }
            };
        }

        var voucherTypes = existedServicePackage.ValuableVoucherTypes!.ToList();
        foreach (var item in model)
        {
            if (item.IsDeleted)
            {
                var existedVoucherType = voucherTypes.FirstOrDefault(vt => vt.Id == item.VoucherTypeId);
                if(existedVoucherType is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy loại voucher",
                        Error = new List<string>() { $"Can not find applied voucher type with id '{item.VoucherTypeId}'" }
                    };
                }
                voucherTypes.Remove(existedVoucherType);
            }
            else
            {
                var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(item.VoucherTypeId);
                if (existedVoucherType is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy loại voucher",
                        Error = new List<string>() { $"Can not find voucher type with id '{item.VoucherTypeId}'" }
                    };
                }
                voucherTypes.Add(existedVoucherType);
            }
        }

        existedServicePackage.ValuableVoucherTypes = voucherTypes;

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe there was no changes made", "Maybe Server error" }
            };
        }
    }

    public async Task<ServiceResponse> UpdateInformation(int servicePackageId, UpdatedServicePackageVM updatedServicePackage)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == servicePackageId).FirstOrDefaultAsync();
        var checkServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.ServicePackageName == updatedServicePackage.ServicePackageName).AsNoTracking().FirstOrDefaultAsync();

        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { "Can not find service package with the given id: " + servicePackageId }
            };
        }

        if(checkServicePackage != null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = $"Gói dịch vụ '{updatedServicePackage.ServicePackageName}' đã tồn tại",
                Error = new List<string>() { $"Service package '{updatedServicePackage.ServicePackageName}' already exist" }
            };
        }

        existedServicePackage.ServicePackageName = updatedServicePackage.ServicePackageName!;
        existedServicePackage.Description = updatedServicePackage.Description;

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
            };
        }
    }

    public async Task<ServiceResponse> UpdateServiceOfServicePackage(IEnumerable<UpdatedServicesInPackageVM> model, int servicePackageId)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(s => s.Id == servicePackageId, sp => sp.Services!).FirstOrDefaultAsync();

        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { $"Service Package not found with id '{servicePackageId}'" }
            };
        }

        var services = existedServicePackage.Services!.ToList();

        foreach(var item in model)
        {
            if (item.IsDeleted)
            {
                var existedService = services.Where(s => s.Id == item.ServiceId).FirstOrDefault();
                if(existedService is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = $"Không tìm thấy dịch vụ",
                        Error = new List<string>() { $"Can not find service with id '{item.ServiceId}' in service package '{existedServicePackage.ServicePackageName}'" }
                    };
                }
                services.Remove(existedService);
            }
            else
            {
                var existedService = await _unitOfWork.Services.FindAsync(item.ServiceId);
                if (existedService == null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy dịch vụ",
                        Error = new List<string>() { "Can not find service with the given id: " + item.ServiceId }
                    };
                }
                services.Add(existedService);
            }
        }

        existedServicePackage.Services = services;

        if(await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe there was no changes made", "Maybe Server error" }
            };
        }

    }

    public async Task<ServicePackage?> AddNewServicePackage(ServicePackage servicePackage, IEnumerable<int> servicesIds)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.ServicePackageName == servicePackage.ServicePackageName).AsNoTracking().FirstOrDefaultAsync();
        if(existedServicePackage != null)
        {
            throw new CustomException($"Gói dịch vụ '{existedServicePackage.ServicePackageName}' đã tồn tại")
            {
                Errors = new List<string>() { $"Service package '{existedServicePackage.ServicePackageName}' already exist" }
            };
        }

        var serviceList = new List<Service>();
        foreach (int id in servicesIds)
        {
            var service = await _unitOfWork.Services.FindAsync(id);
            if (service == null)
            {
                throw new ArgumentNullException(null, $"Không tìm thấy dịch vụ:{id}");
            }
            serviceList.Add(service);
        }
        servicePackage.Services = serviceList;
        await _unitOfWork.ServicePackages.AddAsync(servicePackage);
        if (await _unitOfWork.SaveChangesAsync())
        {
            return servicePackage;
        }

        return null;
    }

    public IEnumerable<ServicePackage> GetALlServicePackage()
    {
        Expression<Func<ServicePackage, object>>[] includes = {
            sp => sp.Services!,
            sp => sp.ValuableVoucherTypes!
        };
        return _unitOfWork.ServicePackages.Get(s => !s.IsDeleted, includes).AsNoTracking();
    }

    public IEnumerable<ServicePackage> GetAllDeletedServicePackage()
    {
        return _unitOfWork.ServicePackages.Get(s => s.IsDeleted).AsNoTracking();
    }

    public ServicePackage? GetServicePackageById(int id)
    {
        Expression<Func<ServicePackage, bool>> where = sp => !sp.IsDeleted && sp.Id == id;
        Expression<Func<ServicePackage, object>>[] includes = {
            sp => sp.Services!,
            sp => sp.ValuableVoucherTypes!
        };
        return _unitOfWork.ServicePackages.Get(where, includes)?.FirstOrDefault();
    }

    public async Task<ServiceResponse> SoftDelete(int id)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { "Can not find service package with the given id: " + id }
            };
        }

        existedServicePackage.IsDeleted = true;

        if (await _unitOfWork.SaveDeletedChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Xóa thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Xóa thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
            };
        }
    }

    public async Task<ServiceResponse> SoftDeleteBatch(IEnumerable<int> ids)
    {
        foreach(var id in ids)
        {
            var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
            if (existedServicePackage == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy gói dịch vụ",
                    Error = new List<string>() { "Can not find service package with the given id: " + id }
                };
            }

            existedServicePackage.IsDeleted = true;
        }

        if (await _unitOfWork.SaveDeletedChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Xóa thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Xóa thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
            };
        }
    }
}
