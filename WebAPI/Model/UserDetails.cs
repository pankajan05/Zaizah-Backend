using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Model
{
    
    public class UserDetails
    {

        [JsonProperty("idToken")]
        public string IdToken { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string StreetAddress { get; set; }
        public int PostalCode { get; set; }
        public string Email { get; set; }
        public UserRole UserRole { get; set; }
        public string Password { get; set; }
        public string PROVIDER { get; set; }
        public  ICollection<Item> cart { get; set; }
        public  ICollection<WishList> WishList { get; set; }
    }

}
