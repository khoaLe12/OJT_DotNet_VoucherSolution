using Base.Core.Entity;
using Base.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IServicePackageService
{
    Task<ServicePackage?> AddNewServicePackage(ServicePackage? servicePackage, IEnumerable<int>? servicesIds);
    IEnumerable<ServicePackage>? GetALlServicePackage();
    ServicePackage? GetServicePackageById(int id);
}
