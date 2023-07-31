using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IServicePackageService
{
    Task<ServiceResponse> UpdateVoucherTypesOnServicePackage(int servicePackageId, IEnumerable<UpdatedVoucherTypesInPackageVM> model);
    Task<ServicePackage?> AddNewServicePackage(ServicePackage servicePackage, IEnumerable<int> servicesIds);
    IEnumerable<ServicePackage> GetALlServicePackage();
    IEnumerable<ServicePackage> GetAllDeletedServicePackage();
    ServicePackage? GetServicePackageById(int id);
    Task<ServiceResponse> UpdateInformation(int servicePackageId, UpdatedServicePackageVM updatedServicePackage);
    Task<ServiceResponse> UpdateServiceOfServicePackage(IEnumerable<UpdatedServicesInPackageVM> updatedServicesInPackage, int servicePackageId);
    Task<ServiceResponse> SoftDelete(int id);
    Task<ServiceResponse> SoftDeleteBatch(IEnumerable<int> ids);
}
