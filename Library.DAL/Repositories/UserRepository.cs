using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.BL.Interfaces.Repositories.Models;
using Library.DAL.Db;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Library.DAL.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ISqlConnectionFactory connectionFactory, ILogger<UserRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<UserAuthRecord?> GetAuthByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               SELECT Id, Email, PasswordHash, FirstName, LastName, Role
                               FROM Users
                               WHERE Email = @Email;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            var role = reader.GetString(reader.GetOrdinal("Role"));
            return new UserAuthRecord
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Role = ParseRole(role),
            };
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading user auth by email {Email}.", email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading user auth by email {Email}.", email);
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               SELECT Id, Email, FirstName, LastName, DateOfBirth, Role, CreatedAt
                               FROM Users
                               WHERE Id = @Id;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapUser(reader);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading user {UserId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading user {UserId}.", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               SELECT Id, Email, FirstName, LastName, DateOfBirth, Role, CreatedAt
                               FROM Users
                               ORDER BY LastName, FirstName;
                               """;

            using var command = new SqlCommand(sql, connection);
            var rows = new List<User>();

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(MapUser(reader));
            }

            return rows;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while listing users.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while listing users.");
            throw;
        }
    }

    public async Task<int> AddWithPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               INSERT INTO Users (Email, PasswordHash, FirstName, LastName, DateOfBirth, Role)
                               OUTPUT INSERTED.Id
                               VALUES (@Email, @PasswordHash, @FirstName, @LastName, @DateOfBirth, @Role);
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
            command.Parameters.AddWithValue("@Role", ToDbRole(user.Role));

            var insertedId = (int)await command.ExecuteScalarAsync(cancellationToken);
            return insertedId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while adding user.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding user.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               UPDATE Users
                               SET Email = @Email,
                                   FirstName = @FirstName,
                                   LastName = @LastName,
                                   DateOfBirth = @DateOfBirth,
                                   Role = @Role
                               WHERE Id = @Id;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
            command.Parameters.AddWithValue("@Role", ToDbRole(user.Role));

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while updating user {UserId}.", user.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user {UserId}.", user.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "DELETE FROM Users WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while deleting user {UserId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting user {UserId}.", id);
            throw;
        }
    }

    private static User MapUser(SqlDataReader reader)
    {
        var role = reader.GetString(reader.GetOrdinal("Role"));

        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
            Role = ParseRole(role),
            CreatedAt = SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("CreatedAt"))),
        };
    }

    private static UserRole ParseRole(string role) =>
        role switch
        {
            "User" => UserRole.User,
            "Admin" => UserRole.Admin,
            _ => throw new InvalidOperationException($"Unknown role value '{role}'."),
        };

    private static string ToDbRole(UserRole role) =>
        role switch
        {
            UserRole.User => "User",
            UserRole.Admin => "Admin",
            _ => throw new InvalidOperationException($"Unsupported role '{role}'."),
        };

    private static DateTime SpecifyUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
