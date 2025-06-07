using aspNetCoreMvc.Models;

namespace aspNetCoreMvc.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllMoviesAsync();
    Task<bool> IsMovieTableEmptyAsync();
    Task<List<string>> GetAllGenresAsync();
    Task<List<Movie>> GetMoviesByTitleOrGenreAsync(string? searchString);
    Task<Movie> GetMovieByIdAsync(int? id);
    Task<bool> CreateMovie(Movie movie);
    Task<bool> UpdateMovie(Movie movie);
}
