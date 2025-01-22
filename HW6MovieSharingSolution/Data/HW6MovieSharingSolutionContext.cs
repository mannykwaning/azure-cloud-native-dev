using HW6MovieSharingSolution.Models;
using Microsoft.EntityFrameworkCore;

namespace HW6MovieSharingSolution.Data
{
    /// <summary>
    /// Model - Movie : Database Context
    /// </summary>
    public class HW6MovieSharingSolutionContext : DbContext
    {
        public HW6MovieSharingSolutionContext (DbContextOptions<HW6MovieSharingSolutionContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; }
    }
}
