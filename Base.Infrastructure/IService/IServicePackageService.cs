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
    Task<ServicePackage?> AddNewServicePackage(ServicePackage? servicePackage, IEnumerable<int>? servicesIds);
    IEnumerable<ServicePackage>? GetALlServicePackage();
    ServicePackage? GetServicePackageById(int id);
    Task<ServiceResponse> PatchUpdate(int servicePackageId, JsonPatchDocument<ServicePackage> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> UpdateServicePackage(ServicePackage? updatedServicePackage, int servicePackageId);
}
