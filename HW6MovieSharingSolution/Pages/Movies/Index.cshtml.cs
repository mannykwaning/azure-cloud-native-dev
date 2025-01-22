using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HW6MovieSharingSolution.Data;
using HW6MovieSharingSolution.Models;
using Microsoft.Extensions.Logging;

namespace HW6MovieSharingSolution.Pages.Movies
{
    public class IndexModel : BasePageModel
    {
        /// <summary>
        /// Initializes <see cref="IndexModel"/> class
        /// </summary>
        /// <param name="context"></param>
        public IndexModel(HW6MovieSharingSolutionContext context) : base(context) { }

        /// <summary>
        /// Movie Getter/ Setter
        /// </summary>
        public IList<Movie> Movie { get; set; }

        public string AuthUser { get; set; }

        /// <summary>
        /// Async on get
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            AuthUser = AuthenticatedUserInfo.ObjectIdentifier;

            Movie = (AuthUser != null) ?
                await Context.Movie.Where(
                _ => (_.UserRealmId == AuthUser
                || _.CanBeShared == true)
                && (_.AprovalStatus.ToLower().Equals("available"))).ToListAsync()
                : null;
        }
    }
}
