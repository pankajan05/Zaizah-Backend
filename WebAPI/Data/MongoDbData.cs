using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Model;

namespace WebAPI.Data
{
    public class MongoDbData : IMongoDbData
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<UserDetails> _userdetailsCol;

        public MongoDbData(IMongoClient mongoClient)
        {
            BsonClassMap.RegisterClassMap<UserDetails>(c =>
            {
                c.AutoMap();
                c.SetIgnoreExtraElements(true);
            });
            _mongoClient = mongoClient;
            var mongoDb = _mongoClient.GetDatabase("ZaiZah");
            _userdetailsCol = mongoDb.GetCollection<UserDetails>("UserDetails");
        }


        public async Task<UserDetails> GetUserDetails(string userName, string password)
        {
            var userDetails = await _userdetailsCol.FindAsync(f => f.UserName == userName && f.Password == password);
            while(await userDetails.MoveNextAsync()) {
                if (userDetails.Current.Any())
                {
                    return userDetails.Current.FirstOrDefault();
                }
            }
            return null;
        }


        public async Task<UserDetails> GetUserDetails(string userName)
        {
            var userDetails = await _userdetailsCol.FindAsync(f => f.UserName == userName);
            while (await userDetails.MoveNextAsync())
            {
                if (userDetails.Current.Any())
                {
                    return userDetails.Current.FirstOrDefault();
                }
            }
            return null;
        }


        public Task saveUserDetails(UserDetails userDetails)
        {
            return _userdetailsCol.InsertOneAsync(userDetails);
        }

        public async Task<string> Update(string username, UserDetails userDeatails)
        {
            await _userdetailsCol.ReplaceOneAsync(z => z.UserName == username, userDeatails);
            return "Success";
        }
    }
}
