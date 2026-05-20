using ApiBase.Domain.Entities;

namespace ApiBase.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId);
}
