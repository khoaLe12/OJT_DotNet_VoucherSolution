using AutoMapper;
using Base.API.Services;
using Base.Core.Entity;
using Base.Core.Identity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly IJWTTokenService<User> _jwtTokenUserService;
        private readonly IJWTTokenService<Customer> _jwtTokenCustomerService;
        private readonly IMapper _mapper;
        public LoginController(IUserService userService, ICustomerService customerService, IJWTTokenService<User> jwtTokenUserService, IJWTTokenService<Customer> jwtTokenCustomerService, IMapper mapper)
        {
            _mapper = mapper;
            _userService = userService;
            _customerService = customerService;
            _jwtTokenCustomerService = jwtTokenCustomerService;
            _jwtTokenUserService = jwtTokenUserService;
        }

        [HttpPost]
        [Route("User")]
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
                    var tokenString = await _jwtTokenUserService.CreateToken(result.LoginUser!);
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
                Message = "Some properties are not valid"
            });
        }

        [HttpPost]
        [Route("Customer")]
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
                    var tokenString = await _jwtTokenCustomerService.CreateToken(result.LoginCustomer!);
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
                Message = "Some properties are not valid"
            });
        }
    }
}
