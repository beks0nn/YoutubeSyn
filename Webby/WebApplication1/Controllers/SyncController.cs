﻿using System;
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
    public class SyncController : Controller, IDisposable
    {
        private MongoData mongo = new MongoData();
        private bool disposed = false;


        public ActionResult Index()
        {
            return View(PopulateViewModel());
        }

        public HomeViewModel PopulateViewModel()
        {
            var retModel = new HomeViewModel();
            var urlList = mongo.GetAllUrls();
            var currentUrlObj = mongo.GetAllCurrUrls().First();
            var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

            retModel.CurrentTime = currentUrlObj.time;
            retModel.UrlList = urlList;
            retModel.UrlCurrent = CurrentUrl.UrlPart;
            retModel.RowVersion = currentUrlObj.version.ToString();

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