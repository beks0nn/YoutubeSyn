using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using WebApplication1.MongoDBLayer;
using System.Text.RegularExpressions;
using WebApplication1.Models;
using System.Net;

namespace WebApplication1.Hubs
{
    public class SyncHub : Hub
    {
        private MongoData mongo = new MongoData();
        private static Random RND = new Random();
        private bool disposed = false;

        public void AddUrl(string url, string title)
        {
            try
            {
                var urlToAdd = new Url();
                checkInputStrings(url,title);
                urlToAdd.UrlPart = url;
                urlToAdd.Title = title;
                mongo.AddUrlToList(urlToAdd, Guid.NewGuid());//TODOS Only 1 playlist for now as there are no users.

                Clients.All.addUrl(url, title);
            }
            catch (InputFormatException)
            {
                Clients.Caller.addUrlEx();
            }
            catch (Exception)
            {
                Clients.Caller.addUrlEx(); // TODOS not expected.
            }
        }

        public void DeleteUrl(string url)
        {
            mongo.RemoveUrlFromList(url);//TODOS Only 1 playlist for now as there are no users.
            /*SHUFFLE CODE*/
            var playList = mongo.GetAllPlayLists().First();
            var r = RND.Next(playList.UrlList.Count);
            mongo.UpdatePlayListCurrentUrl(playList.Id, playList.UrlList[r].UrlPart); 
            /**/
            Clients.All.afterDelete(playList.UrlList[r].UrlPart, url);

        }

        public void NextUrl()
        {
            //Mabey not use mongoloid function here,  shape up
            var returnUrl = "";
            var playList = mongo.GetAllPlayLists().First();

            if (playList.UrlList.Count() < 2) //IF LIST IS ONLY 1 ELEMENT
            {
                returnUrl = playList.UrlList.First().UrlPart;
                
            }
            else //IF LIST IS SEVERAL ELEMENTS
            {

                var index = playList.UrlList.FindIndex(m => m.UrlPart == playList.CurrentUrl);
                try
                {
                    //IF AT END OF LIST
                    if (index + 1 >= playList.UrlList.Count())
                    {
                        index = 0;
                        returnUrl = playList.UrlList[index].UrlPart;
                        mongo.UpdatePlayListCurrentUrl(playList.Id, returnUrl);
                    }
                    else
                    {
                        //UpdateDB ITEM
                        index = index + 1;
                        returnUrl = playList.UrlList[index].UrlPart;
                        mongo.UpdatePlayListCurrentUrl(playList.Id, returnUrl);
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
            var playList = mongo.GetAllPlayLists().First();
            decimal yTimeIN = decimal.Parse(time);
            decimal yTimeCurr = decimal.Parse(playList.time);
            var diff = yTimeIN - yTimeCurr;

            if (playList.version.ToString() == rowV)
            {
                if (diff <= -5 || diff >= 5)
                {
                    var newVersion = mongo.UpdatePlayListTime(playList.Id, time);//Get updated version set new time and version
                    Clients.All.goToTime(time, newVersion);
                }
            }
            else
            {
                Clients.Caller.goToTime(playList.time, playList.version);
            }
        }

        public void GoToUrl(string url)
        {
            var playList = mongo.GetAllPlayLists().First();
            mongo.UpdatePlayListCurrentUrl(playList.Id, url);
            Clients.All.goToUrl(url);
        }

        public void ShuffleUrl()
        {
            var playList = mongo.GetAllPlayLists().First();
            var r = RND.Next(playList.UrlList.Count);
            mongo.UpdatePlayListCurrentUrl(playList.Id, playList.UrlList[r].UrlPart);
            Clients.All.goToUrl(playList.UrlList[r].UrlPart);
        }

        public void InitShuffleUrl()
        {
            var playList = mongo.GetAllPlayLists().First();
            var setter = playList.isShuffle ? false : true;
            mongo.UpdatePlayListShuffle(playList.Id, setter);
            var r = RND.Next(playList.UrlList.Count);
            mongo.UpdatePlayListCurrentUrl(playList.Id, playList.UrlList[r].UrlPart);

            Clients.All.initShuffleUrl(playList.UrlList[r].UrlPart, setter);
        }


        public void SetRepeat(string url, bool set)
        {
            var playList = mongo.GetAllPlayLists().First();
            mongo.UpdatePlayListRepeat(playList.Id, set);
            if (set)
                mongo.UpdatePlayListShuffle(playList.Id, false);

            Clients.All.setRepeat(set, url);
        }

        #region Helpers
        public void checkInputStrings(string url, string title)
        {
            if (url.Length < 20 && title.Length < 100)
            {
                return;
            }
            else
            {
                throw new InputFormatException();
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