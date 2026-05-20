using System.Data;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Infrastructure.Data;
using Dapper;

namespace ApiBase.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public TokenRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task AddAsync(RefreshToken token)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        var sql = @"
            INSERT INTO RefreshTokens (Token, UserId, ExpiryDate, IsRevoked)
            VALUES (@Token, @UserId, @ExpiryDate, @IsRevoked);";
        await connection.ExecuteAsync(sql, token);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            "SELECT * FROM RefreshTokens WHERE Token = @Token", new { Token = token });
    }

    public async Task RevokeAsync(string token)
    {
        using var connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE RefreshTokens SET IsRevoked = true WHERE Token = @Token", new { Token = token });
    }
}
