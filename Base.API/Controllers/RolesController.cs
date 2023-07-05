using AutoMapper;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public IActionResult GetAllRoles()
        {
            var roles = _roleService.GetAllRole();
            if (roles.IsNullOrEmpty() || roles == null)
            {
                return NotFound("No Roles Found (empty) !!!!");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseRoleVM>>(roles));
        }
    }
}
