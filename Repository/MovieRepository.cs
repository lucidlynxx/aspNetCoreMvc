using System.Data;
using aspNetCoreMvc.Interfaces;
using aspNetCoreMvc.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace aspNetCoreMvc.Repository;

public class MovieRepository : IMovieRepository
{
    private readonly string? _connectionString;

    public MovieRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("aspNetCoreMvcContext");
    }

    // implementasi pengambilan data menggunakan stored procedure dan ADO.NET
    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        var movies = new List<Movie>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand("GetAllMovies", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var movie = new Movie
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Genre = reader.GetString(2),
                            ReleaseDate = reader.GetDateTime(3),
                            Price = reader.GetDecimal(4),
                            Rating = reader.GetString(5)
                        };

                        movies.Add(movie);
                    }
                }
            }
        }

        return movies;
    }
}
