using AutoMapper.Configuration.Annotations;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
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
