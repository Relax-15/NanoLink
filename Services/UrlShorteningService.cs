using Microsoft.EntityFrameworkCore;
using NanoLink.Models;

namespace NanoLink.Services;


    // handles generating unique codes.
    // This service generates a random string of the specified length using our predefined chars.
    // It checks against the database to ensure uniqueness.
public class UrlShorteningService(ApiDbContext context)
{
    private readonly Random _random = new();
    private readonly ApiDbContext _context = context;

    public async Task<string> GenerateUniqueCode()
    {
        var codeChars = new char[ShortLinkSettings.Length];

        int maxValue = ShortLinkSettings.Chars.Length;

        while (true)
        {
            for (var i = 0; i < ShortLinkSettings.Length; i++)
            {
                var randomIndex = _random.Next(maxValue);

                codeChars[i] = ShortLinkSettings.Chars[randomIndex];
            }
            var code = new string(codeChars);

            if (!await _context.ShortenedUrls.AnyAsync(s => s.Code == code))
            {
                return code;
            }
        }
    }
}

 
