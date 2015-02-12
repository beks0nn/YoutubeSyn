using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using WebApplication1.MongoDBLayer;
using WebApplication1.Models;

namespace WebApplication1.Hubs
{
    public class SyncHub : Hub
    {
        private static Random rnd = new Random();
        private MongoData mongo = new MongoData();
        private bool disposed = false;

        public void AddUrl(string url)
        {
            var urlToAdd = new Url();
            urlToAdd.UrlPart = url;
            mongo.CreateUrl(urlToAdd);
            Clients.All.addUrl(url);
        }

        public void NextUrl()
        {
            //Mabey not use mongoloid function here,  shape up
            var returnUrl = "";
            var urlList = mongo.GetAllUrls();
            var currUrlObj = mongo.GetAllCurrUrls().First();

            if (urlList.Count() < 2) //IF LIST IS ONLY 1 ELEMENT
            {
                returnUrl = urlList.First().UrlPart;
                
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {

                var currUrlObjPOINTINGAT = currUrlObj.UrlIdentity;
                var index = urlList.FindIndex(m => m.Id == currUrlObjPOINTINGAT);
                try
                {
                    //IF AT END OF LIST
                    if (index + 1 >= urlList.Count())
                    {
                        index = 0;
                        var newid = urlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);
                        returnUrl = urlList.First().UrlPart;
                    }
                    else
                    {
                        //UpdateDB ITEM
                        index = index + 1;
                        var newid = urlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);


                        var ReturnUrl = urlList.Single(i => i.Id == newid);
                        /*** Send less data... ***/
                        returnUrl = ReturnUrl.UrlPart;

                    }
                }
                catch (Exception ex)
                {
                    /*** Todos ***/
                    returnUrl = "";
                }
            }
            Clients.All.nextUrl(returnUrl);
        }

        public void GoToTime(string time,string rowV)
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            decimal yTimeIN = decimal.Parse(time);
            decimal yTimeCurr = decimal.Parse(currUrl.time);
            var diff = yTimeIN - yTimeCurr;

            if (currUrl.version.ToString() == rowV)
            {
                if (diff <= -5 || diff >= 5)
                {
                    mongo.updateCurrUrlTime(currUrl.Id, time);
                    var updatedcurrUrl = mongo.GetAllCurrUrls().First();
                    Clients.All.goToTime(time, updatedcurrUrl.version);
                }
            }
            else
            {
                Clients.Caller.goToTime(currUrl.time, currUrl.version);
            }
        }

        public void GoToUrl(string url)
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            var toId = mongo.GetAllUrls().Single(e => e.UrlPart == url);
            mongo.updateCurrUrl(currUrl.Id, toId.Id);

            Clients.All.goToUrl(url);
        }

        public void ShuffleUrl()
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            var urlList = mongo.GetAllUrls().ToList();
            var r = rnd.Next(urlList.Count);
            mongo.updateCurrUrl(currUrl.Id, urlList[r].Id);
            Clients.All.goToUrl(urlList[r].UrlPart);
        }

        public void DeleteUrl(string url)
        {
            mongo.DeleteUrl(url);
            /*SHUFFLE CODE*/
            var currUrl = mongo.GetAllCurrUrls().First();
            var urlList = mongo.GetAllUrls().ToList();
            var r = rnd.Next(urlList.Count);
            mongo.updateCurrUrl(currUrl.Id, urlList[r].Id);
            /**/
            Clients.All.afterDelete(urlList[r].UrlPart, url);

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