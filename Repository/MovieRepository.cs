using System.Data;
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

    public async Task<bool> CreateMovie(Movie movie)
    {
        int movieId = 0;

        using (var reader = await _sqlHelper.ExecutedStoredProcedureAsync("CreateMovie", parameter =>
        {
            parameter.AddWithValue("@Title", movie.Title);
            parameter.AddWithValue("@ReleaseDate", movie.ReleaseDate);
            parameter.AddWithValue("@Genre", movie.Genre);
            parameter.AddWithValue("@Price", movie.Price);
            parameter.AddWithValue("@Rating", movie.Rating);
        }))
        {
            if (await reader.ReadAsync())
            {
                movieId = Convert.ToInt32(reader["Id"]);
            }
        }

        return movieId > 0;
    }

    public async Task<bool> DeleteMovie(int? id)
    {
        int affectedRows = 0;

        await _sqlHelper.UseConnectionAsync(async connection =>
        {
            using (var command = new SqlCommand("DeleteMovie", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", (object?)id ?? DBNull.Value);

                affectedRows = await command.ExecuteNonQueryAsync();
            }
        });

        return affectedRows > 0; //* true jika ada baris yang terhapus
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

    public async Task<bool> IsAMovieExists(int? id)
    {
        bool exists = false;

        await _sqlHelper.UseConnectionAsync(async connection =>
        {
            using (var command = new SqlCommand("CheckMovieExists", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", (object?)id ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                exists = result != null && Convert.ToBoolean(result);
            }
        });

        return exists;
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

    public async Task<bool> UpdateMovie(Movie movie)
    {
        int movieId = 0;

        using (var reader = await _sqlHelper.ExecutedStoredProcedureAsync("UpdateMovie", parameter =>
        {
            parameter.AddWithValue("@Id", movie.Id);
            parameter.AddWithValue("@Title", movie.Title);
            parameter.AddWithValue("@ReleaseDate", movie.ReleaseDate);
            parameter.AddWithValue("@Genre", movie.Genre);
            parameter.AddWithValue("@Price", movie.Price);
            parameter.AddWithValue("@Rating", movie.Rating);
        }))
        {
            if (await reader.ReadAsync())
            {
                movieId = Convert.ToInt32(reader["Id"]);
            }
        }

        return movieId > 0;
    }
}
