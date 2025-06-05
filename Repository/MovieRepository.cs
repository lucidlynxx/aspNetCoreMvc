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

    public async Task<List<string>> GetAllGenresAsync()
    {
        var genres = new List<string>();

        using (var reader = await _sqlHelper.ExecuteStoredProcedureAsync("GetAllGenres"))
        {
            while (await reader.ReadAsync())
            {
                genres.Add(reader.GetString(0));
            }
        }

        return genres;
    }

    //* implementasi pengambilan data menggunakan stored procedure dan ADO.NET
    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        var movies = new List<Movie>();

        using (var reader = await _sqlHelper.ExecuteStoredProcedureAsync("GetAllMovies"))
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
    }

    public async Task<Movie> GetMovieByIdAsync(int? id)
    {
        Movie movie = new Movie();

        using (var reader = await _sqlHelper.ExecutedStoredProcedureAsync("GetMovieById", parameter =>
        {
            parameter.AddWithValue("@Id", (object?)id ?? DBNull.Value);
        }
        ))
        {
            if (await reader.ReadAsync())
            {
                movie.Id = reader.GetInt32(0);
                movie.Title = reader.GetString(1);
                movie.ReleaseDate = reader.GetDateTime(2);
                movie.Genre = reader.GetString(3);
                movie.Price = reader.GetDecimal(4);
                movie.Rating = reader.GetString(5);
            }
        }

        return movie;
    }

    public async Task<List<Movie>> GetMoviesByTitleOrGenreAsync(string? searchString)
    {
        List<Movie> movies = new List<Movie>();

        using (var reader = await _sqlHelper.ExecutedStoredProcedureAsync("SearchMovie", parameter =>
        {
            parameter.AddWithValue("@Title", (object?)searchString ?? DBNull.Value);
            parameter.AddWithValue("@Genre", (object?)searchString ?? DBNull.Value);
        }))
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
