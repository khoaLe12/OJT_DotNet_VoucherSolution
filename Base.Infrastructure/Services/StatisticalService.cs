using Base.Core.Common;
using Base.Core.ViewModel;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Base.Infrastructure.Services;

internal class StatisticalService : IStatisticalService
{
    private readonly IUnitOfWork _unitOfWork;

	public StatisticalService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<IEnumerable<ResponseConsumptionStatisticVM>> GetConsumptionStatisticOfCustomer(Guid customerId, DateTime from, DateTime to)
	{
		var existedUser = await _unitOfWork.Customers.FindAsync(customerId);
		if(existedUser is null)
		{
			throw new CustomException("Không tìm thấy khách hàng")
			{
				Errors = new List<string>() { "Can not find customer with the given id: " + customerId }
			};
        }

		var bookings = await _unitOfWork.Bookings.Get(b => !b.IsDeleted && b.BookingStatus == 2 && b.CustomerId == customerId && b.BookingDate >= from && b.BookingDate <= to).AsNoTracking().ToArrayAsync();
		var result = new List<ResponseConsumptionStatisticVM>();
		var months = new List<int>() { 1,2,3,4,5,6,7,8,9,10,11,12 };
		for(int i = from.Year; i <= to.Year; i++)
		{
            var yearlyStatistic = new ResponseConsumptionStatisticVM();
            ConcurrentDictionary<int, Decimal> keyValuePairs = new ConcurrentDictionary<int, decimal>();
			yearlyStatistic.Year = i;

			months.AsParallel()
				.WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2)))
				.ForAll(month =>
				{
					var monthlyConsumption = bookings.Where(b => b.BookingDate.Year == i && b.BookingDate.Month == month).Sum(b => b.TotalPrice);
					keyValuePairs.TryAdd(month, monthlyConsumption);
                });

            yearlyStatistic.MonthlyStatistic = keyValuePairs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            result.Add(yearlyStatistic);
        }
		return result;
	}

	public async Task<IEnumerable<ResponseServicePackageStatisticByNumberOfBookingsVM>> GetServicePackageStatistics(DateTime From, DateTime To)
	{
		var servicePackages = await _unitOfWork.ServicePackages
			.Get(sp => !sp.IsDeleted)
			.AsNoTracking()
			.ToArrayAsync();

        var bookings = await _unitOfWork.Bookings
			.Get(b => !b.IsDeleted && b.BookingDate >= From && b.BookingDate <= To && b.BookingStatus == 2)
			.GroupBy(b => b.ServicePackageId)
			.Select(b => new { ServicePackageId = b.Key, Count = b.Count()})
            .AsNoTracking()
            .ToArrayAsync();

		var result = new List<ResponseServicePackageStatisticByNumberOfBookingsVM>();
		servicePackages.AsParallel()
			.WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2)))
			.ForAll(sp =>
			{
				var statistic = new ResponseServicePackageStatisticByNumberOfBookingsVM();
                statistic.ServicePackageName = sp.ServicePackageName;

                var bookingNumbers = bookings.Where(b => b.ServicePackageId == sp.Id).FirstOrDefault();
				if(bookingNumbers is null)
				{
					statistic.BookingNumbers = 0;
                }
				else
				{
					statistic.BookingNumbers = bookingNumbers.Count;
                }

                result.Add(statistic);
            });

		return result;
	}

	public async Task<IEnumerable<ResponseServicePackageStatisticByTotalSpendingVM>> GetServicePackageStatisticsWithTotalSpending(DateTime From, DateTime To)
	{
        var servicePackages = await _unitOfWork.ServicePackages
            .Get(sp => !sp.IsDeleted)
            .AsNoTracking()
            .ToArrayAsync();

        var bookings = await _unitOfWork.Bookings
            .Get(b => !b.IsDeleted && b.BookingDate >= From && b.BookingDate <= To && b.BookingStatus == 2)
            .GroupBy(b => b.ServicePackageId)
            .Select(b => new { ServicePackageId = b.Key, Total = b.Sum(b => b.TotalPrice) })
            .AsNoTracking()
            .ToArrayAsync();

        var result = new List<ResponseServicePackageStatisticByTotalSpendingVM>();
        servicePackages.AsParallel()
            .WithDegreeOfParallelism(Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.1 * 2)))
            .ForAll(sp =>
            {
                var statistic = new ResponseServicePackageStatisticByTotalSpendingVM();
                statistic.ServicePackageName = sp.ServicePackageName;

                var bookingSpending = bookings.Where(b => b.ServicePackageId == sp.Id).FirstOrDefault();
                if (bookingSpending is null)
                {
                    statistic.TotalSpending = 0;
                }
                else
                {
                    statistic.TotalSpending = bookingSpending.Total;
                }

                result.Add(statistic);
            });

        return result;
    }
}

