using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Model;

namespace WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class WishlistController: ControllerBase
    {
        private readonly string username;
        private readonly ILogger<AuthController> _logger;
        private readonly IMongoDbData _mongoDbData;
        private readonly IProductRepo _productRepo;
        public WishlistController(ILogger<AuthController> logger, IMongoDbData mongoDbData, IHttpContextAccessor httpContextAccessor, IProductRepo productRepo)
        {
            _logger = logger;
            username = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            _mongoDbData = mongoDbData;
            _productRepo = productRepo;
        }

        [Authorize]
        [HttpGet]
        public async Task<String> Get()
        {
            var user = await _mongoDbData.GetUserDetails(username);
            return JsonConvert.SerializeObject(user.WishList);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> createlist([FromBody] Name name)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<WishList> lists = (List<WishList>)user.WishList;
            bool find = false;

            foreach (WishList list in lists)
            {
                if (list.Name == name.name)
                { 
                    find = true;
                    break;
                }
            }
            if (find)
            {
                return BadRequest(new { message = "Sorry Wishlist Already Available" });
            }
            else
            {
                WishList w = new WishList();
                w.Name = name.name;
                w.list = new Product[] { };
                user.WishList.Add(w);

                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Successfully created" });
            }
        }

        [Authorize]
        [HttpPost("{name}/{id}")]
        public async Task<IActionResult> add(string name, string id)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<WishList> lists = (List<WishList>)user.WishList;
            bool find = false;

            foreach (WishList wishlist in lists)
            {
                if (wishlist.Name == name)
                {
                    
                    foreach (Product p in wishlist.list)
                    {
                        if (p.id == id)
                        {
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                    {
                        wishlist.list.Add(await _productRepo.Get(id));
                        await _mongoDbData.Update(username, user);
                        return Ok(new { message = "Successfully added" });
                    }

                }
            }
            
            if(find)
            {
                return BadRequest(new { message = "Sorry invalid Wishlist." + name});
            }
            return BadRequest(new { message = "Error" });
        }


        [Authorize]
        [HttpDelete("{name}/{id}")]
        public async Task<IActionResult> Delete(string name, string id)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<WishList> lists = (List<WishList>)user.WishList;
            bool find = false;

            foreach (WishList wishlist in lists)
            {
                if (wishlist.Name == name)
                {
                    foreach(Product p in wishlist.list)
                    {
                        if(p.id == id)
                        {
                            wishlist.list.Remove(p);
                            find = true;
                            break;
                        }
                    }
                    
                }
            }
            if (find)
            {
                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Successfully delete" });
            }
            else
            {
                return BadRequest(new { message = "Sorry invalid Wishlist" });
            }
        }


        [Authorize]
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteList(string name)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<WishList> lists = (List<WishList>)user.WishList;
            bool find = false;

            foreach (WishList list in lists)
            {
                if (list.Name == name)
                {
                    user.WishList.Remove(list);
                    find = true;
                    break;
                }
            }
            if (find)
            {
                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Successfully delete" });
            }
            else
            {
                return BadRequest(new { message = "Sorry invalid Wishlist" });
            }
        }

    }
}
