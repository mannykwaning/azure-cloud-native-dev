using System.Security.Claims;

namespace HW6MovieSharingSolution
{
    /// <summary>
    /// Helper class to access user data stored in claims principal
    /// </summary>
    public static class ClaimsPricipalExtensions
    {
        /// <summary>
        /// Objects the identifier.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <returns>System.String.</returns>
        public static string ObjectIdentifier(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? string.Empty;
        }

        public static string DisplayName(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst("name")?.Value ?? string.Empty;
        }
    }

    public class DecodedClaims
    {
        /// <summary>
        /// The claims principal
        /// </summary>
        private readonly ClaimsPrincipal _claimsPrincipal;

        public DecodedClaims(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;
        }

        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        /// <value>The object identifier.</value>
        public string ObjectIdentifier
        {
            get
            {
                return _claimsPrincipal.ObjectIdentifier();
            }
        }
    }
}
