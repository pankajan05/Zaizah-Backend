using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class Review
    {
        [BsonId]
        public string id { get; set; }
        public int rating { get; set; }
        public string review { get; set; }

    }
}
