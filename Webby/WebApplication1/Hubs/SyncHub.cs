using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using WebApplication1.MongoDBLayer;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Hubs
{
    public class SyncHub : Hub
    {
        private MongoData mongo = new MongoData();
        private static Random RND = new Random();
        private bool disposed = false;

        public void AddUrl(string url)
        {
            try
            {
                url = ExtractYoutubeId(url);
                var urlToAdd = new Url();
                urlToAdd.UrlPart = url;
                mongo.CreateUrl(urlToAdd);
                Clients.All.addUrl(url);
            }
            catch (UrlFormatException ex)
            {
                Clients.Caller.addUrlEx();
            }
            catch (Exception)
            {
                Clients.Caller.addUrlEx(); // TODOS not expected.
            }
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
            var toId = mongo.GetAllUrls().First(e => e.UrlPart == url);
            mongo.updateCurrUrl(currUrl.Id, toId.Id);

            Clients.All.goToUrl(url);
        }

        public void ShuffleUrl()
        {
            var currUrl = mongo.GetAllCurrUrls().First();
            var urlList = mongo.GetAllUrls().ToList();
            var r = RND.Next(urlList.Count);
            mongo.updateCurrUrl(currUrl.Id, urlList[r].Id);
            Clients.All.goToUrl(urlList[r].UrlPart);
        }

        public void DeleteUrl(string url)
        {
            mongo.DeleteUrl(url);
            /*SHUFFLE CODE*/
            var currUrl = mongo.GetAllCurrUrls().First();
            var urlList = mongo.GetAllUrls().ToList();
            var r = RND.Next(urlList.Count);
            mongo.updateCurrUrl(currUrl.Id, urlList[r].Id);
            /**/
            Clients.All.afterDelete(urlList[r].UrlPart, url);

        }

        #region Helpers 
        /// <summary>
        /// Try to extract youtube ID from URl or normal ID input & Basic Sanity check
        /// Encode to Html String to protect from scripts etc.( good enough).
        /// </summary>
        private static Regex YOUTUBEREGEX = new Regex(@"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&amp;]v=)|youtu\.be\/)([^""&amp;?\/ ]{11})");
        private static Regex YOUTUBESANITY = new Regex(@"[a-zA-Z0-9_-]{11}");
        public string ExtractYoutubeId(string url)
        {
            var retUrl = System.Net.WebUtility.HtmlEncode(url);

            if (url.Length > 11)
            {
                retUrl = YOUTUBEREGEX.Match(url).Groups[1].Value;
                if (YOUTUBESANITY.Match(retUrl).Success)
                {
                    return retUrl;
                }
                else
                {
                    throw new UrlFormatException();
                }
            }
            else
            {
                if (YOUTUBESANITY.Match(retUrl).Success)
                {
                    return retUrl;
                }
                else
                {
                    throw new UrlFormatException();
                }
            }      
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