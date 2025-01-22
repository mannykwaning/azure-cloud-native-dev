using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class LibraryModel : BasePageModel
    {
        /// <summary>
        /// Initializes <see cref="IndexModel"/> class
        /// </summary>
        /// <param name="context"></param>
        public LibraryModel(HW6MovieSharingSolutionContext context) : base(context) { }

        /// <summary>
        /// List of user's movies
        /// </summary>
        public IList<Movie> Movie { get; set; }

        /// <summary>
        /// List of borrowed Movies
        /// </summary>
        public IList<Movie> BorrowedMovies { get; set; }

        /// <summary>
        /// Copy Object data
        /// </summary>
        private static List<Movie> _copyMovies = new();

        /// <summary>
        /// Async on get
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            Movie = await Context.Movie.Where(
                _ => _.UserRealmId == AuthenticatedUserInfo.ObjectIdentifier).ToListAsync();

            BorrowedMovies = await Context.Movie.Where(
                _ => _.SharedUserRealmId == AuthenticatedUserInfo.ObjectIdentifier
                && _.isApproved == true).ToListAsync();

            BorrowedMovies.ToList().ForEach(_ =>
                 _copyMovies.Add(new Movie()
                 {
                     Id = _.Id,
                     UserRealmId = _.UserRealmId,
                     Owner = _.Owner,
                     OwnerEmail = _.OwnerEmail,
                     CanBeShared = _.CanBeShared,
                     Category = _.Category,
                     Title = _.Title,
                     AprovalStatus = _.AprovalStatus,
                     SharedDate = new DateTime()
                 })
            );
        }

        public async Task<IActionResult> OnPostReturnAsync(long? id)
        {
            System.Console.WriteLine(id);

            Movie tempMovie = _copyMovies.Where(m => m.Id == id).FirstOrDefault();

            tempMovie.AprovalStatus = "Available";
            tempMovie.isApproved = false;

            Context.Attach(tempMovie).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Library");
        }

        /// <summary>
        /// Checks if Movie Exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool MovieExists(long? id)
        {
            return Context.Movie.Any(e => e.Id == id);
        }
    }
}
