using AutoMapper;
using Base.Core.Common;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers;

[Authorize(Policy = "Role")]
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IMapper _mapper;

    public RolesController(IRoleService roleService, IMapper mapper)
    {
        _roleService = roleService;
        _mapper = mapper;
    }

    [Authorize(Policy = "All")]
    [HttpGet("All")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseRoleVM>))]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRole();
        return Ok(_mapper.Map<IEnumerable<ResponseRoleVM>>(roles));
    }

    [Authorize(Policy = "All")]
    [HttpGet("all-delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseRoleVM>))]
    public async Task<IActionResult> GetAllDeletedRoles()
    {
        var roles = await _roleService.GetAllDeletedRole();
        return Ok(_mapper.Map<IEnumerable<ResponseRoleVM>>(roles));
    }

    [Authorize(Policy = "Read")]
    [HttpGet("{roleId}", Name = nameof(GetRoleById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseRoleVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> GetRoleById(Guid roleId)
    {
        if (ModelState.IsValid)
        {
            var role = await _roleService.GetRoleById(roleId);
            if (role == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy vai trò",
                    Error = new List<string>() { "Can not find role with the given id: " + roleId }
                });
            }
            return Ok(_mapper.Map<ResponseRoleVM>(role));
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

    [Authorize(Policy = "Write")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseRoleVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewRole(RoleVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var role = await _roleService.AddNewRole(resource);
                if (role != null)
                {
                    return CreatedAtAction(nameof(GetRoleById),
                        new
                        {
                            roleId = role.Id,
                        },
                        _mapper.Map<ResponseRoleVM>(role));
                }
                else
                {
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Đã có lỗi xảy ra"
                    });
                }
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Error = new List<string>() { "Invalid input" }
            });
        }
        catch (CustomException ex)
        {
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = ex.Message,
                Error = ex.Errors,
                IsRestored = ex.IsRestored
            });
        }
        catch(ObjectDisposedException ex)
        {
            return StatusCode(500, new ServiceResponse
            {
                IsSuccess = false,
                Message = "Đã có lỗi xảy ra",
                Error = new List<string>() { ex.Message }
            });
        }
    }

    [Authorize(Policy = "Update")]
    [HttpPost("Claims/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewClaims(Guid roleId, [FromBody] IEnumerable<ClaimVM> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.AddRoleClaims(roleId, resource);
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
    }

    [Authorize(Policy = "Update")]
    [HttpPut("{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] UpdatedRoleVM resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.UpdateRole(roleId, resource);
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
    }


    [Authorize(Policy = "Update")]
    [HttpPut("Claims/{roleId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> UpdateClaims(Guid roleId, [FromBody] IEnumerable<UpdatedClaimVM> resource)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.UpdateRoleClaims(roleId, resource);
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
    }

    [Authorize(Policy = "Delete")]
    [HttpDelete("Role/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteRole(Guid id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.SoftDeleteRole(id);
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

    [Authorize(Policy = "Delete")]
    [HttpDelete("Claim/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> SoftDeleteRoleClaim(int id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.DeleteRoleClaim(id);
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

    [Authorize(Policy = "Restore")]
    [HttpPatch("restore-role/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> RestoreRole(Guid id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var result = await _roleService.RestoreRole(id);
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
                Message = "Khôi phục thất bại",
                Error = new List<string>() { ex.Message }
            });
        }
    }
}
