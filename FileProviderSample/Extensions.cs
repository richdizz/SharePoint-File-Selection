using FileProviderSample.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileProviderSample
{
    public static class Extensions
    {
        public static List<SiteModel> ToSitesList(this JObject json)
        {
            List<SiteModel> sites = new List<SiteModel>();
            var rows = ((JArray)json.SelectToken("PrimaryQueryResult.RelevantResults.Table.Rows"));
            foreach (var row in rows)
            {
                SiteModel site = new SiteModel();
                var cells = ((JArray)row.SelectToken("Cells"));
                foreach (var cell in cells)
                {
                    if (cell.SelectToken("Key").Value<string>().Equals("Title"))
                        site.Name = cell.SelectToken("Value").Value<string>();
                    if (cell.SelectToken("Key").Value<string>().Equals("Path"))
                        site.Path = cell.SelectToken("Value").Value<string>();
                }
                sites.Add(site);
            }
            return sites;
        }

        public static List<ItemModel> ToItemsFromSites(this JObject json, string resource)
        {
            List<ItemModel> items = new List<ItemModel>();

            var results = ((JArray)json.SelectToken("value"));
            foreach (var item in results)
            {
                items.Add(new ItemModel()
                {
                    ItemType = ItemType.Site,
                    Title = item.SelectToken("Title").Value<string>(),
                    Url = resource + item.SelectToken("ServerRelativeUrl").Value<string>()
                });
            }

            return items;
        }

        public static List<ItemModel> ToItemsFromLists(this JObject json)
        {
            List<ItemModel> items = new List<ItemModel>();

            var results = ((JArray)json.SelectToken("value"));
            foreach (var item in results)
            {
                //only display non-hidden document libraries (101)
                if (item.SelectToken("BaseTemplate").Value<int>() == 101
                    && !item.SelectToken("Hidden").Value<bool>())
                {
                    items.Add(new ItemModel()
                    {
                        ItemType = ItemType.List,
                        Title = item.SelectToken("Title").Value<string>(),
                        Id = item.SelectToken("Id").Value<string>()
                    });
                }
            }

            return items;
        }

        public static List<ItemModel> ToItemsFromListItems(this JObject json, ItemType type)
        {
            List<ItemModel> items = new List<ItemModel>();

            var results = ((JArray)json.SelectToken("value"));
            foreach (var item in results)
            {
                items.Add(new ItemModel()
                {
                    ItemType = type,
                    Title = item.SelectToken("Name").Value<string>(),
                    Url = item.SelectToken("ServerRelativeUrl").Value<string>()
                });
            }

            return items;
        }
    }
}