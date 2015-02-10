using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using MongoDB.Driver;
using System.Configuration;
using MongoDB.Driver.Builders;

namespace WebApplication1.MongoDBLayer
{
    public class MongoData : IDisposable
    {
        private MongoServer mongoServer = null;
        private bool disposed = false;
        private string connectionString = "mongodb://localhost:27017";
        MongoUrl url;

        private string dbName = "YouSync";
        private string urls = "Urls";
        private string currUrls = "CurrUrls";

        public MongoData()
        {
            url = new MongoUrl(connectionString);
        }

        /// <summary>
        /// URLS
        /// </summary>
        public List<Url> GetAllUrls()
        {
            try
            {

                MongoCollection<Url> collection = GetUrlsCollection();
                return collection.FindAll().ToList<Url>();
            }
            catch (MongoConnectionException)
            {
                return new List<Url>();
            }
        }

        public void CreateUrl(Url url)
        {
            MongoCollection<Url> collection = getUrlsCollectionForEdit();
            try
            {
                collection.Insert(url);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        private MongoCollection<Url> GetUrlsCollection()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<Url> urlColl = database.GetCollection<Url>(urls);
            return urlColl;
        }

        private MongoCollection<Url> getUrlsCollectionForEdit()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<Url> urlColl = database.GetCollection<Url>(urls);
            return urlColl;
        }

        /// <summary>
        /// CURRENT URLS
        /// </summary>

        public void updateCurrUrl(Guid id, Guid pointTo)
        {
            var collection = getCurrUrlsCollectionForEdit();
            var query = Query<CurrUrl>.EQ(e => e.Id, id);
            var update = Update<CurrUrl>.Set(e => e.UrlIdentity, pointTo).Set(e=> e.version, Guid.NewGuid()); // update modifiers
            collection.Update(query, update);
        }
        public void updateCurrUrlTime(Guid id, string time)
        {
            var collection = getCurrUrlsCollectionForEdit();
            var query = Query<CurrUrl>.EQ(e => e.Id, id);
            var update = Update<CurrUrl>.Set(e => e.time, time).Set(e => e.version,Guid.NewGuid()); // update modifiers
            collection.Update(query, update);
        }


        public List<CurrUrl> GetAllCurrUrls()
        {
            try
            {
                MongoCollection<CurrUrl> collection = getCurrUrlColl();
                return collection.FindAll().ToList<CurrUrl>();
            }
            catch (MongoConnectionException)
            {
                return new List<CurrUrl>();
            }
        }

        public void CreateCurrUrl(CurrUrl url)
        {
            MongoCollection<CurrUrl> collection = getCurrUrlsCollectionForEdit();
            try
            {
                collection.Insert(url);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        private MongoCollection<CurrUrl> getCurrUrlColl()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<CurrUrl> urlColl = database.GetCollection<CurrUrl>(currUrls);
            return urlColl;
        }

        private MongoCollection<CurrUrl> getCurrUrlsCollectionForEdit()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<CurrUrl> urlColl = database.GetCollection<CurrUrl>(currUrls);
            return urlColl;
        }


        # region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (mongoServer != null)
                    {
                        this.mongoServer.Disconnect();
                    }
                }
            }

            this.disposed = true;
        }

        # endregion
    }
}

