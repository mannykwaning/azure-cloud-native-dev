using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class ReviewModel : BasePageModel
    {
        public ReviewModel(HW6MovieSharingSolutionContext context) : base(context)
        {

        }

        /// <summary>
        /// Movie getter/ setter
        /// </summary>
        [BindProperty]
        public Movie Movie { get; set; }

        /// <summary>
        /// Copy Object data
        /// </summary>
        private static Movie _copyMovie = new();

        /// <summary>
        /// Async OnGet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find movie in current user's realm via id
            Movie = await Context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            _copyMovie = new Movie()
            {
                Id = Movie.Id,
                UserRealmId = Movie.UserRealmId,
                Owner = Movie.Owner,
                OwnerEmail = Movie.OwnerEmail,
                CanBeShared = Movie.CanBeShared,
                Category = Movie.Category,
                SharedUserRealmId = Movie.SharedUserRealmId,
                SharedWithEmailAddress = Movie.SharedWithEmailAddress,
                SharedWithName = Movie.SharedWithName,
                Title = Movie.Title,
            };

            return (Movie == null) ? NotFound() : Page();
        }

        /// Async post operation - For approved requests
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostApproveAsync()
        {
            _copyMovie.AprovalStatus = "Approved";
            _copyMovie.isApproved = true;
            _copyMovie.SharedDate = Movie.SharedDate;

            Context.Attach(_copyMovie).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(Movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        /// Async post operation - For rejected requests
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRejectAsync()
        {
            Movie.AprovalStatus = "Rejected";

            Context.Attach(Movie).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(Movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        /// <summary>
        /// Checks if Movie Exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool MovieExists(long id)
        {
            return Context.Movie.Any(e => e.Id == id);
        }
    }
}
