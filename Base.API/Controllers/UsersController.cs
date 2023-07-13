using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

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

    [HttpGet("All-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    public IActionResult GetAllUser()
    {
        var result = _userService.GetAllUser();
        if(result.IsNullOrEmpty() || result == null)
        {
            return NotFound(new ServiceResponse
            {
                IsSuccess = true,
                Message = "empty"
            });
        }
        return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
    }

    [HttpGet("All-Managed-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllManagedUser()
    {
        try
        {
            var result = await _userService.GetAllManagedUser();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Please Login First",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpGet("All-Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetAllManager()
    {
        try
        {
            var result = await _userService.GetAllManager();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Please Login First",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [HttpGet("{UserId}", Name = nameof(GetUserById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseUserInformationVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetUserById(Guid UserId)
    {
        var result = await _userService.GetUserById(UserId);
        if (result == null)
        {
            return NotFound( new ServiceResponse
            {
                IsSuccess = false,
                Message = "No Customer Found with the given id"
            });
        }
        return Ok(_mapper.Map<ResponseUserInformationVM>(result));
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
            Message = "Some properties are not valid"
        });
    }

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
                Message = "Some Error Happend",
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
            Message = "Some properties are not valid"
        });
    }

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
            Message = "Some properties are not valid"
        });
    }
}
