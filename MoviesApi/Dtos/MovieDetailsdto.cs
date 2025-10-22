namespace MoviesApi.Dtos
{
    public class MovieDetailsdto
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        public int Year { get; set; }
        public double Rate { get; set; }
        
        public string Storyline { get; set; }

        public byte[] Poster { get; set; }

        public byte GenreId { get; set; }

        public string GenreName { get; set; }
    }
}
