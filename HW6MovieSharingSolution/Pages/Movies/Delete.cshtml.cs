using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;
using System.Linq;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class DeleteModel : BasePageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteModel"/> class.
        /// </summary>
        /// <param name="context"></param>
        public DeleteModel(HW6MovieSharingSolutionContext context) : base(context)
        {
        }

        /// <summary>
        /// Movie Getter/ Setter
        /// </summary>
        [BindProperty]
        public Movie Movie { get; set; }

        /// <summary>
        /// On Get as Async operation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await Context.Movie.SingleOrDefaultAsync(
                m => m.Id == id
                && m.UserRealmId == AuthenticatedUserInfo.ObjectIdentifier);

            return (Movie == null) ? NotFound() : Page();
        }

        /// <summary>
        /// Async Post Operation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie = await (from m in Context.Movie
                           where m.Id == id
                           && m.UserRealmId == AuthenticatedUserInfo.ObjectIdentifier
                           select m).SingleOrDefaultAsync();

            if (Movie != null)
            {
                Context.Movie.Remove(Movie);
                await Context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
