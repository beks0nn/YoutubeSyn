using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;

namespace WebApplication1.Models
{
    public class CurrUrl
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        [BsonElement("CurrUrl")]
        public Guid UrlIdentity { get; set; }
        [BsonElement("version")]
        public Guid version { get; set; }
        [BsonElement("time")]
        public string time { get; set; }
        [BsonElement("isRepeat")]
        public bool isRepeat { get; set; }
    }
}