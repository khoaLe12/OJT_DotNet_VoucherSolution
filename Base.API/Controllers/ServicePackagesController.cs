﻿using AutoMapper;
using Base.Core.Common;
using Base.Core.Entity;
using Base.Core.ViewModel;
using Base.Infrastructure.IService;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicePackagesController : ControllerBase
{
    private readonly IServicePackageService _servicePackageService;
    private readonly IMapper _mapper;

    public ServicePackagesController(IServicePackageService servicePackageService, IMapper mapper)
    {
        _servicePackageService = servicePackageService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ResponseServicePackageVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    public IActionResult GetAllServicePackages()
    {
        var result = _servicePackageService.GetALlServicePackage();
        if(result.IsNullOrEmpty() || result == null)
        {
            return NotFound(new ServiceResponse
            {
                IsSuccess = false,
                Message = "empty"
            });
        }
        return Ok(_mapper.Map<IEnumerable<ServicePackage>, IEnumerable<ResponseServicePackageVM>>(result));
    }

    [HttpGet("{servicePackageId}", Name = nameof(GetServicePackageById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseServicePackageVM))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ServiceResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public IActionResult GetServicePackageById(int servicePackageId)
    {
        if (ModelState.IsValid)
        {
            var result = _servicePackageService.GetServicePackageById(servicePackageId);
            if (result == null)
            {
                return NotFound( new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "No Service Package Found with the given id"
                });
            }
            return Ok(_mapper.Map<ServicePackage, ResponseServicePackageVM>(result));
        }
        return BadRequest( new ServiceResponse
        {
            IsSuccess = false,
            Message = "Some properties are not valid"
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseServicePackageVM))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ServiceResponse))]
    public async Task<IActionResult> AddNewServicePackage([FromBody] ServicePackageVM resource)
    {
        if (ModelState.IsValid)
        {
            var newServicePackage = _mapper.Map<ServicePackage>(resource);
            var result = await _servicePackageService.AddNewServicePackage(newServicePackage, resource.ServicesIds);
            if(result != null)
            {
                return CreatedAtAction(nameof(GetServicePackageById),
                    new
                    {
                        servicePackageId = result.Id,
                    },
                    _mapper.Map<ResponseServicePackageVM>(result));
            }
            return BadRequest( new ServiceResponse
            {
                IsSuccess = false,
                Message = "Some error happened"
            });
        }
        return BadRequest(new ServiceResponse
        {
            IsSuccess = false,
            Message = "Some properties are not valid"
        });
    }
}