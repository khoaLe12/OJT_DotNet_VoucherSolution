using Base.Core.Common;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Authorize(Policy = "Statistic")]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticalService _statistical;
        public StatisticController(IStatisticalService statistical)
        {
            _statistical = statistical;
        }

        [Authorize(Policy = "Read")]
        [HttpGet("customer-consumption")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseConsumptionStatisticVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetConsumptionStatistics([FromQuery] Guid customerId, [FromQuery] DateTime From, [FromQuery] DateTime To)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _statistical.GetConsumptionStatisticOfCustomer(customerId, From, To));
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Dữ liệu không hợp lệ",
                        Error = new List<string>() { "Invalid input" }
                    });
                }
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Thống kê thất bại",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [Authorize(Policy = "Read")]
        [HttpGet("booked-servicepackage-count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServicePackageStatisticByNumberOfBookingsVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetServicePackageCountStatistic([FromQuery] DateTime From, [FromQuery] DateTime To)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _statistical.GetServicePackageStatistics(From, To));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ",
                    Error = new List<string>() { "Invalid input" }
                });
            }
        }

        [Authorize(Policy = "Read")]
        [HttpGet("servicepackage-consumption")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServicePackageStatisticByTotalSpendingVM>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetServicePackageStatisticByTotalSpending([FromQuery] DateTime From, [FromQuery] DateTime To)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _statistical.GetServicePackageStatisticsWithTotalSpending(From, To));
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ",
                    Error = new List<string>() { "Invalid input" }
                });
            }
        }
    }
}
