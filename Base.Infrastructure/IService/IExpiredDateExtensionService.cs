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

public interface IExpiredDateExtensionService
{
    Task<ExpiredDateExtension?> AddNewExpiredDateExtension(ExpiredDateExtension expiredDateExtension, int VoucherId);
    IEnumerable<ExpiredDateExtension> GetAllExpiredDateExtensions();
    Task<ExpiredDateExtension?> GetExpiredDateExtensionById(int id);
    Task<ServiceResponse> UpdateVoucherExtension(UpdatedExpiredDateExtensionVM updatedExpiredDateExtension, int id);
    Task<IEnumerable<ExpiredDateExtension>> GetAllExpiredDateExtensionOfUser();
    Task<IEnumerable<ExpiredDateExtension>> GetAllExpiredDateExtensionOfCustomer();
    Task<ServiceResponse> SoftDelete(int id);
}
