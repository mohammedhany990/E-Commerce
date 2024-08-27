using Demo.DAL.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Demo.DAL.Data.Contexts
{
    public static class ECommerceDbContextSeeding
    {
        public static async Task DataSeedAsync(ECommerceDbContext dbContext,
            RoleManager<IdentityRole> userRoleManager,
            UserManager<AppUser> userManager)
        {
            /*
            if (dbContext.Categories.Count() == 0)
            {
                var categoriesData = File.ReadAllText("../Demo.DAL/Data/DataSeeding/Category.json");
                var categories = JsonSerializer.Deserialize<List<Category>>(categoriesData);
                if (categories?.Count() > 0)
                {
                    foreach (var category in categories)
                    {
                        dbContext.Set<Category>().AddAsync(category);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }*/

            if (!userRoleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                userRoleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                userRoleManager.CreateAsync(new IdentityRole("Company")).GetAwaiter().GetResult();
                userRoleManager.CreateAsync(new IdentityRole("Employee")).GetAwaiter().GetResult();
                userRoleManager.CreateAsync(new IdentityRole("Customer")).GetAwaiter().GetResult();

                var newAdmin = new AppUser()
                {
                    UserName = "MrAdmin@gmail.com",
                    Email = "MrAdmin@gmail.com",
                    PhoneNumber = "0109900999",
                    Name = "Hany",
                    StreetAddress = "123 TS",
                    City = "NYC",
                    State = "USA",
                    PostalCode = "123546"
                };
                await userManager.CreateAsync(newAdmin, "Admin@1234");
                var user = dbContext.AppUsers.FirstOrDefault(u => u.Email == newAdmin.Email);
                await userManager.AddToRoleAsync(user, "Admin");
            }

           
        }
    }
}
