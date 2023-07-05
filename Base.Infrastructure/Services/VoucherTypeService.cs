using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Services;

internal class VoucherTypeService : IVoucherTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public VoucherTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VoucherType?> AddNewVoucherType(VoucherType? voucherType, IEnumerable<int>? ServicePackageIds)
    {
        if (voucherType != null)
        {
            if(ServicePackageIds != null)
            {
                var servicePackageList = new List<ServicePackage>();
                foreach (int id in ServicePackageIds)
                {
                    var servicePackage = await _unitOfWork.ServicePackages.FindAsync(id);
                    if (servicePackage != null)
                    {
                        servicePackageList.Add(servicePackage);
                    }
                    else
                    {
                        //Lets log an error that can not find servicePackage with the given id
                        //Or throw a Null Argument Exception
                        return null;
                    }
                }
                voucherType.UsableServicePackages = servicePackageList;
            }
            
            await _unitOfWork.VoucherTypes.AddAsync(voucherType);
            if(await _unitOfWork.SaveChangesAsync())
            {
                return voucherType;
            }
        }
        return null;
    }

    public IEnumerable<VoucherType>? GetAllVoucherTypes()
    {
        return _unitOfWork.VoucherTypes.FindAll();
    }

    public VoucherType? GetVoucherTypeById(int id)
    {
        Expression<Func<VoucherType, bool>> where = vt => vt.Id == id;
        Expression<Func<VoucherType, object>>[] includes = {
            vt => vt.Vouchers ?? new List<Voucher>(),
            vt => vt.UsableServicePackages ?? new List<ServicePackage>()
        };
        return _unitOfWork.VoucherTypes.Get(where, includes)?.FirstOrDefault();
    }
}
