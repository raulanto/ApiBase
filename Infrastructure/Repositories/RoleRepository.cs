using System.Data;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Infrastructure.Data;
using Dapper;

namespace ApiBase.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public RoleRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Role>(
            "SELECT * FROM Roles WHERE Name = @Name", new { Name = name });
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var sql = @"
            SELECT p.* FROM Permissions p
            INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
            WHERE rp.RoleId = @RoleId";
        return await connection.QueryAsync<Permission>(sql, new { RoleId = roleId });
    }
}
