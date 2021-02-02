using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Model;

namespace WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class ProductController: ControllerBase
    {
        private readonly IProductRepo _productRepo;
        private readonly ILogger<AuthController> _logger;
        private readonly IMongoDbData _mongoDbData;
        private string username;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(ILogger<AuthController> logger, IProductRepo productRepo, IMongoDbData mongoDbData, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _productRepo = productRepo;
            _mongoDbData = mongoDbData;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            return await this.GetProduct();
        }

        private async Task<string> GetProduct()
        {
            var products = await _productRepo.Get();
            return JsonConvert.SerializeObject(products);
        }

        [HttpGet("{id}")]
        public Task<string> Get(string id)
        {
            return this.GetProductById(id);
        }

        public async Task<string> GetProductById(string id)
        {
            var product = await _productRepo.Get(id) ?? new Model.Product();
            return JsonConvert.SerializeObject(product);
        }

        [HttpPost]
        public async Task<string> Post([FromBody] Product product)
        {
            await _productRepo.Add(product);
            return "";
        }

        [HttpPut("{id}")]
        public async Task<string> Put(string id, [FromBody] Product product)
        {
            if (string.IsNullOrEmpty(id)) return "Invalid id !!!";
            return await _productRepo.Update(id, product);
        }

        [HttpDelete("{id}")]
        public async Task<string> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return "Invalid id";
            await _productRepo.Remove(id);

            return "";
        }

        [Authorize]
        [Route("review/{id}")]
        [HttpPost]
        public async Task<IActionResult> Addreview(string id, [FromBody] Review review)
        {

            username = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;

            Product product = await _productRepo.Get(id);
            var user = await _mongoDbData.GetUserDetails(username);

            if (product == null && user == null)
            {
                return BadRequest(new { message = "Sorry invalid" });
            }
            review.id = user.FirstName + " " + user.LastName;
            product.reviews.Add(review);
            await _productRepo.Update(id, product);

            return Ok(new { message = "your review successfully added." });
        }
    }

}
