using Base.Core.Application;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Base.Infrastructure.Services;

internal class ExpiredDateExtensionService : IExpiredDateExtensionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ExpiredDateExtensionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResponse> UpdateVoucherExtension(UpdatedExpiredDateExtensionVM updatedExpiredDateExtension, int id)
    {
        var existedExpiredDateExtension = await _unitOfWork.ExpiredDateExtensions.FindAsync(id);
        if (existedExpiredDateExtension == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy đơn gia hạn voucher",
                Error = new List<string>() { "Can not find voucher extension with the given id: " + id }
            };
        }

        var voucher = await _unitOfWork.Vouchers.FindAsync(existedExpiredDateExtension.VoucherId);
        if (voucher == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy voucher"
            };
        }

        if (existedExpiredDateExtension.OldExpiredDate > updatedExpiredDateExtension.NewExpiredDate)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Ngày hết hạn mới phải lớn hơn ngày hết hạn cũ",
                Error = new List<string>() 
                {
                    "New expired date is less then the old one", 
                    $"Old expired date: {existedExpiredDateExtension.OldExpiredDate}",
                    $"New expired date: {updatedExpiredDateExtension.NewExpiredDate}"
                }
            };
        }

        if (voucher.ExpiredDate == existedExpiredDateExtension.NewExpiredDate)
        {
            voucher.ExpiredDate = updatedExpiredDateExtension.NewExpiredDate;
        }

        existedExpiredDateExtension.Price = updatedExpiredDateExtension.Price;
        existedExpiredDateExtension.NewExpiredDate = updatedExpiredDateExtension.NewExpiredDate;

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

    public async Task<ExpiredDateExtension?> AddNewExpiredDateExtension(ExpiredDateExtension expiredDateExtension, int VoucherId)
    {
        var salesEmployee = await _unitOfWork.Users.FindAsync(_currentUserService.UserId);
        var voucher = await _unitOfWork.Vouchers.FindAsync(VoucherId);

        if(salesEmployee == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin người dùng:{_currentUserService.UserId}");
        }

        if(voucher == null)
        {
            throw new ArgumentNullException(null, $"Không tìm thấy thông tin voucher:{VoucherId}");
        }

        if (voucher.ExpiredDate > expiredDateExtension.NewExpiredDate)
        {
            throw new CustomException("Ngày hết hạn mới không hợp lệ")
            {
                Errors = new List<string>() 
                { 
                    "New expired date is less then the old one",
                    $"New expired date: {expiredDateExtension.NewExpiredDate}", 
                    $"Old expired date: {voucher.ExpiredDate}"
                }
            };
        }
        expiredDateExtension.OldExpiredDate = voucher.ExpiredDate;
        expiredDateExtension.DateTime = DateTime.Now;
        expiredDateExtension.SalesEmployee = salesEmployee;
        expiredDateExtension.Voucher = voucher;

        //Update new expired date of the Voucher
        voucher.ExpiredDate = expiredDateExtension.NewExpiredDate;

        await _unitOfWork.ExpiredDateExtensions.AddAsync(expiredDateExtension);
        if (await _unitOfWork.SaveChangesAsync())
        {
            return expiredDateExtension;
        }

        return null;
    }

    public IEnumerable<ExpiredDateExtension> GetAllExpiredDateExtensions()
    {
        Expression<Func<ExpiredDateExtension, object>>[] includes = {
            e => e.Voucher!
        };
        return _unitOfWork.ExpiredDateExtensions.Get(e => !e.IsDeleted, includes).AsNoTracking();
    }

    public IEnumerable<ExpiredDateExtension> GetAllDeletedExpiredDateExtensions()
    {
        return _unitOfWork.ExpiredDateExtensions.Get(e => e.IsDeleted).AsNoTracking();
    }

    public async Task<ExpiredDateExtension?> GetExpiredDateExtensionById(int id)
    {
        Expression<Func<ExpiredDateExtension, bool>> where = e => !e.IsDeleted && e.Id == id;
        Expression<Func<ExpiredDateExtension, object>>[] includes = {
            e => e.SalesEmployee!,
            e => e.Voucher!
        };
        return await _unitOfWork.ExpiredDateExtensions.Get(where, includes)
            .Include(nameof(ExpiredDateExtension.Voucher) + "." + nameof(Voucher.Customer))
            .Include(nameof(ExpiredDateExtension.Voucher) + "." + nameof(Voucher.VoucherType))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ExpiredDateExtension>> GetAllExpiredDateExtensionOfUser()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentNullException(null, $"User Not Found with the given id: {userId}");
        }
        return await _unitOfWork.ExpiredDateExtensions.Get(e => !e.IsDeleted && e.SalesEmployeeId == userId, e => e.Voucher!).AsNoTracking().ToArrayAsync();
    }

    public async Task<IEnumerable<ExpiredDateExtension>> GetAllExpiredDateExtensionOfCustomer()
    {
        var userId = _currentUserService.UserId;
        var user = await _unitOfWork.Customers.Get(u => u.Id == userId, u => u.Vouchers!).AsNoTracking().FirstOrDefaultAsync();
        if (user == null)
        {
            throw new ArgumentNullException(null, $"Customer Not Found with the given id: {userId}");
        }
        if(user.Vouchers == null)
        {
            return Enumerable.Empty<ExpiredDateExtension>();
        }

        Expression<Func<ExpiredDateExtension, object>>[] includes =
        {
            e => e.Voucher!,
            e => e.SalesEmployee!
        };

        return await _unitOfWork.ExpiredDateExtensions.Get(e => !e.IsDeleted && user.Vouchers.Contains(e.Voucher), includes).AsNoTracking().ToArrayAsync();
    }

    public async Task<ServiceResponse> SoftDelete(int id)
    {
        var existedVoucherExtension = await _unitOfWork.ExpiredDateExtensions.Get(e => e.Id == id && !e.IsDeleted, e => e.Voucher!).FirstOrDefaultAsync();
        if (existedVoucherExtension == null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy đơn gia hạn voucher",
                Error = new List<string>() { "Can not find voucher extension with the given id: " + id }
            };
        }

        var voucher = existedVoucherExtension.Voucher;
        if(voucher is null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy voucher",
                Error = new List<string>() { "Can not find voucher" }
            };
        }

        existedVoucherExtension.IsDeleted = true;
        
        if(voucher.ExpiredDate == existedVoucherExtension.NewExpiredDate)
        {
            voucher.ExpiredDate = existedVoucherExtension.OldExpiredDate;
            _unitOfWork.Vouchers.Update(voucher);
        }

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
