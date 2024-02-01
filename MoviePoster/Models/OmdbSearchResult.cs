namespace MoviePoster.Models
{
    public class OmdbSearchResult
    {
        public List<OmdbMovie>? Search { get; set; }
        public string? TotalResults { get; set; }
        public string? Response { get; set; }
    }
}
