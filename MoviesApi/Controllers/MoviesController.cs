using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1 m");
            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
            {
                return BadRequest("Invalid genre ID!");
            }
            using var datastream = new MemoryStream();
            await dto.Poster.CopyToAsync(datastream);
            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Poster = datastream.ToArray(),
                Rate = dto.Rate,
                Storyline = dto.Storyline,
                Year = dto.Year
            };
            await _context.AddAsync(movie);
            _context.SaveChanges();

            return Ok(movie);
        }
        [HttpGet("GetAllMovies")]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                .OrderByDescending(x => x.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsdto
                {
                    Id = m.Id,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    Poster = m.Poster,
                    Rate = m.Rate,
                    Storyline = m.Storyline,
                    Title = m.Title,
                    Year = m.Year
                })
                .ToListAsync();
            return Ok(movies);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return BadRequest("You entered wrong Id");

            var dto = new MovieDetailsdto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre?.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                Storyline = movie.Storyline,
                Title = movie.Title,
                Year = movie.Year
            };
            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreid)
        {
            var movies = await _context.Movies
                .Where(m => m.GenreId == genreid)
                .OrderByDescending(x => x.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsdto
                {
                    Id = m.Id,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    Poster = m.Poster,
                    Rate = m.Rate,
                    Storyline = m.Storyline,
                    Title = m.Title,
                    Year = m.Year
                })
                .ToListAsync();
            return Ok(movies);

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound($"No movie was found with ID: {id}");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest("Invalid genre ID!");

            // Check if a new poster was uploaded
            if (dto.Poster != null)
            {
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1 MB");

                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);
                movie.Poster = dataStream.ToArray();
            }

            // Update other fields
            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.Rate = dto.Rate;
            movie.Storyline = dto.Storyline;
            movie.GenreId = dto.GenreId;

            await _context.SaveChangesAsync();

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return BadRequest($"The id is not Found :{id}");
            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return Ok();
        }
     }
}
