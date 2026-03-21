using System.Text;
using Library.BL.Dtos;
using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.DAL.Db;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Library.DAL.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(ISqlConnectionFactory connectionFactory, ILogger<BookRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BookSearchRow>> SearchAsync(BookSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            search ??= new BookSearchDto();

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var sql = BuildSearchSql(search, out var parameters);
            using var command = new SqlCommand(sql, connection);
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            var rows = new List<BookSearchRow>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(MapBookSearchRow(reader));
            }

            return rows;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while searching books.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while searching books.");
            throw;
        }
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               SELECT Id, Title, Author, ISBN, TotalCopies, AvailableCopies, Description, LanguageId, GenreId, CreatedAt
                               FROM Books
                               WHERE Id = @Id;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapBook(reader);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while reading book by id {BookId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reading book by id {BookId}.", id);
            throw;
        }
    }

    public async Task<int> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               INSERT INTO Books (Title, Author, ISBN, TotalCopies, AvailableCopies, Description, LanguageId, GenreId)
                               OUTPUT INSERTED.Id
                               VALUES (@Title, @Author, @ISBN, @TotalCopies, @AvailableCopies, @Description, @LanguageId, @GenreId);
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Author", book.Author);
            command.Parameters.AddWithValue("@ISBN", book.Isbn);
            command.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
            command.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);
            command.Parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@LanguageId", book.LanguageId);
            command.Parameters.AddWithValue("@GenreId", book.GenreId);

            var insertedId = (int)await command.ExecuteScalarAsync(cancellationToken);
            return insertedId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while adding book.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding book.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                               UPDATE Books
                               SET Title = @Title,
                                   Author = @Author,
                                   ISBN = @ISBN,
                                   TotalCopies = @TotalCopies,
                                   AvailableCopies = @AvailableCopies,
                                   Description = @Description,
                                   LanguageId = @LanguageId,
                                   GenreId = @GenreId
                               WHERE Id = @Id;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", book.Id);
            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Author", book.Author);
            command.Parameters.AddWithValue("@ISBN", book.Isbn);
            command.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
            command.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);
            command.Parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@LanguageId", book.LanguageId);
            command.Parameters.AddWithValue("@GenreId", book.GenreId);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while updating book {BookId}.", book.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating book {BookId}.", book.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = "DELETE FROM Books WHERE Id = @Id;";
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "DB error while deleting book {BookId}.", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting book {BookId}.", id);
            throw;
        }
    }

    private static string BuildSearchSql(BookSearchDto search, out List<SqlParameter> parameters)
    {
        var sql = new StringBuilder();
        sql.AppendLine("""
                       SELECT
                           b.Id,
                           b.Title,
                           b.Author,
                           b.ISBN,
                           b.TotalCopies,
                           b.AvailableCopies,
                           b.Description,
                           b.LanguageId,
                           l.Name AS LanguageName,
                           b.GenreId,
                           g.Name AS GenreName,
                           b.CreatedAt
                       FROM Books b
                       INNER JOIN Languages l ON l.Id = b.LanguageId
                       INNER JOIN Genres g ON g.Id = b.GenreId
                       WHERE 1 = 1
                       """);

        parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search.Title))
        {
            sql.AppendLine("AND b.Title LIKE '%' + @Title + '%'");
            parameters.Add(new SqlParameter("@Title", search.Title));
        }

        if (!string.IsNullOrWhiteSpace(search.Author))
        {
            sql.AppendLine("AND b.Author LIKE '%' + @Author + '%'");
            parameters.Add(new SqlParameter("@Author", search.Author));
        }

        if (!string.IsNullOrWhiteSpace(search.Isbn))
        {
            sql.AppendLine("AND b.ISBN = @ISBN");
            parameters.Add(new SqlParameter("@ISBN", search.Isbn));
        }

        var page = search.Page <= 0 ? 1 : search.Page;
        var items = search.ItemsPerPage <= 0 ? 10 : search.ItemsPerPage;
        if (items > 100)
        {
            items = 100;
        }

        var offset = (page - 1) * items;
        parameters.Add(new SqlParameter("@Offset", offset));
        parameters.Add(new SqlParameter("@Fetch", items));

        sql.AppendLine("ORDER BY b.Title OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;");
        return sql.ToString();
    }

    private static BookSearchRow MapBookSearchRow(SqlDataReader reader)
    {
        return new BookSearchRow
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Isbn = reader.GetString(reader.GetOrdinal("ISBN")),
            TotalCopies = reader.GetInt32(reader.GetOrdinal("TotalCopies")),
            AvailableCopies = reader.GetInt32(reader.GetOrdinal("AvailableCopies")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            LanguageId = reader.GetInt32(reader.GetOrdinal("LanguageId")),
            LanguageName = reader.GetString(reader.GetOrdinal("LanguageName")),
            GenreId = reader.GetInt32(reader.GetOrdinal("GenreId")),
            GenreName = reader.GetString(reader.GetOrdinal("GenreName")),
            CreatedAt = SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("CreatedAt"))),
        };
    }

    private static Book MapBook(SqlDataReader reader)
    {
        return new Book
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Isbn = reader.GetString(reader.GetOrdinal("ISBN")),
            TotalCopies = reader.GetInt32(reader.GetOrdinal("TotalCopies")),
            AvailableCopies = reader.GetInt32(reader.GetOrdinal("AvailableCopies")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            LanguageId = reader.GetInt32(reader.GetOrdinal("LanguageId")),
            GenreId = reader.GetInt32(reader.GetOrdinal("GenreId")),
            CreatedAt = SpecifyUtc(reader.GetDateTime(reader.GetOrdinal("CreatedAt"))),
        };
    }

    private static DateTime SpecifyUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
