using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class BorrowModel : BasePageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BorrowModel"/> class.
        /// </summary>
        /// <param name="context"></param>
        public BorrowModel(HW6MovieSharingSolutionContext context) : base(context)
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
                Title = Movie.Title,
            };

            return (Movie == null) ? NotFound() : Page();
        }

        /// Async post operation
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Movie.SharedWithName)
                || string.IsNullOrEmpty(Movie.SharedWithEmailAddress))
            {
                return Page();
            }
            _copyMovie.SharedWithName = Movie.SharedWithName;
            _copyMovie.SharedWithEmailAddress = Movie.SharedWithEmailAddress;
            _copyMovie.AprovalStatus = "Requested";
            _copyMovie.SharedUserRealmId = AuthenticatedUserInfo.ObjectIdentifier;

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
