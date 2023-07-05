using AutoMapper;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public IActionResult GetAllUser()
    {
        var result = _userService.GetAllUser();
        if(result.IsNullOrEmpty() || result == null)
        {
            return NotFound("No User Found (empty) !!! ");
        }
        return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
    }

    [HttpGet("All-Managed-Users")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseUserInformationVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public IActionResult GetAllManagedUser()
    {
        try
        {
            var result = _userService.GetAllManagedUser();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound("No User Found (empty) !!! ");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseUserInformationVM>>(result));
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.ToString() + "\n\n Please Login First !!!!!");
        }
    }

    [HttpGet("{UserId}", Name = nameof(GetUserById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseUserInformationVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<IActionResult> GetUserById(Guid UserId)
    {
        var result = await _userService.GetUserById(UserId);
        if (result == null)
        {
            return NotFound("No Customer Found with the given id !!!");
        }
        return Ok(_mapper.Map<ResponseUserInformationVM>(result));
    }

    [Authorize(Policy = "SalesEmployee")]
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
            Message = "Some properties are not valid !!!"
        });
    }

    [Authorize(Policy = "SalesEmployee")]
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
            Message = "Some properties are not valid !!!"
        });
    }

    [Authorize(Policy = "SalesAdmin")]
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
            Message = "Some properties are not valid !!!"
        });
    }
}
