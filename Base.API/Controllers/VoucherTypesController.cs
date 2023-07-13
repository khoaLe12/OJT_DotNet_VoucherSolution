using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherTypesController : ControllerBase
    {
        private readonly IVoucherTypeService _voucherTypeService;
        private readonly IMapper _mapper;

        public VoucherTypesController(IVoucherTypeService voucherTypeService, IMapper mapper)
        {
            _voucherTypeService = voucherTypeService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseVoucherTypeVM>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        public IActionResult GetAllVoucherTypes()
        {
            var result = _voucherTypeService.GetAllVoucherTypes();
            if (result.IsNullOrEmpty() || result == null)
            {
                return NotFound(new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "empty"
                });
            }
            return Ok(_mapper.Map<IEnumerable<ResponseVoucherTypeVM>>(result));
        }

        [HttpGet("{voucherTypeId}", Name = nameof(GetVoucherTypeById))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseVoucherTypeVM))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public IActionResult GetVoucherTypeById(int voucherTypeId)
        {
            if (ModelState.IsValid)
            {
                var result = _voucherTypeService.GetVoucherTypeById(voucherTypeId);
                if(result == null)
                {
                    return NotFound(new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "No Voucher type Found with the given id"
                    });
                }
                return Ok(_mapper.Map<ResponseVoucherTypeVM>(result));
            }
            return BadRequest(new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some properties are not valid"
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseVoucherTypeVM))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
        public async Task<IActionResult> AddNewVoucherType([FromBody] VoucherTypeVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _voucherTypeService.AddNewVoucherType(_mapper.Map<VoucherType>(resource), resource.ServicePackageIds);
                if (result != null)
                {
                    return CreatedAtAction(nameof(GetVoucherTypeById),
                        new
                        {
                            voucherTypeId = result.Id
                        },
                        _mapper.Map<ResponseVoucherTypeVM>(result));
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
    }
}
