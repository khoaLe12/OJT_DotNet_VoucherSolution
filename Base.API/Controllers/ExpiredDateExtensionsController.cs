using AutoMapper;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpiredDateExtensionsController : ControllerBase
    {
        private readonly IExpiredDateExtensionService _expiredDateExtensionService;
        private readonly IMapper _mapper;

        public ExpiredDateExtensionsController(IExpiredDateExtensionService expiredDateExtensionService, IMapper mapper)
        {
            _expiredDateExtensionService = expiredDateExtensionService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseExpiredDateExtensionVM>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public IActionResult GetAllExpiredDateExtensions()
        {
            var result = _expiredDateExtensionService.GetAllExpiredDateExtensions();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound("No Expired Extension Found (empty) !!!");
            }
            return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
        }

        [HttpGet("{extensionId}", Name = nameof(GetExpiredDateExtensionById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> GetExpiredDateExtensionById(int extensionId)
        {
            if (ModelState.IsValid)
            {
                var result = await _expiredDateExtensionService.GetExpiredDateExtensionById(extensionId);
                if (result == null)
                {
                    return NotFound("No Expired Extension Found with the given id !!!");
                }
                return Ok(_mapper.Map<ResponseExpiredDateExtensionVM>(result));
            }
            return BadRequest("Some properties are not valid !!!");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> AddNewBooking([FromBody] ExpiredDateExtensionVM resource)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.AddNewExpiredDateExtension(_mapper.Map<ExpiredDateExtension>(resource), resource.VoucherId);
                    if (result != null)
                    {
                        return CreatedAtAction(nameof(GetExpiredDateExtensionById),
                            new
                            {
                                extensionId = result.Id
                            },
                            _mapper.Map<ResponseExpiredDateExtensionVM>(result));
                    }
                    return BadRequest("Some errors happened");
                }
                return BadRequest("Some properties are not valid !!!");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.ToString() + "\n\n Please Login First !!!!!");
            }
        }
    }
}
