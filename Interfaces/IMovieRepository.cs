using aspNetCoreMvc.Models;

namespace aspNetCoreMvc.Interfaces;

public interface IMovieRepository
{
    Task<List<Movie>> GetAllMoviesAsync();
    Task<bool> IsMovieTableEmptyAsync();
}
