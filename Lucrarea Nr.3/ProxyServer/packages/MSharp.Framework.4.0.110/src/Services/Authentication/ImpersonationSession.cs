namespace MSharp.Framework.Services
{
    using System;
    using System.Security.Principal;
    using System.Web;
    using MSharp.Framework;
    using MSharp.Framework.Services.Globalization;

    /// <summary>
    /// Defines an admin user who can impersonate other users.
    /// </summary>
    public interface IImpersonator : IUser, IIdentity, IPrincipal
    {
        /// <summary>
        /// A unique single-use-only cookie-based token to specify the currently impersonated user session.
        /// </summary>
        string ImpersonationToken { get; set; }

        /// <summary>
        /// Determines if this user can impersonate the specified other user.
        /// </summary>
        bool CanImpersonate(IUser user);
    }

    /// <summary>
    /// Provides the business logic for ImpersonationContext class.
    /// </summary>
    public class ImpersonationSession
    {
        /// <summary>
        /// Provides the current user. 
        /// </summary>
        public static Func<IUser> CurrentUserProvider = GetCurrentUser;

        static HttpContext Context => HttpContext.Current;

        /// <summary>
        /// Determines if the current user is impersonated.
        /// </summary>
        public static bool IsImpersonated() => Impersonator != null;

        /// <summary>
        /// Impersonates the specified user by the current admin user.
        /// </summary>
        /// <param name="originalUrl">If not specified, the current HTTP request's URL will be used.</param>
        public static void Impersonate(IUser user, bool redirectToHome = true, string originalUrl = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var admin = CurrentUserProvider?.Invoke() as IImpersonator;

            if (admin == null)
                throw new InvalidOperationException("The current user is not an IImpersonator.");

            if (!admin.CanImpersonate(user))
                throw new InvalidOperationException("The current user is not allowed to impersonate the specified user.");

            var token = Guid.NewGuid().ToString();

            Database.Update(admin, o => o.ImpersonationToken = token);

            ImpersonationToken = token;
            OriginalUrl = originalUrl.Or(Context.Request.RawUrl);

            user.LogOn();

            if (redirectToHome && !Context.Request.IsAjaxCall())
                Context.Response.Redirect("~/");
        }

        /// <summary>
        /// Ends the current impersonation session.
        /// </summary>
        public static void End()
        {
            if (!IsImpersonated()) return;

            CookieProperty.Remove<ILanguage>();

            var admin = Impersonator;

            Database.Update(admin, o => o.ImpersonationToken = null);

            admin.LogOn();

            var returnUrl = OriginalUrl;
            OriginalUrl = null;
            ImpersonationToken = null;

            if (!Context.Request.IsAjaxCall())
                Context.Response.Redirect(returnUrl);
        }

        static IUser GetCurrentUser()
        {
            var result = Context.User as IIdentity;
            if (result == null || !result.IsAuthenticated) return null;

            return result as IUser;
        }

        /// <summary>
        /// Gets the original user who impersonated the current user.
        /// </summary>
        public static IImpersonator Impersonator
        {
            get
            {
                var user = CurrentUserProvider?.Invoke();
                if (user == null || user.IsInRole("Guest") || user.IsInRole("Anonymous")) return null;

                var token = ImpersonationToken;
                if (token.IsEmpty()) return null;

                return Database.Find<IImpersonator>(x => x.ImpersonationToken == token);
            }
        }

        static string ImpersonationToken
        {
            get
            {
                return CookieProperty.Get("Impersonation.Token");
            }
            set
            {
                CookieProperty.Set("Impersonation.Token", value);
            }
        }

        public static string OriginalUrl
        {
            get
            {
                return CookieProperty.Get("Impersonation.Original.Url").Or("~/");
            }
            set
            {
                CookieProperty.Set("Impersonation.Original.Url", value);
            }
        }
    }
}