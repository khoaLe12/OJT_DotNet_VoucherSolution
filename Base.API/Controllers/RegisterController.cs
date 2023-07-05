using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;

        public RegisterController(IUserService userService, ICustomerService customerService)
        {
            _userService = userService;
            _customerService = customerService;
        }

        [Authorize(Policy = "SalesEmployee")]
        [HttpPost]
        [Route("Customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
        public async Task<IActionResult> RegisterNewCustomer([FromBody] CustomerVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.RegisterNewCustomerAsync(resource);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }

            return BadRequest(new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid !!!"
            });
        }

        [Authorize(Policy = "SupAdmin")]
        [HttpPost]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
        public async Task<IActionResult> RegisterNewUser([FromBody] UserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterNewUserAsync(resource);
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
}
