using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.DAL.Db;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Library.DAL.Repositories;

public sealed class GenreRepository : IGenreRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<GenreRepository> _logger;

    public GenreRepository(ISqlConnectionFactory connectionFactory, ILogger<GenreRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Genre?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "SELECT Id, Name FROM Genres WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Genre
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading genre {GenreId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading genre {GenreId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Genre>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "SELECT Id, Name FROM Genres ORDER BY Name;";
            using var command = new SqlCommand(sql, connection);

            var rows = new List<Genre>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new Genre
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                });
            }

            return rows;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while listing genres.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while listing genres.");
            throw;
        }
    }

    public async Task<int> AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               INSERT INTO Genres (Name)
                               OUTPUT INSERTED.Id
                               VALUES (@Name);
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", genre.Name);

            var insertedId = (int)await command.ExecuteScalarAsync(cancellationToken);
            return insertedId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while adding genre.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding genre.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "UPDATE Genres SET Name = @Name WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", genre.Id);
            command.Parameters.AddWithValue("@Name", genre.Name);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while updating genre {GenreId}.", genre.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating genre {GenreId}.", genre.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "DELETE FROM Genres WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while deleting genre {GenreId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting genre {GenreId}.", id);
            throw;
        }
    }
}

