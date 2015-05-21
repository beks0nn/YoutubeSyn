using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
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
            var playList = mongo.GetAllPlayLists().First();
            //var retUrlList = playList.UrlList.Select(e => new SmallerUrl() { Title = WebUtility.HtmlDecode(e.Title), UrlPart = e.UrlPart });

            retModel.RowVersion = playList.version.ToString();
            retModel.UrlCurrent = playList.CurrentUrl;
            retModel.isRepeat = playList.isRepeat;
            retModel.CurrentTime = playList.time;
            retModel.jSonList = JsonConvert.SerializeObject(playList.UrlList);

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