﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;


namespace WebApplication1.Models
{
    public class Url
    {
        [BsonElement("Url")]
        public string UrlPart { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }
    }
}