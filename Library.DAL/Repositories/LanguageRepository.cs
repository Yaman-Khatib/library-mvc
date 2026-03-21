using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.DAL.Db;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Library.DAL.Repositories;

public sealed class LanguageRepository : ILanguageRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<LanguageRepository> _logger;

    public LanguageRepository(ISqlConnectionFactory connectionFactory, ILogger<LanguageRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Language?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "SELECT Id, Name FROM Languages WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Language
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading language {LanguageId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading language {LanguageId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Language>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "SELECT Id, Name FROM Languages ORDER BY Name;";
            using var command = new SqlCommand(sql, connection);

            var rows = new List<Language>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new Language
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                });
            }

            return rows;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while listing languages.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while listing languages.");
            throw;
        }
    }

    public async Task<int> AddAsync(Language language, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               INSERT INTO Languages (Name)
                               OUTPUT INSERTED.Id
                               VALUES (@Name);
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", language.Name);

            var insertedId = (int)await command.ExecuteScalarAsync(cancellationToken);
            return insertedId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while adding language.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding language.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Language language, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "UPDATE Languages SET Name = @Name WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", language.Id);
            command.Parameters.AddWithValue("@Name", language.Name);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while updating language {LanguageId}.", language.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating language {LanguageId}.", language.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "DELETE FROM Languages WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while deleting language {LanguageId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting language {LanguageId}.", id);
            throw;
        }
    }
}

