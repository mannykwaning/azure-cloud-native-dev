using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HW6MovieSharingSolution.Models;
using HW6MovieSharingSolution.Data;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class CreateModel : BasePageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateModel"/> class.
        /// </summary>
        /// <param name="context"></param>
        public CreateModel(HW6MovieSharingSolutionContext context) : base(context)
        {
        }

        /// <summary>
        /// Called on Get
        /// </summary>
        /// <returns>IActionResult</returns>
        public IActionResult OnGet()
        {
            return Page();
        }

        /// <summary>
        /// Movie getter/ setter
        /// </summary>
        [BindProperty]
        public Movie Movie { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        /// <summary>
        /// Post as async operation
        /// </summary>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Assign record user's obj id to filter records belonging to user
            Movie.UserRealmId = AuthenticatedUserInfo.ObjectIdentifier;

            Context.Movie.Add(Movie);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
