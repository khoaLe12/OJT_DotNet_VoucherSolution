using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Service> GetAllService()
    {
        return _unitOfWork.Services.FindAll().Where(s => !s.IsDeleted);
    }

    public IEnumerable<Service> GetAllDeletedService()
    {
        return _unitOfWork.Services.FindAll().Where(s => s.IsDeleted);
    }

    public async Task<Service?> GetServiceById(int id)
    {
        Expression<Func<Service, bool>> where = s => !s.IsDeleted && s.Id == id;
        Expression<Func<Service, object>>[] includes = {
            s => s.ServicePackages!
        };
        return await _unitOfWork.Services.Get(where, includes).FirstOrDefaultAsync();
    }

    public async Task<ServiceResponse> UpdateInformation(ServiceVM updatedService, int serviceId)
    {
        var existedService = await _unitOfWork.Services.Get(s => s.Id == serviceId).FirstOrDefaultAsync();
        if (existedService == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy dịch vụ",
                Error = new List<string>() { "Can not find service with the given id: " + serviceId }
            };
        }

        if(existedService.ServiceName != updatedService.ServiceName)
        {
            var checkService = await _unitOfWork.Services.Get(s => s.ServiceName == updatedService.ServiceName).AsNoTracking().FirstOrDefaultAsync();
            if (checkService != null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = $"Dịch vụ '{updatedService.ServiceName}' đã tồn tại",
                    Error = new List<string>() { $"Service '{updatedService.ServiceName}' already exist" }
                };
            }
        }

        existedService.ServiceName = updatedService.ServiceName!;
        existedService.Description = updatedService.Description;

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

    public async Task<Service?> AddNewService(Service service)
    {
        var existedService = await _unitOfWork.Services.Get(s => s.ServiceName == service.ServiceName).FirstOrDefaultAsync();
        if(existedService != null)
        {
            if(existedService.IsDeleted == true)
            {
                throw new CustomException($"Dịch vụ '{existedService.ServiceName}' đã tồn tại")
                {
                    Errors = new List<string>() { $"Service '{existedService.ServiceName}' already exists but has been deleted, you need to restore it" },
                    IsRestored = true
                };
            }

            throw new CustomException($"Dịch vụ '{existedService.ServiceName}' đã tồn tại")
            {
                Errors = new List<string>() { $"Service '{existedService.ServiceName}' has already existed" }
            };
        }

        await _unitOfWork.Services.AddAsync(service);
        if (await _unitOfWork.SaveChangesAsync())
        {
            return service;
        }
        return null;
    }

    public async Task<ServiceResponse> SoftDelete(int id)
    {
        var existedService = await _unitOfWork.Services.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
        if (existedService == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy dịch vụ",
                Error = new List<string>() { "Can not find service with the given id: " + id }
            };
        }

        existedService.IsDeleted = true;

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

    public async Task<ServiceResponse> RestoreService(int id)
    {
        var deletedService = await _unitOfWork.Services.Get(s => s.Id == id && s.IsDeleted).FirstOrDefaultAsync();
        if (deletedService is null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy service đã xóa",
                Error = new List<string>() { "Can not find deleted service with the given id: " + id }
            };
        }
        deletedService.IsDeleted = false;

        var log = await _unitOfWork.AuditLogs.Get(l => l.PrimaryKey == id.ToString() && l.Type == 3 && l.IsRestored != true && l.TableName == nameof(Service)).FirstOrDefaultAsync();
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
