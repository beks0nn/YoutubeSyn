using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext DBCONTEXT = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(PopulateViewModel());
        }

        public HomeViewModel PopulateViewModel()
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = DBCONTEXT.Urls.ToList();
            var identity = retModel.UrlList.First().CurrentUrl.UrlIdentity;
            var item = DBCONTEXT.Urls.First(i => i.Id == identity);
            retModel.UrlCurrent = item.UrlPart;

            return retModel;
        }

        public PartialViewResult AddUrl(string Url)
        {
            var itemToAdd = new WebApplication1.Models.Uri();
            itemToAdd.UrlPart = (Url);
            itemToAdd.UrlIdentity = 1;
            DBCONTEXT.Urls.Add(itemToAdd);
            DBCONTEXT.SaveChanges();

            return PartialView("UrlListPartial", PopulateViewModel());
        }


        public PartialViewResult NextUrl()
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = DBCONTEXT.Urls.ToList();

            if(retModel.UrlList.Count() < 2 )
            {
                retModel.UrlCurrent = DBCONTEXT.Urls.First().UrlPart;
                return PartialView("IFramePartial", retModel);
            }
            else
            {
                var currentId = retModel.UrlList.First().CurrentUrl.UrlIdentity;
                var index = retModel.UrlList.FindIndex(m => m.Id == currentId);

                //IF AT END OF LIST
                if(index+1 >= retModel.UrlList.Count()) 
                {
                    index = 0;
                    var ret = DBCONTEXT.CurrUrl.Single(a => a.Id == 1);
                    ret.UrlIdentity = retModel.UrlList[index].Id;
                    DBCONTEXT.SaveChanges();

                    retModel.UrlCurrent = retModel.UrlList[index].UrlPart;
                }
                else
                { 
                    var ret_ = DBCONTEXT.CurrUrl.Single(a => a.Id == 1);
                    ret_.UrlIdentity = retModel.UrlList[index + 1].Id;
                    DBCONTEXT.SaveChanges();
                
                    retModel.UrlCurrent = retModel.UrlList[index+1].UrlPart;
                }
                
            }
            return PartialView("IFramePartial", retModel);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}