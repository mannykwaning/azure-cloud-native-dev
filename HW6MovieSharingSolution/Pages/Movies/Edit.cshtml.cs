using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HW6MovieSharingSolution.Models;
using HW6MovieSharingSolution.Data;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class EditModel : BasePageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditModel"/> class.
        /// </summary>
        /// <param name="context"></param>
        public EditModel(HW6MovieSharingSolutionContext context) : base(context)
        {
        }

        /// <summary>
        /// Movie getter/ setter
        /// </summary>
        [BindProperty]
        public Movie Movie { get; set; }

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
            Movie = await Context.Movie.FirstOrDefaultAsync(
                m => m.Id == id
                && m.UserRealmId == AuthenticatedUserInfo.ObjectIdentifier);

            return (Movie == null) ? NotFound() : Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// Async post operation
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            bool isValid = !(string.IsNullOrEmpty(Movie.Title)
                && string.IsNullOrEmpty(Movie.Category)
                && string.IsNullOrEmpty(Movie.AprovalStatus));
            
            if (!isValid)
            {
                return Page();
            }

            if(Movie.UserRealmId != AuthenticatedUserInfo.ObjectIdentifier)
            {
                ModelState.AddModelError("realm", "This movie cannot be updated because " +
                                         "current user does not have access to it");
                return Page();
            }

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
            return Context.Movie.Any(e => e.Id == id
                                     && e.UserRealmId == AuthenticatedUserInfo.ObjectIdentifier);
        }
    }
}
