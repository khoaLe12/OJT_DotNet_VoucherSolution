using Base.Core.Common;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Hangfire;

namespace Base.API.Services;

public interface IBackgroundTaskService
{
    Task<ServiceResponse> ScheduleVoucherSell(SchedulesVoucherTypeVM model);
}

internal class BackgroundTaskService : IBackgroundTaskService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IUnitOfWork _unitOfWork;

    public BackgroundTaskService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IUnitOfWork unitOfWork)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse> ScheduleVoucherSell(SchedulesVoucherTypeVM model)
    {
        if(model.From >= model.To)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Thời gian không hợp lệ",
                Error = new List<string>() { "Datetime from must less than to" }
            };
        }

        var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(model.Id);
        if(existedVoucherType is null)
        {
            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy loại voucher",
                Error = new List<string>() { "Can not find voucher type with the given id: " + model.Id }
            };
        }

        if(model.From is null)
        {
            _backgroundJobClient.Enqueue(() => UpdateIsAvailableOfVoucherType(model.Id, true));
        }
        else
        {
            _backgroundJobClient.Schedule(() => UpdateIsAvailableOfVoucherType(model.Id, true), (TimeSpan)(model.From - DateTime.UtcNow));
        }
        _backgroundJobClient.Schedule(() => UpdateIsAvailableOfVoucherType(model.Id, false), model.To - DateTime.UtcNow);

        return new ServiceResponse
        {
            IsSuccess = true,
            Message = "Cập nhật thành công"
        };
    }

    public async Task UpdateIsAvailableOfVoucherType(int id, bool isAvailable)
    {
        var existedVoucherType = await _unitOfWork.VoucherTypes.FindAsync(id);
        if(existedVoucherType is not null)
        {
            existedVoucherType.IsAvailable = isAvailable;
            await _unitOfWork.SaveChangesNoLogAsync();
        }
    }
}
