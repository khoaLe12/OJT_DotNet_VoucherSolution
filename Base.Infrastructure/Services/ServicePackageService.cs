using AutoMapper.Configuration;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;


namespace Base.Infrastructure.Services;

internal class ServicePackageService : IServicePackageService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServicePackageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> ApplyVoucherType(int servicePackageId, IEnumerable<int> voucherTypeIds)
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

        var exceptions = new ConcurrentQueue<Exception>();
        var voucherTypeList = new ConcurrentBag<VoucherType>();
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2))
        };
        Parallel.ForEach(voucherTypeIds, options, (id, state) =>
        {
            var existedVoucherType = _unitOfWork.VoucherTypes.Get(vt => vt.Id == id).FirstOrDefault();
            if(existedVoucherType == null)
            {
                exceptions.Enqueue(new ArgumentNullException(null, $"Không tìm thấy loại voucher: {id}"));
                state.Stop();
                return;
            }
            voucherTypeList.Add(existedVoucherType);
        });

        if (!exceptions.IsNullOrEmpty())
        {
            var message = exceptions.First().Message;
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = message.Split(":").First(),
                Error = new List<string>() { "Voucher Type not found with the given id:" + message.Split(":").Last() }
            };
        }

        if (voucherTypeList.IsNullOrEmpty())
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Danh sách loại voucher trống",
                Error = new List<string>() { "Voucher list is null or empty" }
            };
        }

        if (existedServicePackage.ValuableVoucherTypes.IsNullOrEmpty())
        {
            existedServicePackage.ValuableVoucherTypes = voucherTypeList.ToList();
        }
        else
        {
            existedServicePackage.ValuableVoucherTypes!.Concat(voucherTypeList).ToList();
        }

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

    public async Task<ServiceResponse> UpdateInformation(int servicePackageId, ServicePackage updatedServicePackage)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => sp.Id == servicePackageId).AsNoTracking().FirstOrDefaultAsync();
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

        updatedServicePackage.IsDeleted = existedServicePackage.IsDeleted;
        updatedServicePackage.Id = servicePackageId;

        _unitOfWork.ServicePackages.Update(updatedServicePackage);

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
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(s => s.Id == servicePackageId, sp => sp.Services).FirstOrDefaultAsync();

        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy gói dịch vụ",
                Error = new List<string>() { $"Service Package not found with id '{servicePackageId}'" }
            };
        }

        var services = existedServicePackage.Services.ToList();

        foreach(var item in model)
        {
            var service = await _unitOfWork.Services.FindAsync(item.Id);
            if (service == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy dịch vụ",
                    Error = new List<string>() { $"Service '{item.Id}' not found" }
                };
            }

            if (item.IsDelete)
            {
                if (!services.Remove(service))
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = $"Không thể xóa dịch vụ '{service.ServiceName}'",
                        Error = new List<string>() { $"Service '{service.ServiceName}' is not in Service Package '{existedServicePackage.ServicePackageName}'" }
                    };
                }
            }
            else
            {
                services.Add(service);
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
        var existedServicePackage = await _unitOfWork.ServicePackages.Get(sp => !sp.IsDeleted && sp.ServicePackageName == servicePackage.ServicePackageName).AsNoTracking().FirstOrDefaultAsync();
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
            sp => sp.Services,
            sp => sp.ValuableVoucherTypes!
        };
        return _unitOfWork.ServicePackages.Get(s => !s.IsDeleted, includes);
    }

    public ServicePackage? GetServicePackageById(int id)
    {
        Expression<Func<ServicePackage, bool>> where = sp => !sp.IsDeleted && sp.Id == id;
        Expression<Func<ServicePackage, object>>[] includes = {
            sp => sp.Services,
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

        var log = new Log
        {
            Type = (int)AuditType.Delete,
            TableName = nameof(ServicePackage),
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
