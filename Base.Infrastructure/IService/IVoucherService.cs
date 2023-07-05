using Base.Core.Entity;
using Base.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IVoucherService
{
    Task<Voucher?> AddNewVoucher(Voucher? voucher, Guid? CustomerId, int? VoucherTypeId);
    IEnumerable<Voucher>? GetAllVoucher();
    Task<Voucher?> GetVoucherById(int id);
}
