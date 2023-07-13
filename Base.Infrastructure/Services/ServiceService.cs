using AutoMapper.Configuration.Annotations;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Services;

internal class ServiceService : IServiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> PatchUpdate(int serviceId, JsonPatchDocument<Service> patchDoc, ModelStateDictionary ModelState)
    {
        var existedService = await _unitOfWork.Services.FindAsync(serviceId);
        if (existedService == null)
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

        patchDoc.ApplyTo(existedService, errorHandler);
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

    public async Task<ServiceResponse> UpdateService(Service? updatedService, int serviceId)
    {
        if (updatedService == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Invalid: Update Information are null"
            };
        }

        var existedService = await _unitOfWork.Services.FindAsync(serviceId);
        if (existedService == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Can not found"
            };
        }
        else
        {
            updatedService.Id = serviceId;
            _unitOfWork.Services.Update(updatedService);

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

    public async Task<Service?> AddNewService(Service? service)
    {
        if (service != null)
        {
            await _unitOfWork.Services.AddAsync(service);
            if(await _unitOfWork.SaveChangesAsync())
            {
                return service;
            }
        }
        return null;
    }

    public IEnumerable<Service>? GetAllService()
    {
        return _unitOfWork.Services.FindAll();
    }

    public async Task<Service?> GetServiceById(int id)
    {
        Expression<Func<Service, bool>> where = s => s.Id == id;
        Expression<Func<Service, object>>[] includes = {
            s => s.ServicePackages!
        };
        return await _unitOfWork.Services.Get(where, includes).Include(s => s.ServicePackages).FirstOrDefaultAsync();
    }
}
