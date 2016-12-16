using FileProviderSample.Models;
using FileProviderSample.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FileProviderSample.Controllers
{
    public class FilesController : Controller
    {
        // GET: Sites
        public async Task<ActionResult> Sites()
        {
            //get the discovery information for the user
            var discoToken = await this.HttpContext.GetAccessToken(TokenHelper.DISCO_RESOURCE);
            if (discoToken == null)
                return RedirectToAction("Index", "AADAuth");

            //call into the discovery service to get SharePoint endpoints
            var discoJson = await getJson(discoToken.AccessToken, "https://api.office.com/discovery/v2.0/me/services");
            var shptEndpoint = String.Empty;
            var shptResource = String.Empty;
            var items = ((JArray)discoJson["value"]);

            //find the rootsite resource
            foreach (var item in items)
            {
                if (item.Value<string>("capability") == "RootSite")
                {
                    shptEndpoint = item.Value<string>("serviceEndpointUri");
                    shptResource = item.Value<string>("serviceResourceId");
                    break;
                }
            }

            //get token for the rootsite
            var sitesToken = await this.HttpContext.GetAccessToken(shptResource);
            var sitesJson = await getJson(sitesToken.AccessToken, $"{shptEndpoint}/search/query?querytext='contentclass:sts_site'&trimduplicates=true&rowlimit=50&SelectProperties='WebTemplate,Title,Path,SiteLogo'");

            //parse the rows into SiteModel entities
            List<SiteModel> sites = sitesJson.ToSitesList();

            //return the view
            return View(sites);
        }

        [Route("Files/Site")]
        [HttpGet]
        public async Task<ActionResult> Site(string site)
        {
            //get the root domain of the site that will be used as the resource for tokens
            if (site.ToCharArray()[site.Length - 1] != '/')
                site += "/";
            var resource = site.Substring(0, 8 + site.Substring(8).IndexOf('/'));

            //get access token
            var token = await this.HttpContext.GetAccessToken(resource);
            new List<ItemModel>();

            //first get subsites for the site
            var subsitesJson = await getJson(token.AccessToken, $"{site}_api/web/webinfos");
            List<ItemModel> items = subsitesJson.ToItemsFromSites(resource);

            //next, add document libraries
            var listsJson = await getJson(token.AccessToken, $"{site}_api/web/lists");
            items.AddRange(listsJson.ToItemsFromLists());
            
            //add the site to the view data
            ViewData["Site"] = site;
            return View(items);
        }

        [Route("Files/List")]
        [HttpGet]
        public async Task<ActionResult> List(string site, string list, string listname, string folder)
        {
            //get the root domain of the site that will be used as the resource for tokens
            if (site.ToCharArray()[site.Length - 1] != '/')
                site += "/";
            var resource = site.Substring(0, 8 + site.Substring(8).IndexOf('/'));

            //get access token
            var token = await this.HttpContext.GetAccessToken(resource);
            new List<ItemModel>();

            var endpoint = "";
            if (!String.IsNullOrEmpty(folder))
                endpoint = $"{site}_api/web/GetFolderByServerRelativeUrl('{folder}')/";
            else
                endpoint = $"{site}_api/web/lists(guid'{list}')/rootfolder/";

            //first get the folders
            var foldersJson = await getJson(token.AccessToken, endpoint + "folders");
            List<ItemModel> items = foldersJson.ToItemsFromListItems(ItemType.Folder);

            //next add the files
            var filesJson = await getJson(token.AccessToken, endpoint + "files");
            items.AddRange(filesJson.ToItemsFromListItems(ItemType.File));

            //add the site to the view data
            ViewData["Site"] = site;
            ViewData["List"] = list;
            ViewData["Listname"] = listname;
            return View(items);
        }

        private async Task<JObject> getJson(string token, string endpoint)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");//; odata=verbose");
            using (var response = await client.GetAsync(endpoint))
            {
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(json);
                }
                else
                    return null;
            }
        }
    }
}