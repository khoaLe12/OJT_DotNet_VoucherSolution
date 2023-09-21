using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
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

    public async Task<ServiceResponse> UpdateVoucherTypesOnServicePackage(int servicePackageId, IEnumerable<int> model)
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

        var voucherTypes = await _unitOfWork.VoucherTypes.Get(vt => model.Contains(vt.Id)).ToListAsync();

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
        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { "Can not find service package with the given id: " + servicePackageId }
            };
        }

        if(existedServicePackage.ServicePackageName != updatedServicePackage.ServicePackageName)
        {
            var checkServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.ServicePackageName == updatedServicePackage.ServicePackageName).AsNoTracking().FirstOrDefaultAsync();
            if (checkServicePackage != null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Gói dịch vụ '{updatedServicePackage.ServicePackageName}' đã tồn tại",
                    Error = new List<string>() { $"Service package '{updatedServicePackage.ServicePackageName}' already exist" }
                };
            }
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

    public async Task<ServiceResponse> UpdateServiceOfServicePackage(IEnumerable<int> serviceIds, int servicePackageId)
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

        var services = await _unitOfWork.Services.Get(b => serviceIds.Contains(b.Id)).ToListAsync();
        if (services.IsNullOrEmpty())
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy danh sách dịch vụ",
                Error = new List<string>() { "List of service is null or empty" }
            };
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
            if(existedServicePackage.IsDeleted == true)
            {
                throw new CustomException("Gói dịch vụ đã tồn tại")
                {
                    Errors = new List<string>() { $"Service package '{servicePackage.ServicePackageName}' already exists but has been deleted, you need to restored it" },
                    IsRestored = true
                };
            }

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

    public async Task<ServiceResponse> RestoreServicePackage(int id)
    {
        var deletedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == id && sp.IsDeleted).FirstOrDefaultAsync();
        if (deletedServicePackage is null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ đã xóa",
                Error = new List<string>() { "Can not find deleted service package with the given id: " + id }
            };
        }
        deletedServicePackage.IsDeleted = false;

        var log = await _unitOfWork.AuditLogs.Get(l => l.PrimaryKey == id.ToString() && l.Type == 3 && l.IsRestored != true && l.TableName == nameof(ServicePackage)).FirstOrDefaultAsync();
        if (log is not null)
        {
            log.IsRestored = true;
        }

        if (await _unitOfWork.SaveChangesNoLogAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Khôi phục thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Khôi phục thất bại",
                Error = new List<string>() { "Maybe there is error from server", "Maybe there is no change made" }
            };
        }
    }
}
