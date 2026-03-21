using Microsoft.Data.SqlClient;

namespace Library.DAL.Db;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}

