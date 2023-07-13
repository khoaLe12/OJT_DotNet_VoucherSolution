using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Policy = "Register Customer")]
        [HttpPost]
        [Route("Customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> RegisterNewCustomer([FromBody] CustomerVM resource)
        {
            try
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
                    Message = "Some properties are not valid"
                });
            }
            catch (ObjectDisposedException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Registry new Customer Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [Authorize(Policy = "Register User")]
        [HttpPost]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> RegisterNewUser([FromBody] UserVM resource)
        {
            try
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
                    Message = "Some properties are not valid"
                });
            }
            catch (ObjectDisposedException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Registry new User Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Register new User Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }
    }
}
