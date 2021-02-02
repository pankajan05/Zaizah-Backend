using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public float price { get; set; }
        public string[] imageUrl { get; set; }
        public int rating { get; set; }
        public string[] color { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public ICollection<Review> reviews { get; set; }
    }
}
