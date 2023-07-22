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

        [Authorize(Policy = "Customer")]
        [Authorize(Policy = "Write")]
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
                    Message = "Dữ liệu không hợp lệ",
                    Errors = new List<string>() { "Invalid input" }
                });
            }
            catch (ObjectDisposedException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Đã có lỗi xảy ra",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [Authorize(Policy = "User")]
        [Authorize(Policy = "Write")]
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
                    Message = "Dữ liệu không hợp lệ",
                    Errors = new List<string>() { "Invalid input" }
                });
            }
            catch (ObjectDisposedException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Đã có lỗi xảy ra",
                    Error = new List<string>() { ex.Message }
                });
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Đăng ký thất bại",
                    Error = new List<string>() { ex.Message }
                });
            }
        }
    }
}
