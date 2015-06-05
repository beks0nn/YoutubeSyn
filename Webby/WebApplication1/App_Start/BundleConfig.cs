using System.Web;
using System.Web.Optimization;

namespace WebApplication1
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/jquery.signalR-2.2.0.min.js",
                        "~/Scripts/respond.min.js",
                        "~/Scripts/underscore.js",
                        "~/Scripts/jquery-scrollto.js"
                        ));

            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"
                        ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/clean-blog.css"
                      ));

            //!!Comment out if you don't want minification!!.
            BundleTable.EnableOptimizations = true;
        }
    }
}
