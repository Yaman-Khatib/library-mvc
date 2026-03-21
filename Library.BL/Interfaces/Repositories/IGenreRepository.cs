using Library.BL.Entities;

namespace Library.BL.Interfaces.Repositories;

public interface IGenreRepository
{
    Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Genre>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Genre genre, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

