using Base.API.Services;
using Base.Core.Common;
using Base.Core.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundTaskController : ControllerBase
    {
        private readonly IBackgroundTaskService _backgroundTaskService;

        public BackgroundTaskController(IBackgroundTaskService backgroundTaskService)
        {
            _backgroundTaskService = backgroundTaskService;
        }

        [Authorize(Policy = "VoucherType")]
        [Authorize(Policy = "Update")]
        [HttpPost("schedule-vouchertype")]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> ScheduleVoucherSell([FromBody] SchedulesVoucherTypeVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _backgroundTaskService.ScheduleVoucherSell(resource);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
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
