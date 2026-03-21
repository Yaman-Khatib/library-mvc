using Library.BL.Dtos;
using Library.BL.Interfaces.Repositories;
using Library.BL.Interfaces.Repositories.Models;
using Library.DAL.Db;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Library.DAL.Repositories;

public sealed class BorrowingRepository : IBorrowingRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<BorrowingRepository> _logger;

    public BorrowingRepository(ISqlConnectionFactory connectionFactory, ILogger<BorrowingRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<BorrowingDetailsDto?> GetByIdAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               SELECT
                                   b.Id,
                                   b.UserId,
                                   u.FirstName,
                                   u.LastName,
                                   b.BookId,
                                   bk.Title AS BookTitle,
                                   b.BorrowDate,
                                   b.DueDate,
                                   b.ReturnDate
                               FROM Borrowings b
                               INNER JOIN Users u ON u.Id = b.UserId
                               INNER JOIN Books bk ON bk.Id = b.BookId
                               WHERE b.Id = @Id;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", borrowingId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapBorrowingDetails(reader);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading borrowing details {BorrowingId}.", borrowingId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading borrowing details {BorrowingId}.", borrowingId);
            throw;
        }
    }

    public async Task<IReadOnlyList<BorrowingDetailsDto>> ListByUserAsync(
        int userId,
        bool onlyActive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var sql = """
                      SELECT
                          b.Id,
                          b.UserId,
                          u.FirstName,
                          u.LastName,
                          b.BookId,
                          bk.Title AS BookTitle,
                          b.BorrowDate,
                          b.DueDate,
                          b.ReturnDate
                      FROM Borrowings b
                      INNER JOIN Users u ON u.Id = b.UserId
                      INNER JOIN Books bk ON bk.Id = b.BookId
                      WHERE b.UserId = @UserId
                      """;

            if (onlyActive)
            {
                sql += " AND b.ReturnDate IS NULL";
            }

            sql += " ORDER BY b.BorrowDate DESC;";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var rows = new List<BorrowingDetailsDto>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(MapBorrowingDetails(reader));
            }

            return rows;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while listing borrowings for user {UserId}.", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while listing borrowings for user {UserId}.", userId);
            throw;
        }
    }

    public async Task<BorrowAttempt> TryBorrowAsync(BorrowBookRequestDto request, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        SqlTransaction? transaction = null;

        try
        {
            await connection.OpenAsync(cancellationToken);
            transaction = connection.BeginTransaction();

            const string updateBookSql = """
                                         UPDATE Books
                                         SET AvailableCopies = AvailableCopies - 1
                                         WHERE Id = @BookId AND AvailableCopies > 0;
                                         """;

            using (var updateCommand = new SqlCommand(updateBookSql, connection, transaction))
            {
                updateCommand.Parameters.AddWithValue("@BookId", request.BookId);
                var affected = await updateCommand.ExecuteNonQueryAsync(cancellationToken);
                if (affected == 0)
                {
                    transaction.Rollback();
                    return new BorrowAttempt(BorrowAttemptStatus.NotAvailable, null);
                }
            }

            const string insertBorrowingSql = """
                                              INSERT INTO Borrowings (UserId, BookId, DueDate)
                                              OUTPUT INSERTED.Id
                                              VALUES (@UserId, @BookId, @DueDate);
                                              """;

            using (var insertCommand = new SqlCommand(insertBorrowingSql, connection, transaction))
            {
                insertCommand.Parameters.AddWithValue("@UserId", request.UserId);
                insertCommand.Parameters.AddWithValue("@BookId", request.BookId);
                insertCommand.Parameters.AddWithValue("@DueDate", request.DueDateUtc);

                var borrowingId = (int)await insertCommand.ExecuteScalarAsync(cancellationToken);
                transaction.Commit();
                return new BorrowAttempt(BorrowAttemptStatus.Success, borrowingId);
            }
        }
        catch (SqlException ex) when (IsUniqueViolation(ex))
        {
            try
            {
                transaction?.Rollback();
            }
            catch
            {
            }

            _logger.LogWarning(ex, "Duplicate active borrow prevented for user {UserId} book {BookId}.", request.UserId, request.BookId);
            return new BorrowAttempt(BorrowAttemptStatus.AlreadyBorrowed, null);
        }
        catch (SqlException ex)
        {
            try
            {
                transaction?.Rollback();
            }
            catch
            {
            }

            _logger.LogError(ex, "DB error while borrowing book {BookId} for user {UserId}.", request.BookId, request.UserId);
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                transaction?.Rollback();
            }
            catch
            {
            }

            _logger.LogError(ex, "Unexpected error while borrowing book {BookId} for user {UserId}.", request.BookId, request.UserId);
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    public async Task<ReturnAttempt> TryReturnAsync(ReturnBookRequestDto request, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();

        try
        {
            await connection.OpenAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            const string updateBorrowingSql = """
                                              UPDATE Borrowings
                                              SET ReturnDate = @ReturnDate
                                              OUTPUT INSERTED.BookId
                                              WHERE Id = @BorrowingId AND ReturnDate IS NULL;
                                              """;

            int? bookId;
            using (var updateCommand = new SqlCommand(updateBorrowingSql, connection, transaction))
            {
                updateCommand.Parameters.AddWithValue("@ReturnDate", DateTime.UtcNow);
                updateCommand.Parameters.AddWithValue("@BorrowingId", request.BorrowingId);

                var result = await updateCommand.ExecuteScalarAsync(cancellationToken);
                bookId = result is null || result is DBNull ? null : Convert.ToInt32(result);
            }

            if (bookId is null)
            {
                transaction.Rollback();
                return new ReturnAttempt(ReturnAttemptStatus.NotFoundOrAlreadyReturned);
            }

            const string updateBookSql = """
                                         UPDATE Books
                                         SET AvailableCopies = AvailableCopies + 1
                                         WHERE Id = @BookId;
                                         """;

            using (var updateBookCommand = new SqlCommand(updateBookSql, connection, transaction))
            {
                updateBookCommand.Parameters.AddWithValue("@BookId", bookId.Value);
                await updateBookCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();
            return new ReturnAttempt(ReturnAttemptStatus.Success);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while returning borrowing {BorrowingId}.", request.BorrowingId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while returning borrowing {BorrowingId}.", request.BorrowingId);
            throw;
        }
    }

    private static BorrowingDetailsDto MapBorrowingDetails(SqlDataReader reader)
    {
        var firstName = reader.GetString(reader.GetOrdinal("FirstName"));
        var lastName = reader.GetString(reader.GetOrdinal("LastName"));

        return new BorrowingDetailsDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserFullName = $"{firstName} {lastName}",
            BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
            BookTitle = reader.GetString(reader.GetOrdinal("BookTitle")),
            BorrowDateUtc = SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("BorrowDate"))),
            DueDateUtc = SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("DueDate"))),
            ReturnDateUtc = reader.IsDBNull(reader.GetOrdinal("ReturnDate"))
                ? null
                : SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("ReturnDate"))),
        };
    }

    //This to catch unique constraint exception
    private static bool IsUniqueViolation(SqlException ex) => ex.Number is 2601 or 2627;

    private static DateTime SpecifyUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
