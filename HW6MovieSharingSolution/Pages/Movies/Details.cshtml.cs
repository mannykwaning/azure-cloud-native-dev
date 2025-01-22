using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class DetailsModel : BasePageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsModel"/> class
        /// </summary>
        /// <param name="context"></param>
        public DetailsModel(HW6MovieSharingSolution.Data.HW6MovieSharingSolutionContext context) : base(context)
        {
        }

        /// <summary>
        /// Movie getter/ setter
        /// </summary>
        public Movie Movie { get; set; }

        /// <summary>
        /// Async get operation
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

            if (Movie == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
