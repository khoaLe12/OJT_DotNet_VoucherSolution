using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        public IActionResult GetAllExpiredDateExtensions()
        {
            var result = _expiredDateExtensionService.GetAllExpiredDateExtensions();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound( new ServiceResponse
                {
                    IsSuccess = true,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseExpiredDateExtensionVM>>(result));
        }

        [HttpGet("{extensionId}", Name = nameof(GetExpiredDateExtensionById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> GetExpiredDateExtensionById(int extensionId)
        {
            if (ModelState.IsValid)
            {
                var result = await _expiredDateExtensionService.GetExpiredDateExtensionById(extensionId);
                if (result == null)
                {
                    return NotFound( new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "No Expired Extension Found with the given id"
                    });
                }
                return Ok(_mapper.Map<ResponseExpiredDateExtensionVM>(result));
            }
            return BadRequest( new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid"
            });
        }

        [Authorize(Policy = "SalesEmployee")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseExpiredDateExtensionVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> AddNewExpiredDateExtension([FromBody] ExpiredDateExtensionVM resource)
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
                    return BadRequest(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Some errors happened"
                    });
                }
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some properties are not valid"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ServiceResponse 
                { 
                    IsSuccess = false,
                    Message = "Create new Voucher Extension Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Create new Voucher Extension Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> UpdateInformation(int id, [FromBody] ExpiredDateExtensionVM resource)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.UpdateVoucherExtension(_mapper.Map<ExpiredDateExtension>(resource), id);
                    if (result.IsSuccess)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest(result);
                    }
                }
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some properties are not valid"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Voucher Extension Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Voucher Extension Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }

        /*[HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> PatchUpdate(int id, [FromBody] JsonPatchDocument<ExpiredDateExtension> patchDoc)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _expiredDateExtensionService.PatchUpdate(id, patchDoc, ModelState);
                    if (result.IsSuccess)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest(result);
                    }
                }
                return BadRequest(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Some properties are not valid"
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Update Voucher Extension Fail",
                    Error = new List<string>() { ex.Message }
                });
            }
        }*/
    }
}
