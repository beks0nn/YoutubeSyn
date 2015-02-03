using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class HomeViewModel
    {
        public List<Uri> UrlList { get; set; }
        public string Url { get; set; }
        public string UrlCurrent { get; set; }
    }
}