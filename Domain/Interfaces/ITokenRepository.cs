using ApiBase.Domain.Entities;

namespace ApiBase.Domain.Interfaces;

public interface ITokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
}
