
using Microsoft.EntityFrameworkCore;
using NanoLink.DTOs;
using NanoLink.Models;

namespace NanoLink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connectionString));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            app.MapPost("/shorturl", async (UrlDto urlDto, ApiDbContext context, HttpContext http) =>
            {
                //validating the input URL
                if (!Uri.TryCreate(urlDto.Url, UriKind.Absolute, out var inputUri))
                    return Results.BadRequest("Invalid URL");
                
                // CReate the short version of URL
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var randomString = new string(Enumerable.Repeat(chars, 6)
                    .Select(x => x[random.Next(x.Length)]).ToArray());

                // Map the short url with long one
                var shortUrl = new ShortenedUrl()
                {
                    Url = urlDto.Url,
                    ShortUrl = randomString
                };

                // save to DB
                context.ShortenedUrls.Add(shortUrl);
                context.SaveChangesAsync();
                
                // construct the url
                var result = $"{http.Request.Scheme}://{http.Request.Host}/{shortUrl.ShortUrl}";

                return Results.Ok(new UrlResponseDto()
                {
                    ShortUrl = result
                });
                
            });

            app.MapFallback(async (ApiDbContext context, HttpContext http) =>
            {
                var path = http.Request.Path.ToUriComponent().Trim('/');

                var urlMatch = await context.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortUrl.Trim() == path.Trim());

                if (urlMatch == null) 
                {
                    return Results.BadRequest("Invalid url");  
                }

                return Results.Redirect(urlMatch.Url);
            });
            
            app.Run();
        }
    }
}
