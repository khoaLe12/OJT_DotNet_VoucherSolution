using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "User")]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UsersController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet("All-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    public IActionResult GetAllUser()
    {
        var result = _userService.GetAllUser();
        return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{UserId}", Name = nameof(GetUserById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseUserInformationVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetUserById(Guid UserId)
    {
        var result = await _userService.GetUserById(UserId);
        if (result == null)
        {
            return NotFound(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy"
            });
        }
        return Ok(_mapper.Map<ResponseUserInformationVM>(result));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("All-Managed-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllManagedUsers()
    {
        try
        {
            var result = await _userService.GetAllManagedUser();
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không hợp lệ",
                Error = new List<string>() { ex.Message }
            });
        }
        catch(ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { "Can not find logged User with the given user id" }
            });
        }
    }

    [HttpGet("All-Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllManagers()
    {
        try
        {
            var result = await _userService.GetAllManager();
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không hợp lệ",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { "Can not find logged User with the given user id" }
            });
        }
    }

    [Authorize(Policy = "Customer")]
    [HttpGet("All-Supporting-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllSupportingUsers()
    {
        try
        {
            var result = await _userService.GetAllSupportingUser();
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Hành động không hợp lệ",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = new List<string>() { "Can not find logged Customer with the given customer id" }
            });
        }
    }

    [HttpPost("Reset-Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
    public async Task<IActionResult> ResetPassword(ResetPasswordVM resource)
    {
        if (ModelState.IsValid)
        {
            var result = await _userService.ResetPassword(resource);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        return BadRequest(new UserManagerResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Update")]
    [HttpPost("Assign-Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(UserManagerResponse))]
    public async Task<IActionResult> AssignManager([FromQuery] Guid ManagerId, [FromQuery] Guid UserId)
    {
        try
        {
            var result = await _userService.AssignManager(ManagerId, UserId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Đã có lỗi xảy ra",
                Errors = new List<string>() { ex.Message }
            });
        }
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
    public async Task<IActionResult> UpdateInformation([FromBody] UpdateInformationVM resource)
    {
        if (ModelState.IsValid)
        {
            var result = await _userService.UpdateInformation(resource);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        return BadRequest(new UserManagerResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Update")]
    [HttpPatch("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
    public async Task<IActionResult> PatchUpdate(Guid userId, [FromBody] JsonPatchDocument<User> patchDoc)
    {
        if(ModelState.IsValid && patchDoc != null)
        {
            var result = await _userService.PatchUpdate(userId, patchDoc, ModelState);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        return BadRequest(new UserManagerResponse
        {
            IsSuccess = false,
            Message = "Dữ liệu không hợp lệ"
        });
    }

    [Authorize(Policy = "Delete")]
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteServicePackage(Guid userId)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.SoftDelete(userId);
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
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Cập nhật thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}
