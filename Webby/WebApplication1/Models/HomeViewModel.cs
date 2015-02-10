using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class HomeViewModel
    {
        public List<Url> UrlList { get; set; }
        public string UrlCurrent { get; set; }
        public string RowVersion { get; set; }
        public string CurrentTime { get; set; }
    }
}