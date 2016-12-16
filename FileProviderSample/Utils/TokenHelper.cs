using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FileProviderSample.Utils
{
    public static class TokenHelper
    {
        public static string AUTHORITY = "https://login.microsoftonline.com/common";
        public static string CLIENT_ID = ConfigurationManager.AppSettings["ClientId"];
        public static string CLIENT_SECRET = ConfigurationManager.AppSettings["ClientSecret"];
        public static string DISCO_RESOURCE = "https://api.office.com/discovery/";
        public static string HOST_DOMAIN = ConfigurationManager.AppSettings["HostDomain"];

        public static async Task<AuthenticationResult> GetAccessToken(this HttpContextBase context, string resource)
        {
            AuthenticationResult result = null;
            try
            {
                AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/common", false, null);
                ClientCredential creds = new ClientCredential(CLIENT_ID, CLIENT_SECRET);
                var refreshToken = context.Request.Cookies["RefreshToken"].Value;
                result = await authContext.AcquireTokenByRefreshTokenAsync(refreshToken, creds, resource);

                //save the NEW refresh token (rolling two week expiration)
                var cookie = new HttpCookie("RefreshToken", result.RefreshToken);
                cookie.Expires = DateTime.Now.AddDays(14);
                context.Response.SetCookie(cookie);
            }
            catch (Exception)
            {
                //swallow exception and allow null result to return
            }
            
            return result;
        }
    }
}