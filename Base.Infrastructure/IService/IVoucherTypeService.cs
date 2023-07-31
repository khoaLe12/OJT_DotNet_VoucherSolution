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

public interface IVoucherTypeService
{
    Task<VoucherType?> AddNewVoucherType(VoucherType voucherType, IEnumerable<int>? ServicePackageIds);
    IEnumerable<VoucherType> GetAllVoucherTypes();
    IEnumerable<VoucherType> GetAllDeletedVoucherTypes();
    VoucherType? GetVoucherTypeById(int id);
    Task<ServiceResponse> PatchUpdate(int voucherTypeId, JsonPatchDocument<VoucherType> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> UpdateVoucherType(UpdatedVoucherTypeVM updatedVoucherType, int voucherTypeId);
    Task<ServiceResponse> SoftDelete(int id);
}
