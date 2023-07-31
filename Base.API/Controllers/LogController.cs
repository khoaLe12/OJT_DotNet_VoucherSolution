using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Authorize(Policy = "Log")]
[Route("api/[controller]")]
[ApiController]
public class LogController : ControllerBase
{
    private readonly ILogService _logService;
	private readonly IMapper _mapper;

	public LogController(ILogService logService, IMapper mapper)
	{
		_logService = logService;
		_mapper = mapper;
	}

    [Authorize(Policy = "Read")]
    [HttpGet("update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseLogVM>))]
    public async Task<IActionResult> GetAllUpdateActivities()
	{
		var result = await _logService.GetUpdateActivities();
		return Ok(_mapper.Map<IEnumerable<ResponseLogVM>>(result));
	}

    [Authorize(Policy = "Read")]
    [HttpGet("delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseLogVM>))]
    public async Task<IActionResult> GetAllDeleteActivities()
	{
		var result = await _logService.GetDeleteActivities();
		return Ok(_mapper.Map<IEnumerable<ResponseLogVM>>(result));
	}

	[Authorize(Policy = "Read")]
	[HttpGet("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseLogVM>))]
    public async Task<IActionResult> GetAllCreateActivities()
	{
		var result = await _logService.GetCreateActivities();
		return Ok(_mapper.Map<IEnumerable<ResponseLogVM>>(result));
	}

    [Authorize(Policy = "Restore")]
    [HttpPatch("{id}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> RestoreDeletedEntity(int id)
    {
		if (ModelState.IsValid)
		{
            var result = await _logService.Recover(id);
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
