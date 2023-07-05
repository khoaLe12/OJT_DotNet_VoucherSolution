namespace Base.Core.Application;

public interface ICurrentUserService
{
    public Guid UserId { get; }
}
