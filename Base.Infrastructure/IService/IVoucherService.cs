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

public interface IVoucherService
{
    Task<Voucher?> AddNewVoucher(Voucher voucher, Guid CustomerId, int VoucherTypeId);
    IEnumerable<Voucher> GetAllVoucher();
    IEnumerable<Voucher> GetAllDeletedVoucher();
    Task<IEnumerable<Voucher>> GetAllVoucherOfUser();
    Task<IEnumerable<Voucher>> GetAllVoucherOfUserById(Guid userId);
    Task<IEnumerable<Voucher>> GetAllVoucherOfCustomer();
    Task<IEnumerable<Voucher>> GetAllVoucherOfCustomerById(Guid customerId);
    Task<Voucher?> GetVoucherById(int id);
    Task<ServiceResponse> PatchUpdate(int voucherId, JsonPatchDocument<Voucher> patchDoc, ModelStateDictionary ModelState);
    Task<ServiceResponse> UpdateVoucher(UpdatedVoucherVM updatedVoucher, int voucherId);
    Task<ServiceResponse> SoftDelete(int id);
    Task<ServiceResponse> RestoreVoucher(int id);
}
