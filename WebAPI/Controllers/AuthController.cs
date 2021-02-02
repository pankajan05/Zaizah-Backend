using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Data;
using WebAPI.Model;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IMongoDbData _mongoDbData;

        public AuthController(ILogger<AuthController> logger, IMongoDbData mongoDbData)
        {
            _logger = logger;
            _mongoDbData = mongoDbData;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("googleauthenticate")]
        public async Task<IActionResult> GoogleAuthenticate(UserDetails request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage));

            UserDetails user = await AuthenticateGoogleUserAsync(request);
            var token = GenerateToken(user.UserName);
            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("facebookauthenticate")]
        public async Task<IActionResult> FacebookAuthenticate(UserDetails request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage));

            UserDetails user = await AuthenticateFacebookUserAsync(request);
            var token = GenerateToken(user.UserName);
            return Ok(new { token });
        }

        public async Task<UserDetails> AuthenticateGoogleUserAsync(UserDetails request)
        {
            const string PROVIDER = "google";

            Payload payload = await ValidateAsync(request.IdToken, new ValidationSettings
            {
                Audience = new[] { Startup.StaticConfig["google:clientId"] }
            });

            return await GetOrCreateExternalLoginUser( PROVIDER, payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
        }

        public async Task<UserDetails> AuthenticateFacebookUserAsync(UserDetails request)
        {
            const string FBPROVIDER = "facebook";
       
            return await GetOrCreateExternalLoginUser(FBPROVIDER, request.UserId, request.Email, request.FirstName, request.LastName);
        }


        private async Task<UserDetails> GetOrCreateExternalLoginUser(string provider, string key, string email, string firstName, string lastName)
        {
            var user = await _mongoDbData.GetUserDetails(email);
            if (user != null)
                return user;
            else
            {
                user = new UserDetails
                {
                    PROVIDER = provider,
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName,
                    UserId = key,
                    Password = firstName,
                    cart = new Item[] { },
                    WishList = new WishList[] { }
                };
                await _mongoDbData.saveUserDetails(user);


                var newuser = await _mongoDbData.GetUserDetails(email);
                return newuser;
            }
        }



        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Get(UserDetails userdetails)
        {
            //var authBasicToken = HttpContext.Request.Headers["Authorization"].ToString();
            if (userdetails is null)
            {
                return BadRequest();
            }
            //var decodedAuthToken = Encoding.UTF8.GetString(Convert.FromBase64String(authBasicToken.Substring(6).Trim()));
            //var userNamePassword = decodedAuthToken.Split(":"); userNamePassword[0]


            var userDetails = await _mongoDbData.GetUserDetails(userdetails.UserName, userdetails.Password);
            if (userDetails != null)
            //if(userNamePassword[0] == "admin" && userNamePassword[1] == "pass")
            {
                var token = GenerateToken(userdetails.UserName);
                return Ok(new { token });
            }
            return BadRequest(new { message = "Username or Password incorrect" });
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Post(UserDetails userDetails)
        {
            var user = await _mongoDbData.GetUserDetails(userDetails.UserName);
            if (user != null)
            {
                return BadRequest(new { message = "user already registered using this email."});
            }
                try
            {
                userDetails.cart = new Item[] { };
                userDetails.WishList = new WishList[] { };
                 await _mongoDbData.saveUserDetails(userDetails).ConfigureAwait(false);
                return Ok(new { message = "successfully registered." });
            }catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }




        public string GenerateToken(string userName)
        {
            X509Certificate2 x509Certificate2 = new X509Certificate2(@"C:\Program Files\OpenSSL-Win64\bin\cert_key.p12", "1234");
            //X509Certificate2 x509Certificate2 = getCertificateFromStore();
            X509SecurityKey x509SecurityKey = new X509SecurityKey(x509Certificate2);

            var secToken = new JwtSecurityToken(
                "ZaiZah",
                "ZaiZah",
                claims: new[] { new Claim(ClaimTypes.Name, userName)},
                expires: DateTime.Now.AddDays(1),
                signingCredentials: new SigningCredentials(x509SecurityKey, SecurityAlgorithms.RsaSha256Signature)
                );

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            return jwtSecurityTokenHandler.WriteToken(secToken);
        }

        private X509Certificate2 getCertificateFromStore()
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificate2Collection = store.Certificates;
                X509Certificate2Collection currentCert = certificate2Collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signCert = currentCert.Find(X509FindType.FindBySubjectName,"ZaiZah", false);

                if(signCert.Count == 0)
                {
                    return null;
                }
                return signCert[0];
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
