using Bogus;
using DataAccessLayer.Models;

namespace DataAccessLayer.Data.Seeders
{
    public static class CategorySeeder
    {
        public static async Task SeedCategories(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var faker = new Faker<Category>()
                    .RuleFor(c => c.Name, f => f.Commerce.Department());

                var categories = faker.Generate(10); // Generate 10 fake categories

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}