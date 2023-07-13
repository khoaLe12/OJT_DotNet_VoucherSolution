using Base.Core.Common;
using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    public async Task<ServiceResponse> PatchUpdate(int servicePackageId, JsonPatchDocument<ServicePackage> patchDoc, ModelStateDictionary ModelState)
    {
        var existedServicePackage = await _unitOfWork.ServicePackages.FindAsync(servicePackageId);
        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found"
            };
        }

        Action<JsonPatchError> errorHandler = (error) =>
        {
            var operation = patchDoc.Operations.FirstOrDefault(op => op.path == error.AffectedObject.ToString());
            if (operation != null)
            {
                var propertyName = operation.path.Split('/').Last();
                ModelState.AddModelError(propertyName, error.ErrorMessage);
            }
            else
            {
                ModelState.AddModelError("", error.ErrorMessage);
            }
        };

        patchDoc.ApplyTo(existedServicePackage, errorHandler);
        if (!ModelState.IsValid)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = ModelState.ToString(),
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Update Successfully"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Update Fail"
            };
        }
    }

    public async Task<ServiceResponse> UpdateServicePackage(ServicePackage? updatedServicePackage, int servicePackageId)
    {
        if (updatedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid: Update Information are null"
            };
        }

        var existedServicePackage = await _unitOfWork.ServicePackages.FindAsync(servicePackageId);
        if (existedServicePackage == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found"
            };
        }
        else
        {
            updatedServicePackage.Id = servicePackageId;
            _unitOfWork.ServicePackages.Update(updatedServicePackage);

            if (await _unitOfWork.SaveChangesAsync())
            {
                return new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "Update successfully"
                };
            }
            else
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Fail"
                };
            }
        }
    }

    public async Task<ServicePackage?> AddNewServicePackage(ServicePackage? servicePackage, IEnumerable<int>? servicesIds)
    {
        if(servicePackage != null && servicesIds != null)
        {
            var serviceList = new List<Service>();
            foreach (int id in servicesIds)
            {
                var service = await _unitOfWork.Services.FindAsync(id);
                if(service != null)
                {
                    serviceList.Add(service);
                }
                else
                {
                    return null;
                }
            }
            servicePackage.Services = serviceList;
            await _unitOfWork.ServicePackages.AddAsync(servicePackage);
            if(await _unitOfWork.SaveChangesAsync())
            {
                return servicePackage;
            }
        }
        return null;
    }

    public IEnumerable<ServicePackage>? GetALlServicePackage()
    {
        return _unitOfWork.ServicePackages.FindAll()?.Include(sp => sp.Services);
    }

    public ServicePackage? GetServicePackageById(int id)
    {
        Expression<Func<ServicePackage, bool>> where = sp => sp.Id == id;
        Expression<Func<ServicePackage, object>>[] includes = {
            sp => sp.Services,
            sp => sp.ValuableVoucherTypes!
        };
        return _unitOfWork.ServicePackages.Get(where, includes)?.FirstOrDefault();
    }
}
