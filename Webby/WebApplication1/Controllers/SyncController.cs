using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.MongoDBLayer;


namespace WebApplication1.Controllers
{
    public class SyncController : Controller, IDisposable
    {
        private MongoData mongo = new MongoData();
        private bool disposed = false;


        public ActionResult Index()
        {
            return View(PopulateViewModel());
        }

        public SyncViewModel PopulateViewModel()
        {
            var retModel = new SyncViewModel();
            var urlList = mongo.GetAllUrls();
            var currentUrlObj = mongo.GetAllCurrUrls().First();
            var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

            retModel.CurrentTime = currentUrlObj.time;
            retModel.UrlList = urlList;
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = currentUrlObj.version.ToString();
            retModel.jSonList = JsonConvert.SerializeObject(urlList);

            return retModel;
        }

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