using aspNetCoreMvc.Helper;
using aspNetCoreMvc.Interfaces;
using aspNetCoreMvc.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace aspNetCoreMvc.Repository;

public class MovieRepository : IMovieRepository
{
    private readonly SqlHelper _sqlHelper;

    public MovieRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("aspNetCoreMvcContext");
        _sqlHelper = new SqlHelper(connectionString!);
    }

    //* implementasi pengambilan data menggunakan stored procedure dan ADO.NET
    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        return await _sqlHelper.UseConnectionAsync(async connection =>
        {
            var movies = new List<Movie>();
            var command = new SqlCommand("GetAllMovies", connection);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    movies.Add(new Movie
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        ReleaseDate = reader.GetDateTime(2),
                        Genre = reader.GetString(3),
                        Price = reader.GetDecimal(4),
                        Rating = reader.GetString(5)
                    });
                }
            }

            return movies;
        });
    }

    public async Task<bool> IsMovieTableEmptyAsync()
    {
        return await _sqlHelper.UseConnectionAsync(async connection =>
        {
            var command = new SqlCommand("IsMovieTableExist", connection);
            var count = await command.ExecuteScalarAsync();
            return (int)count! == 0;
        });
    }
}
