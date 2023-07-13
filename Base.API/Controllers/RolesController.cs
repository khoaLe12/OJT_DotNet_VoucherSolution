using AutoMapper;
using Azure;
using Base.Core.Common;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Base.API.Controllers
{
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseRoleVM>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRole();
            if (roles.IsNullOrEmpty() || roles == null)
            {
                return NotFound( new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseRoleVM>>(roles));
        }

        [HttpGet("{roleId}", Name = nameof(GetRoleById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseRoleVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetRoleById(Guid roleId)
        {
            var role = await _roleService.GetRoleById(roleId);
            if(role == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Role Not Found"
                });
            }
            return Ok(_mapper.Map<ResponseRoleVM>(role));
        }

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
                            Message = "Some errors happened"
                        });
                    }
                }
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some properties are not valid"
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch(ObjectDisposedException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] JsonPatchDocument<Role> patchDoc)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _roleService.PatchUpdate(roleId, patchDoc, ModelState);
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
                        Message = "Some properties are not valid"
                    });
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Role Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [HttpPut("Claims/{roleId}")]
        public async Task<IActionResult> UpdateRoleClaims(int roleId, [FromBody] IEnumerable<UpdateClaimVM> resource)
        {
            if (ModelState.IsValid)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some properties are not valid"
                });
            }
        }
    }
}
