using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Model;

namespace WebAPI.Data
{
    public interface IProductRepo
    {
        Task<IEnumerable<Product>> Get();
        Task<Product> Get(string id);
        Task Add(Product product);
        Task<string> Update(string id, Product product);
        Task<DeleteResult> Remove(string id);
    }
}
