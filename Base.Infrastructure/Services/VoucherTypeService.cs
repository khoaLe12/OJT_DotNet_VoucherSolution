using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class VoucherTypeService : IVoucherTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public VoucherTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> PatchUpdate(int voucherTypeId, JsonPatchDocument<VoucherType> patchDoc, ModelStateDictionary ModelState)
    {
        var operations = patchDoc.Operations.Where(o => o.op != "replace" || o.path != "IsAvailable");
        if (!operations.IsNullOrEmpty())
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không được hỗ trợ",
                Error = new List<string>() { "This action only support uppdate on IsAvailable of VoucherType" }
            };
        }

        var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(voucherTypeId);
        if (existedVoucherType == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy",
                Error = new List<string>() { "Can not find voucher type with the given id: " + voucherTypeId }
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

        patchDoc.ApplyTo(existedVoucherType, errorHandler);
        if (!ModelState.IsValid)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ModelState.ToString() ?? $"Error when updating VoucherType '{existedVoucherType.TypeName}'" }
            };
        }

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
            };
        }
    }

    public async Task<ServiceResponse> UpdateVoucherType(UpdatedVoucherTypeVM updatedVoucherType, int voucherTypeId)
    {
        var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(voucherTypeId);
        if (existedVoucherType == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy",
                Error = new List<string>() { $"Can not find voucher type with the given id '{voucherTypeId}'" }
            };
        }

        existedVoucherType.TypeName = updatedVoucherType.TypeName!;
        existedVoucherType.IsAvailable = updatedVoucherType.IsAvailable;
        existedVoucherType.CommonPrice = updatedVoucherType.CommonPrice;
        existedVoucherType.AvailableNumberOfVouchers = updatedVoucherType.AvailableNumberOfVouchers;
        existedVoucherType.PercentageDiscount = updatedVoucherType.PercentageDiscount;
        existedVoucherType.ValueDiscount = updatedVoucherType.ValueDiscount;
        existedVoucherType.MaximumValueDiscount = updatedVoucherType.MaximumValueDiscount;
        existedVoucherType.ConditionsAndPolicies = updatedVoucherType.ConditionsAndPolicies;

        if (await _unitOfWork.SaveChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Cập nhật thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Make sure using new value to update", "Maybe error from server" }
            };
        }
    }

    public async Task<VoucherType?> AddNewVoucherType(VoucherType voucherType, IEnumerable<int>? ServicePackageIds)
    {
        /*var existedVoucherType = await _unitOfWork.VoucherTypes.Get(vt => !vt.IsDeleted && vt.TypeName == voucherType.TypeName).FirstOrDefaultAsync();
        if(existedVoucherType != null)
        {
            throw new ArgumentException($"Loại voucher '{existedVoucherType.TypeName}' đã tồn tại");
        }*/

        if (ServicePackageIds != null)
        {
            var servicePackageList = new List<ServicePackage>();
            foreach (int id in ServicePackageIds)
            {
                var servicePackage = await _unitOfWork.ServicePackages.FindAsync(id);
                if (servicePackage == null)
                {
                    throw new ArgumentNullException(null, "Không tìm thấy gói dịch vụ:" + id);
                }
                servicePackageList.Add(servicePackage);
            }
            voucherType.UsableServicePackages = servicePackageList;
        }

        await _unitOfWork.VoucherTypes.AddAsync(voucherType);
        if (await _unitOfWork.SaveChangesAsync())
        {
            return voucherType;
        }
        return null;
    }

    public IEnumerable<VoucherType> GetAllVoucherTypes()
    {
        return _unitOfWork.VoucherTypes.FindAll().Where(vt => !vt.IsDeleted);
    }

    public IEnumerable<VoucherType> GetAllDeletedVoucherTypes()
    {
        return _unitOfWork.VoucherTypes.FindAll().Where(vt => vt.IsDeleted);
    }

    public VoucherType? GetVoucherTypeById(int id)
    {
        Expression<Func<VoucherType, bool>> where = vt => !vt.IsDeleted && vt.Id == id;
        Expression<Func<VoucherType, object>>[] includes = {
            vt => vt.Vouchers!,
            vt => vt.UsableServicePackages!
        };
        return _unitOfWork.VoucherTypes.Get(where, includes)?.FirstOrDefault();
    }

    public async Task<ServiceResponse> SoftDelete(int id)
    {
        var existedVoucherType = await _unitOfWork.VoucherTypes.Get(sp => sp.Id == id && !sp.IsDeleted).FirstOrDefaultAsync();
        if (existedVoucherType == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy loại voucher",
                Error = new List<string>() { "Can not find voucher type with the given id: " + id }
            };
        }

        existedVoucherType.IsDeleted = true;

        if (await _unitOfWork.SaveDeletedChangesAsync())
        {
            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Xóa thành công"
            };
        }
        else
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Xóa thất bại",
                Error = new List<string>() { "Maybe nothing has been changed", "Maybe error from server" }
            };
        }
    }
}
