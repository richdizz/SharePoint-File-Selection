using FileProviderSample.Utils;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FileProviderSample.Controllers
{
    public class AADAuthController : Controller
    {
        /// <summary>
        /// NOTE: this sample does not use the ADAL TokenCache and handles Refresh Tokens manually
        /// You might consider using TokenCache instead
        /// </summary>
        public async Task<ActionResult> Index()
        {
            //check for code
            AuthenticationContext authContext = new AuthenticationContext(TokenHelper.AUTHORITY, false, null);
            ClientCredential creds = new ClientCredential(TokenHelper.CLIENT_ID, TokenHelper.CLIENT_SECRET);
            if (!String.IsNullOrEmpty(Request["code"]))
            {
                //get the access token using the code
                var result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                    Request["code"], 
                    new Uri($"https://{TokenHelper.HOST_DOMAIN}/AADAuth"), 
                    creds,
                    TokenHelper.DISCO_RESOURCE);

                //save the refresh token
                var cookie = new HttpCookie("RefreshToken", result.RefreshToken);
                cookie.Expires = DateTime.Now.AddDays(14);
                Response.SetCookie(cookie);

                //redirect to files controller
                return RedirectToAction("Sites", "Files");
            }
            else
            {
                //try to get a refresh token using access token
                try
                {
                    //get token using refresh token
                    var token = await this.HttpContext.GetAccessToken(TokenHelper.DISCO_RESOURCE);
                    if (token == null)
                        throw new Exception();
                    
                    //redirect to files controller
                    return RedirectToAction("Sites", "Files");
                }
                catch (Exception)
                {
                    //silent attempt for access token failed...perform code authorization flow
                    var url = authContext.GetAuthorizationRequestURL(
                        TokenHelper.DISCO_RESOURCE,
                        TokenHelper.CLIENT_ID, 
                        new Uri($"https://{TokenHelper.HOST_DOMAIN}/AADAuth"), 
                        UserIdentifier.AnyUser, 
                        "state=somestate");

                    //redirect to the code authorization flow with AAD
                    return Redirect(url.ToString());
                }
            }
        }
    }
}