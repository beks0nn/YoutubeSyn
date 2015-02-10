using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using WebApplication1.MongoDBLayer;
using WebApplication1.Models;

namespace WebApplication1.Hubs
{
    public class SyncHub : Hub
    {
        private MongoData mongo = new MongoData();

        public void AddUrl(string url)
        {
            var urlToAdd = new Url();
            urlToAdd.UrlPart = url;
            mongo.CreateUrl(urlToAdd);


            //Push the data ..
            Clients.All.addUrl(url);
        }

        public void NextUrl()
        {
            var returnUrl = "";
            var retModel = new HomeViewModel();
            retModel.UrlList = mongo.GetAllUrls();
            var currUrlObj = mongo.GetAllCurrUrls().First();
            mongo.updateCurrUrlTime(currUrlObj.Id, "0");

            if (retModel.UrlList.Count() < 2) //IF LIST IS ONLY 1 ELEMENT
            {
                returnUrl = retModel.UrlList.First().UrlPart;
                
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {

                var currUrlObjPOINTINGAT = currUrlObj.UrlIdentity;
                var index = retModel.UrlList.FindIndex(m => m.Id == currUrlObjPOINTINGAT);

                try
                {
                    //IF AT END OF LIST
                    if (index + 1 >= retModel.UrlList.Count())
                    {
                        index = 0;
                        //UpdateDB ITEM
                        var newid = retModel.UrlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);

                        //Update Model with latest vals.
                        returnUrl = retModel.UrlList.First().UrlPart;
                    }
                    else
                    {
                        //UpdateDB ITEM
                        index = index + 1;
                        var newid = retModel.UrlList[index].Id;
                        mongo.updateCurrUrl(currUrlObj.Id, newid);

                        //Update Model with latest vals.
                        var urlList = mongo.GetAllUrls();
                        var currentUrlObj = mongo.GetAllCurrUrls().First();
                        var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

                        /*** Send less data... ***/
                        returnUrl = CurrentUrl.UrlPart;

                    }
                }
                catch (Exception ex)
                {
                    var urlList = mongo.GetAllUrls();
                    var currentUrlObj = mongo.GetAllCurrUrls().First();
                    var CurrentUrl = urlList.First(i => i.Id == currentUrlObj.UrlIdentity);

                    /*** Send less data... ***/
                    returnUrl = CurrentUrl.UrlPart;
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
    }
}