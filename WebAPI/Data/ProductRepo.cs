using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Model;

namespace WebAPI.Data
{
    public class ProductRepo : IProductRepo
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Product> _productCol;

        public ProductRepo(IMongoClient mongoClient)
        {
            
            _mongoClient = mongoClient;
            var mongoDb = _mongoClient.GetDatabase("ZaiZah");
            _productCol = mongoDb.GetCollection<Product>("Product");
        }

        public async Task Add(Product product)
        {
            await _productCol.InsertOneAsync(product);
            Console.WriteLine(product);
        }

        public async  Task<IEnumerable<Product>> Get()
        {
            return await _productCol.Find(x => true).ToListAsync();
        }

        public async Task<Product> Get(string id)
        {
            var product = Builders<Product>.Filter.Eq("id", id);
            return await _productCol.Find(product).FirstOrDefaultAsync();
        }

        public async Task<DeleteResult> Remove(string id)
        {
            return await _productCol.DeleteOneAsync(Builders<Product>.Filter.Eq("id", id));
        }

        public async Task<string> Update(string id, Product product)
        {
            await _productCol.ReplaceOneAsync(z => z.id == id, product);
            return "Success";
        }
    }
}
