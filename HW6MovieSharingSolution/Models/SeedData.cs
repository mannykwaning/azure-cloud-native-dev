using HW6MovieSharingSolution.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HW6MovieSharingSolution.Models
{
    /// <summary>
    /// Initialize/ Seed data to database
    /// </summary>
    public class SeedData
    {
        public static void Inititialize(IServiceProvider serviceProvider)
        {
            using (var context = new HW6MovieSharingSolutionContext(serviceProvider.GetRequiredService<
                    DbContextOptions<HW6MovieSharingSolutionContext>>()))
            {
                if (context.Movie.Any())
                {
                    return;
                }
                context.Movie.AddRange(new Movie
                {
                    Category = "Action",
                    SharedDate = new DateTime(2020, 12, 01),
                    SharedWithEmailAddress = "Allissa@amazon.com",
                    SharedWithName = "Allissa",
                    Title = "Enter The Matrix",
                    CanBeShared = true,
                    isApproved = false,
                    UserRealmId = "123-456-798abcd",
                    AprovalStatus = "Available",
                    Owner = "Manny Aboah",
                    OwnerEmail = "Test@email.com",
                    SharedUserRealmId = "aaa_1234-bbb-05589c9"
                });
                context.SaveChanges();
            }
        }
    }
}
