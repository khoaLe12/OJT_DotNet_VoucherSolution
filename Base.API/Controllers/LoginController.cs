using AutoMapper;
using Base.API.Services;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Icao;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly IJWTTokenService _jwtTokenService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, ICustomerService customerService, IJWTTokenService jwtTokenService, IMapper mapper, IConfiguration configuration)
        {
            _mapper = mapper;
            _userService = userService;
            _customerService = customerService;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login/user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticatedResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(resource);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginUser!);
                    if (tokenString != null)
                    {
                        return Ok(new AuthenticatedResponse {
                            CustomerInformation = null,
                            UserInformation = _mapper.Map<ResponseUserInformationVM>(result.LoginUser!),
                            Token = tokenString }) ;
                    }
                }
                else
                {
                    return Unauthorized(result);
                }
            }
            return BadRequest(new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Errors = new List<string>() { "Invalid input" }
            });
        }

        [HttpPost]
        [Route("login/customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticatedResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginCustomer([FromBody] LoginCustomerVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.LoginCustomerAsync(resource);
                if (result.IsSuccess)
                {
                    var tokenString = _jwtTokenService.CreateToken(result.LoginCustomer!);
                    if (tokenString != null)
                    {
                        return Ok(new AuthenticatedResponse {
                            UserInformation = null,
                            CustomerInformation = _mapper.Map<ResponseCustomerInformationVM>(result.LoginCustomer!),
                            Token = tokenString });
                    }
                }
                else
                {
                    return Unauthorized(result);
                }
            }
            return BadRequest(new CustomerManagerResponse
            {
                IsSuccess = false,
                Message = "Dữ liệu không hợp lệ",
                Errors = new List<string>() { "Invalid input" }
            });
        }

        // api/auth/confirmemail/user?userid&token
        [HttpGet("confirmemail/user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmUserEmailAsync(Guid? userid, string? token)
        {
            if(userid is null || string.IsNullOrWhiteSpace(token))
            {
                return NotFound();
            }

            var result = await _userService.ConfirmEmailAsync((Guid)userid, token);
            if (result.IsSuccess)
            {
                return Redirect($"{_configuration["AppUrl"]}/confirmemail.html");
            }
            else
            {
                return BadRequest(result);
            }
        }

        // api/auth/confirmemail/customer?customerid&token
        [HttpGet("confirmemail/customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmCustomerEmailAsync(Guid? customerid, string? token)
        {
            if (customerid is null || string.IsNullOrWhiteSpace(token))
            {
                return NotFound();
            }

            var result = await _customerService.ConfirmEmailAsync((Guid)customerid, token);
            if (result.IsSuccess)
            {
                return Redirect($"{_configuration["AppUrl"]}/confirmemail.html");
            }
            else
            {
                return BadRequest(result);
            }
        }

        // api/auth/user/resetpassword
        [HttpPost("user/resetpassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UserManagerResponse))]
        public async Task<IActionResult> ResetUserPasswordAsync([FromForm] ForgetPasswordVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ForgetAndResetPasswordAsync(resource);
                if (result.IsSuccess)
                {
                    return Redirect($"{_configuration["AppUrl"]}/resetpassword.html");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest(new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = new List<string>() { "Invalid input" }
                });
            }
        }

        // api/auth/customer/resetpassword
        [HttpPost("customer/resetpassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerManagerResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CustomerManagerResponse))]
        public async Task<IActionResult> ResetCustomerPasswordAsync([FromForm] ForgetPasswordVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.ForgetAndResetPasswordAsync(resource);
                if (result.IsSuccess)
                {
                    return Redirect($"{_configuration["AppUrl"]}resetpassword.html");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest(new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = new List<string>() { "Invalid input" }
                });
            }
        }
    }
}
