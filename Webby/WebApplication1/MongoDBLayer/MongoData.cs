using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;
using MongoDB.Driver;
using System.Configuration;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace WebApplication1.MongoDBLayer
{
    public class MongoData : IDisposable
    {
        private MongoServer mongoServer = null;
        private MongoUrl url;
        private string connectionString = "mongodb://localhost:27017";
        private string playLists = "PlayLists";
        private string dbName = "YouSync";
        private bool disposed = false;


        public MongoData()
        {
            url = new MongoUrl(connectionString);
        }

        public void UpdatePlayListCurrentUrl(Guid id, string pointTo)
        {
            var collection = GetPlayListForEdit();
            var query = Query<PlayList>.EQ(e => e.Id, id);
            var update = Update<PlayList>.Set(e => e.CurrentUrl, pointTo).Set(e => e.isRepeat, false).Set(e => e.version, Guid.NewGuid()).Set(e => e.time, "0"); // update modifiers
            collection.Update(query, update);
        }

        public void UpdatePlayListRepeat(Guid id, bool setBool)
        {
            var collection = GetPlayListForEdit();
            var query = Query<PlayList>.EQ(e => e.Id, id);
            var update = Update<PlayList>.Set(e => e.isRepeat, setBool).Set(e => e.version, Guid.NewGuid()); // update modifiers
            collection.Update(query, update);
        }

        public Guid UpdatePlayListTime(Guid id, string time)
        {
            var newVersion = Guid.NewGuid();
            var collection = GetPlayListForEdit();
            var query = Query<PlayList>.EQ(e => e.Id, id);
            var update = Update<PlayList>.Set(e => e.time, time).Set(e => e.version, newVersion); // update modifiers
            collection.Update(query, update);

            return newVersion;
        }

        public void AddUrlToList(Url url, Guid playListId)
        {
            MongoCollection<PlayList> collection = GetPlayListForEdit();
            try
            {
                playListId = collection.FindAll().First().Id;//temporary
                collection.Update(Query<PlayList>.EQ(e => e.Id, playListId), Update.AddToSetWrapped("UrlList", url));
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        public void RemoveUrlFromList(string url)
        {
            MongoCollection<PlayList> collection = GetPlayListForEdit();
            try
            {
                var playListId = collection.FindAll().First().Id;//TODOS Only 1 playlist for now as there are no users.
                //var pull = Update<PlayList>.Pull(e => e.UrlList, builder => builder.EQ(q => q.UrlPart, url));
                //collection.Update(Query.And(Query.EQ("UrlList.Url", url)), pull);
                var query = Query<PlayList>.EQ(e=>e.Id, playListId);
                var update = Update.Pull("UrlList", new BsonDocument() { { "Url", url } });
                collection.Update(query, update);

            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        public List<PlayList> GetAllPlayLists()
        {
            try
            {
                MongoCollection<PlayList> collection = GetPlayLists();
                return collection.FindAll().ToList<PlayList>();
            }
            catch (MongoConnectionException)
            {
                return new List<PlayList>();
            }
        }

        public void CreatePlayList(PlayList playList)
        {
            MongoCollection<PlayList> collection = GetPlayListForEdit();
            try
            {
                collection.Insert(playList);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        public void DeletePlayList(Guid id)
        {
            var collection = GetPlayListForEdit();
            try
            {
                var query = Query<PlayList>.EQ(e => e.Id, id);
                collection.Remove(query);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }

        private MongoCollection<PlayList> GetPlayLists()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<PlayList> urlColl = database.GetCollection<PlayList>(playLists);
            return urlColl;
        }

        private MongoCollection<PlayList> GetPlayListForEdit()
        {
            MongoClient client = new MongoClient(url);
            mongoServer = client.GetServer();
            MongoDatabase database = mongoServer.GetDatabase(dbName);
            MongoCollection<PlayList> urlColl = database.GetCollection<PlayList>(playLists);
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