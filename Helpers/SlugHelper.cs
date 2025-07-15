using PrimeGames.Extensions;

namespace PrimeGames.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateUniqueSlug(string baseSlug, Func<string, bool> slugExists)
        {
            string slug = baseSlug.ToSlug();

            if (string.IsNullOrEmpty(slug))
                slug = "article";

            string originalSlug = slug;
            int counter = 1;

            while (slugExists(slug))
            {
                slug = $"{originalSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}