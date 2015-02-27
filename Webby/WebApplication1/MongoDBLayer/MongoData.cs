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
                url.Id = Guid.NewGuid();
                playListId = collection.FindAll().First().Id;//temporary
                collection.Update(Query<PlayList>.EQ(e => e.Id, playListId), Update.PushWrapped("UrlList", url));
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

/// <summary>
/// URLS
/// </summary>
/*
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

public void DeleteUrl(string url)
{
    var collection = getUrlsCollectionForEdit();
    try
    {
        var query = Query<Url>.EQ(e => e.UrlPart, url);
        collection.Remove(query);
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
*/
/// <summary>
/// CURRENT URLS
/// </summary>
/*
public void updateCurrUrl(Guid id, Guid pointTo)
{
    var collection = getCurrUrlsCollectionForEdit();
    var query = Query<CurrUrl>.EQ(e => e.Id, id);
    var update = Update<CurrUrl>.Set(e => e.UrlIdentity, pointTo).Set(e => e.isRepeat, false).Set(e=> e.version, Guid.NewGuid()).Set(e => e.time,"0"); // update modifiers
    collection.Update(query, update);
}

public void updateCurrUrlTime(Guid id, string time)
{
    var collection = getCurrUrlsCollectionForEdit();
    var query = Query<CurrUrl>.EQ(e => e.Id, id);
    var update = Update<CurrUrl>.Set(e => e.time, time).Set(e => e.version,Guid.NewGuid()); // update modifiers
    collection.Update(query, update);
}

public void updateCurrUrlRepeat(Guid id, bool setBool)
{
    var collection = getCurrUrlsCollectionForEdit();
    var query = Query<CurrUrl>.EQ(e => e.Id, id);
    var update = Update<CurrUrl>.Set(e => e.isRepeat, setBool).Set(e => e.version, Guid.NewGuid()); // update modifiers
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
*/
/// <summary>
/// URlLists
/// </summary>