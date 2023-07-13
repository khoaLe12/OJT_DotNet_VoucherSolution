using Base.Core.Common;
using Base.Core.Entity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IExpiredDateExtensionService
{
    Task<ExpiredDateExtension?> AddNewExpiredDateExtension(ExpiredDateExtension? expiredDateExtension, int? VoucherId);
    IEnumerable<ExpiredDateExtension>? GetAllExpiredDateExtensions();
    Task<ExpiredDateExtension?> GetExpiredDateExtensionById(int id);
    //Task<ServiceResponse> PatchUpdate(int id, JsonPatchDocument<ExpiredDateExtension> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> UpdateVoucherExtension(ExpiredDateExtension? updatedExpiredDateExtension, int id);
}
