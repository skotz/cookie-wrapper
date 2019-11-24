using System;
using System.Linq;
using System.Web;

/// <summary>
/// Scott Clayton 2019
/// https://github.com/skotz/cookie-wrapper
/// </summary>
namespace Skotz
{
    /// <summary>
    /// Wraps the HTTP context cookie so you don't have to worry about lost changes, duplicates, or wrong collections.
    /// </summary>
    public class CookieWrapper
    {
        private HttpContext _context;

        /// <summary>
        /// Simplified collection of HTTP cookies.
        /// </summary>
        /// <param name="name">The name of the cookie</param>
        /// <returns></returns>
        public HttpCookie this[string name]
        {
            get
            {
                return GetCookie(name);
            }
            set
            {
                if (name != value.Name)
                {
                    // To change a cookie name you need to expire the old one and create a new one
                    throw new ArgumentException($"Cookie name {value.Name} cannot be changed to {name}!");
                }

                SetCookie(value);
            }
        }

        /// <summary>
        /// Creates a wrapper around HTTP cookies to make them easier to manage.
        /// </summary>
        public CookieWrapper()
            : this (HttpContext.Current)
        {
        }

        /// <summary>
        /// Creates a wrapper around HTTP cookies to make them easier to manage.
        /// </summary>
        /// <param name="context">The current HttpContext (e.g., HttpContext.Current)</param>
        public CookieWrapper(HttpContext context)
        {
            _context = context ?? throw new ArgumentException($"Argument {nameof(context)} cannot be null!");
        }

        /// <summary>
        /// Get a cookie from the current HTTP context.
        /// </summary>
        /// <param name="name">The name of the cookie to get.</param>
        /// <returns></returns>
        public HttpCookie GetCookie(string name)
        {
            if (_context?.Response?.Cookies?.AllKeys?.Contains(name) ?? false)
            {
                // Always get the response cookie if it exists so we don't grab an old value when updating the cookie multiple times in a page load
                return _context?.Response?.Cookies?[name];
            }
            else
            {
                return _context?.Request?.Cookies?[name];
            }
        }

        /// <summary>
        /// Send a cookie to the client's machine.
        /// </summary>
        /// <param name="cookie">The cookie to send.</param>
        public void SetCookie(HttpCookie cookie)
        {
            // Note that adding a response cookie
            // 1. will insert or overwrite (if it exists) any cookie by the same name in the response collection
            // 2. will automatically add (but not overwrite) the same cookie to the request collection
            // This is why once a cookie is updated in the response you should no longer read that same cookie from the request again
            _context.Response.SetCookie(cookie);
        }

        /// <summary>
        /// Remove a cookie from the client's machine.
        /// </summary>
        /// <param name="name">The name of the cookie to remove.</param>
        public void ExpireCookie(string name)
        {
            // To remove a cookie from the client's computer you need to send them an updated cookie with a past expiration date
            // Deleting the cookie from the response will have no effect on the client
            var cookie = new HttpCookie(name);
            cookie.Expires = DateTime.Now.AddDays(-1);
            _context.Response.SetCookie(cookie);
        }
    }
}