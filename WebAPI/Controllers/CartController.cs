using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Model;

namespace WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class CartController: ControllerBase
    {
        
        private readonly string username;
        private readonly ILogger<AuthController> _logger;
        private readonly IMongoDbData _mongoDbData;
        private readonly IProductRepo _productRepo;
        public CartController(ILogger<AuthController> logger, IMongoDbData mongoDbData, IHttpContextAccessor httpContextAccessor, IProductRepo productRepo)
        {
            _logger = logger;
            username = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            _mongoDbData = mongoDbData;
            _productRepo = productRepo;
        }


        [HttpGet]
        [Authorize]
        public async Task<string> Get()
        {
            var user = await _mongoDbData.GetUserDetails(username);
            return JsonConvert.SerializeObject(user.cart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            Product v_product = await _productRepo.Get(product.id);

            List<Item> items = (List<Item>)user.cart;
            bool ADD = true;
            foreach (Item item in items)
            {
                if (item.product.id == v_product.id)
                {
                    item.Quantity += 1;
                    ADD = false;
                    break;
                }

            }
            if (ADD) { 
                Item i = new Item();
                i.product = v_product;
                i.Quantity = 1;
                user.cart.Add(i);
            }
            else
            {
                user.cart = items;
            }
            await _mongoDbData.Update(username, user);
            return Ok(new { message = "Successfully Added" });
;
        }

        [Authorize]
        [Route("inc")]
        [HttpPost]
        public async Task<IActionResult> increase([FromBody] Product product)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<Item> items = (List<Item>)user.cart;
            bool find = false; 
            foreach (Item item in items)
            {
                if (item.product.id == product.id)
                {
                    item.Quantity += 1;
                    find = true;
                    break;
                }

            }
            if (find)
            {
                user.cart = items;
                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Increased" });
            }
            else
            return BadRequest(new { message = "Sorry invalid" });
        }

        [Authorize]
        [Route("dec")]
        [HttpPost]
        public async Task<IActionResult> decrease([FromBody] Product product)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<Item> items = (List<Item>)user.cart;
            bool find = false;
            foreach (Item item in items)
            {
                if (item.product.id == product.id)
                {
                    item.Quantity -= 1;
                    find = true;
                    break;
                }

            }
            if (find)
            {
                user.cart = items;
                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Decreased" });
            }
            else
                return BadRequest(new { message = "Sorry invalid" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _mongoDbData.GetUserDetails(username);
            List<Item> items = (List<Item>)user.cart;
            bool find = false;
            foreach (Item item in items)
            {
                if (item.product.id == id)
                {
                    user.cart.Remove(item);
                    find = true;
                    break;
                }
            }
            if (find)
            {
                await _mongoDbData.Update(username, user);
                return Ok(new { message = "Successfully delete" });
            } else
            {
                return BadRequest(new { message = "Sorry invalid Id" });
            }

            

        }
    }
}
