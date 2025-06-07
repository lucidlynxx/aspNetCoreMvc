using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using aspNetCoreMvc.Data;
using aspNetCoreMvc.Models;
using Rotativa.AspNetCore;
using ClosedXML.Excel;
using aspNetCoreMvc.Interfaces;

namespace aspNetCoreMvc.Controllers
{
    public class MoviesController : Controller
    {
        private readonly aspNetCoreMvcContext _context;
        private readonly IMovieRepository _movieRepository;

        public MoviesController(aspNetCoreMvcContext context, IMovieRepository movieRepository)
        {
            _context = context;
            _movieRepository = movieRepository;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            if (await _movieRepository.IsMovieTableEmptyAsync())
                return Problem("Entity set 'aspNetCoreMvcContext.Movie' is null.");

            //* implementasi pengambilan data menggunakan stored procedure dan ADO.NET
            List<Movie> movies = await _movieRepository.GetAllMoviesAsync();

            List<string> genres = movies.Select(m => m.Genre!).Distinct().ToList();

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(m => m.Title!.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(m => m.Genre == movieGenre).ToList();
            }

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(genres),
                Movies = movies
            };

            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _movieRepository.GetMovieByIdAsync(id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                await _movieRepository.CreateMovie(movie);
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Movie movie = await _movieRepository.GetMovieByIdAsync(id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (id != movie.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _movieRepository.UpdateMovie(movie);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _movieRepository.IsAMovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);

            if (movie != null)
                _context.Movie.Remove(movie);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadPdf()
        {
            //* implementasi pengambilan data menggunakan stored procedure dan ADO.NET
            List<Movie> movies = await _movieRepository.GetAllMoviesAsync();

            var fileName = $"movie_list_{DateTime.Now:yyyy_MM_dd_HH:mm}.pdf";

            return new ViewAsPdf("PdfTemplate", movies)
            {
                FileName = fileName,
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }

        public async Task<IActionResult> DownloadExcel()
        {
            //* implementasi pengambilan data menggunakan stored procedure dan ADO.NET
            List<Movie> movies = await _movieRepository.GetAllMoviesAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Movie List");
            var currentRow = 1;

            //* Header
            worksheet.Cell(currentRow, 1).Value = "No";
            worksheet.Cell(currentRow, 2).Value = "Title";
            worksheet.Cell(currentRow, 3).Value = "Release Date";
            worksheet.Cell(currentRow, 4).Value = "Genre";
            worksheet.Cell(currentRow, 5).Value = "Price";
            worksheet.Cell(currentRow, 6).Value = "Rating";

            //* Data
            for (int i = 0; i < movies.Count; i++)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = i + 1;
                worksheet.Cell(currentRow, 2).Value = movies[i].Title;
                worksheet.Cell(currentRow, 3).Value = movies[i].ReleaseDate.ToString("dd MMM yyyy");
                worksheet.Cell(currentRow, 4).Value = movies[i].Genre;
                worksheet.Cell(currentRow, 5).Value = movies[i].Price;
                worksheet.Cell(currentRow, 6).Value = movies[i].Rating;
            }

            //* Download
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"movie_list_{DateTime.Now:yyyy_MM_dd_HH:mm}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
