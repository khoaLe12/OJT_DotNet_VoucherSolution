using Base.Core.Common;
using Base.Core.Entity;

namespace Base.Infrastructure.IService;

public interface ILogService
{
    Task<IEnumerable<Log>> GetUpdateActivities();
    Task<IEnumerable<Log>> GetDeleteActivities();
    Task<IEnumerable<Log>> GetCreateActivities();
    Task<Log?> GetLogById(int id);
    Task<ServiceResponse> Recover(int id);
}
