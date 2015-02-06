using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.MongoDBLayer;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net.Http;
using System.Net;
using MongoDB.Driver;

namespace WebApplication1.Controllers
{

    public class HomeController : Controller, IDisposable
    {
        private MongoData mongo = new MongoData();
        private bool disposed = false;

        public ActionResult Index()
        {
            return View(PopulateViewModel());
        }

        public PartialViewResult AddUrl(string Url)
        {
            try
            {
                var urlToAdd = new Url();
                urlToAdd.UrlPart = Url;
                mongo.CreateUrl(urlToAdd);

                return PartialView("UrlListPartial", PopulateViewModel());
            }
            catch(Exception ex)
            {
                //TODOS
                return PartialView();
            }
        }

        public PartialViewResult NextUrl(string rowVersion)
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = mongo.GetAllUrls();

            if(retModel.UrlList.Count() < 2 ) //IF LIST IS ONLY 1 ELEMENT
            {
                retModel.UrlCurrent = retModel.UrlList.First().UrlPart;
                return PartialView("IFramePartial", retModel);
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {
                var currUrlObj = mongo.GetAllCurrUrls().First();
                var currUrlObjPOINTINGAT = currUrlObj.UrlIdentity;
                var index = retModel.UrlList.FindIndex(m => m.Id == currUrlObjPOINTINGAT);

                try
                { 
                    //IF AT END OF LIST
                    if(index+1 >= retModel.UrlList.Count()) 
                    {
                        index = 0;
                        //UpdateDB ITEM
                        var newid = retModel.UrlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);

                        //Update Model with latest vals.
                        retModel = PopulateViewModel();
                        return PartialView("IFramePartial", retModel);
                    }
                    else
                    {
                        //UpdateDB ITEM
                        index = index+1;
                        var newid = retModel.UrlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);

                        //Update Model with latest vals.
                        retModel = PopulateViewModel();
                        return PartialView("IFramePartial", retModel);
                    }
                }
                catch(Exception ex)
                {
                    //hmm
                    return PartialView("IFramePartial", PopulateViewModel());
                }
            }
        }

        public PartialViewResult Sync()
        {
            return PartialView("IFramePartial", PopulateIFrame());
        }

        public JsonResult Syn(string rowV)
        {
            if (IsSync(rowV))
                return this.Json(new { syn = true });
            else
                return this.Json(new {syn = false});
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


        #region DBHelpers
        public bool IsSync(string rowV)
        {
            var urlList = mongo.GetAllUrls();
            var currUrl = mongo.GetAllCurrUrls().First();
            if (currUrl.version.ToString() == rowV)
                return true;
            else
                return false;
        }


        public HomeViewModel PopulateViewModel()
        {
            var retModel = new HomeViewModel();
            retModel.UrlList = mongo.GetAllUrls();
            var identity = mongo.GetAllCurrUrls().First();
            var CurrentUrl = retModel.UrlList.First(i => i.Id == identity.UrlIdentity);
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = identity.version.ToString();
            return retModel;
        }

        public HomeViewModel PopulateIFrame()
        {
            var retModel = new HomeViewModel();
            var UrlList = mongo.GetAllUrls();
            var identity = mongo.GetAllCurrUrls().First();
            var CurrentUrl = UrlList.First(i => i.Id == identity.UrlIdentity);//Get URL item using currentUrlIdentifier
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = identity.version.ToString();
            return retModel;
        }

        #endregion

        # region IDisposable

        new protected void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        new protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.mongo.Dispose();
                }
            }

            this.disposed = true;
        }

        # endregion
    }
}