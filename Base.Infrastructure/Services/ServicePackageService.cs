using Base.Core.Entity;
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
