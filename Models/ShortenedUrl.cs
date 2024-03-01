namespace NanoLink.Models
{
    public class ShortenedUrl
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
    }
}
