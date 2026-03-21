using Library.BL.Entities;

namespace Library.BL.Interfaces.Repositories;

public interface ILanguageRepository
{
    Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Language>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(Language language, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Language language, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

