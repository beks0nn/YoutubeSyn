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
using System.Globalization;

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
            var currUrlObj = mongo.GetAllCurrUrls().First();
            mongo.updateCurrUrlTime(currUrlObj.Id, "0");

            if(retModel.UrlList.Count() < 2 ) //IF LIST IS ONLY 1 ELEMENT
            {
                retModel.UrlCurrent = retModel.UrlList.First().UrlPart;
                return PartialView("IFramePartial", retModel);
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {
                
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
                        retModel = PopulateIFrame();
                        return PartialView("IFramePartial", retModel);
                    }
                    else
                    {
                        //UpdateDB ITEM
                        index = index+1;
                        var newid = retModel.UrlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);

                        //Update Model with latest vals.
                        retModel = PopulateIFrame();
                        return PartialView("IFramePartial", retModel);
                    }
                }
                catch(Exception ex)
                {
                    //hmm
                    return PartialView("IFramePartial", PopulateIFrame());
                }
            }
        }

        public PartialViewResult Sync()
        {
            return PartialView("IFramePartial", PopulateIFrame());
        }

        public JsonResult Syn(string rowV, string yTime)
        {
            var res = IsSync(rowV,yTime);
            return this.Json(new { syn = res });
        }

        public string IsSync(string rowV, string yTime)
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            decimal yTimeIN = decimal.Parse(yTime);
            decimal yTimeCurr = decimal.Parse(currUrl.time);
            var diff = yTimeIN - yTimeCurr;

            if (currUrl.version.ToString() == rowV)
            {
                if (diff <= -5 || diff >= 5)
                {
                    return currUrl.time;  //seek to this time.
                }
                else
                {
                    //DIFF OK update this time
                    mongo.updateCurrUrlTime(currUrl.Id, yTime);
                    return "ok"; // Keep polling
                }

            }
            else
                return "!ok";//Will next if this response 
        }

        public void setTime(string yTime)
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            mongo.updateCurrUrlTime(currUrl.Id, yTime);
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

        public HomeViewModel PopulateViewModel()
        {
            var retModel = new HomeViewModel();
            var urlList = mongo.GetAllUrls();
            var currentUrlObj = mongo.GetAllCurrUrls().First();
            var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

            retModel.UrlList = urlList;
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = currentUrlObj.version.ToString();

            return retModel;
        }

        public HomeViewModel PopulateIFrame()
        {
            var retModel = new HomeViewModel();
            var urlList = mongo.GetAllUrls();
            var currentUrlObj = mongo.GetAllCurrUrls().First();
            var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

            /*** Send less data... ***/
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = currentUrlObj.version.ToString();

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