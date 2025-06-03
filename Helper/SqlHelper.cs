using System.Data;
using Microsoft.Data.SqlClient;

namespace aspNetCoreMvc.Helper;

public class SqlHelper
{
    private readonly string _connectionString;

    public SqlHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    //* Tanpa hasil (non-generic)
    public async Task UseConnectionAsync(Func<SqlConnection, Task> action)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await action(connection);
        }
    }

    //* Dengan hasil balik (generic)
    public async Task<T> UseConnectionAsync<T>(Func<SqlConnection, Task<T>> action)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await action(connection);
        }
    }

    //* Jalankan Stored Procedure tanpa parameter
    public async Task<SqlDataReader> ExecuteStoredProcedureAsync(string procedureName)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        //* Gunakan CommandBehavior.CloseConnection agar koneksi otomatis tertutup saat reader di-close
        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }

    //* Jalankan Stored Procedure dengan parameter dan hasil balik reader
    public async Task<SqlDataReader> ExecutedStoredProcedureAsync(string procedureName, Action<SqlParameterCollection> addParams)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        addParams(command.Parameters);

        //* Gunakan CommandBehavior.CloseConnection agar koneksi otomatis tertutup saat reader di-close
        return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
    }
}
