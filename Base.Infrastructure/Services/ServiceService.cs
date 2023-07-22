using Base.Core.Common;
using Base.Core.Entity;
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

    public async Task<Service?> GetServiceById(int id)
    {
        Expression<Func<Service, bool>> where = s => !s.IsDeleted && s.Id == id;
        Expression<Func<Service, object>>[] includes = {
            s => s.ServicePackages!
        };
        return await _unitOfWork.Services.Get(where, includes).FirstOrDefaultAsync();
    }

    public async Task<ServiceResponse> UpdateInformation(Service updatedService, int serviceId)
    {
        var existedService = await _unitOfWork.Services.Get(s => s.Id == serviceId).AsNoTracking().FirstOrDefaultAsync();
        var checkService = await _unitOfWork.Services.Get(s => s.ServiceName == updatedService.ServiceName).AsNoTracking().FirstOrDefaultAsync();

        if (existedService == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy dịch vụ",
                Error = new List<string>() { "Can not find service with the given id: " + serviceId }
            };
        }
        
        if(checkService != null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = $"Dịch vụ '{updatedService.ServiceName}' đã tồn tại",
                Error = new List<string>() { $"Service '{updatedService.ServiceName}' already exist" }
            };
        }

        updatedService.IsDeleted = existedService.IsDeleted;
        updatedService.Id = serviceId;

        _unitOfWork.Services.Update(updatedService);

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
        var existedService = await _unitOfWork.Services.Get(s => !s.IsDeleted && s.ServiceName == service.ServiceName).FirstOrDefaultAsync();
        if(existedService != null)
        {
            throw new ArgumentException($"Dịch vụ '{existedService.ServiceName}' đã tồn tại");
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

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(Service),
            PrimaryKey = id.ToString()
        };

        await _unitOfWork.AuditLogs.AddAsync(log);

        if (await _unitOfWork.SaveChangesAsync())
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
