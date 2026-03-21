using Library.BL.Entities;
using Library.BL.Interfaces.Repositories.Models;

namespace Library.BL.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserAuthRecord?> GetAuthByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> AddWithPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
