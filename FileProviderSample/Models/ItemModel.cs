using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileProviderSample.Models
{
    public class ItemModel
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public ItemType ItemType { get; set; }
    }

    public enum ItemType
    {
        Site,
        List,
        File,
        Folder
    }
}