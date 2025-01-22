using HW6MovieSharingSolution.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HW6MovieSharingSolution.Pages
{
    public class BasePageModel : PageModel
    {
        /// <summary>
        /// Gets the Context
        /// </summary>
        protected HW6MovieSharingSolutionContext Context { get;  }

        private DecodedClaims _decodedClaims = null;

        public  DecodedClaims AuthenticatedUserInfo
        {
            get
            {
                return _decodedClaims ?? new DecodedClaims(this.User);
            }
        }

        public BasePageModel(HW6MovieSharingSolutionContext context)
        {
            Context = context;
        }
    }
}
