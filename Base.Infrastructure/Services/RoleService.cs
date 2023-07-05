using Base.Core.Entity;
using Base.Infrastructure.Data;
using Base.Infrastructure.IService;

namespace Base.Infrastructure.Services;

internal class RoleService : IRoleService
{
    private readonly UnitOfWork _unitOfWork;

    public RoleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public IEnumerable<Role>? GetAllRole()
    {
        return _unitOfWork.Roles.FindAll();
    }
}
