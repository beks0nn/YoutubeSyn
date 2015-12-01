using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication1
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.IgnoreRoute("{*allashx}", new { allashx = @".*\.ashx" });
            routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.aspx" });
            routes.IgnoreRoute("{*alljs}", new { alljs = @".*\.js" });
            routes.IgnoreRoute("{*allpng}", new { allpng = @".*\.png" });
            routes.IgnoreRoute("{*alljpg}", new { alljpg = @".*\.jpg" });
            routes.IgnoreRoute("{*allgif}", new { allgif = @".*\.gif" });
            routes.IgnoreRoute("{*allcss}", new { allcss = @".*\.css" });
            routes.IgnoreRoute("{*allwoff}", new { allcss = @".*\.woff" });
            routes.IgnoreRoute("{*alleot}", new { allcss = @".*\.eot" });
            routes.IgnoreRoute("{*allsvg}", new { allcss = @".*\.svg" });
            routes.IgnoreRoute("{*alltff}", new { allcss = @".*\.tff" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Sync", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
