using Base.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.IService;

public interface IStatisticalService
{
    Task<IEnumerable<ResponseConsumptionStatisticVM>> GetConsumptionStatisticOfCustomer(Guid customerId, DateTime from, DateTime to);
    Task<IEnumerable<ResponseServicePackageStatisticByNumberOfBookingsVM>> GetServicePackageStatistics(DateTime From, DateTime To);
    Task<IEnumerable<ResponseServicePackageStatisticByTotalSpendingVM>> GetServicePackageStatisticsWithTotalSpending(DateTime From, DateTime To);
}
