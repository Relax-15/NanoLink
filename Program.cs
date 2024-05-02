
using Microsoft.EntityFrameworkCore;
using NanoLink.DTOs;
using NanoLink.Models;
using NanoLink.Services;
using System;

namespace NanoLink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:4200",
                            "chrome-extension://fiijjhbifgdgdnnfcdibkjnnegkgbfgg") // Specify the allowed origin(s)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connectionString));

            builder.Services.AddScoped<UrlShorteningService>();
            var app = builder.Build();

            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.MapPost("/shorturl", async (
                UrlRequestDto request, ApiDbContext context,
                HttpContext http, UrlShorteningService urlService) =>
            {
                //validating the input URL
                //if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
                //    return Results.BadRequest("Invalid URL specified");

                // Create the short version of URL

                var code = await urlService.GenerateUniqueCode();

                var shortUrl = new ShortenedUrl()
                {
                    Id = Guid.NewGuid(),
                    Url = request.Url,
                    Code = code,
                    ShortUrl = $"{http.Request.Scheme}://{http.Request.Host}/{code}",
                    CreatedOnUtc = DateTime.UtcNow
                };

                // save to DB
                context.ShortenedUrls.Add(shortUrl);
                await context.SaveChangesAsync();

                // construct the url
                var result = $"{http.Request.Scheme}://{http.Request.Host}/{shortUrl.ShortUrl}";

                return Results.Ok(shortUrl.ShortUrl);
            }
            );

            app.MapGet("{code}", async (string code, ApiDbContext context) =>
            {
                var shortenedUrl = await context
                    .ShortenedUrls
                    .SingleOrDefaultAsync(s => s.Code == code);

                if (shortenedUrl is null)
                {
                    return Results.NotFound();
                }

                return Results.Redirect(shortenedUrl.Url);
            });

            app.MapGet("/allUrls", async (ApiDbContext db) =>  await db.ShortenedUrls.ToListAsync());


            //app.MapFallback(async (ApiDbContext context, HttpContext http) =>
            //{
            //    var path = http.Request.Path.ToUriComponent().Trim('/');

            //    var urlMatch = await context.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortUrl.Trim() == path.Trim());

            //    if (urlMatch == null)
            //    {
            //        return Results.BadRequest("Invalid url");
            //    }

            //    return Results.Redirect(urlMatch.Url);
            //});

            app.UseCors("AllowSpecificOrigin");
            app.Run();
        }
        
    }
}

