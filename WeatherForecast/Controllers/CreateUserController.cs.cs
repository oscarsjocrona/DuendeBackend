using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Web;
using static System.Net.WebRequestMethods;

namespace WeatherForecast.Controllers
{
    public class CreateUserController : Controller
    {
        private IConfiguration _configuration;

        public CreateUserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {

            string authUrl = _configuration["InteractiveServiceSettings:AuthorityUrl"];
            Uri uri = new Uri("https://localhost:5444/home/weather");
            if (authUrl == null)
            {
                throw new Exception("Auth is null");
            }

            //Path.Combine(authUrl, "Account/Create", $"returnurl={uri}")
            //UriBuilder uriBuilder = new UriBuilder(Path.Combine(authUrl,
            //    "Account/Create"));
            //var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            //query["returnUrl"] = System.Web.HttpUtility.UrlEncode("https://localhost:5444/home/weather");
            //uriBuilder.Query = query.ToString();

            UriBuilder uriBuilder = new UriBuilder("https://localhost");
            uriBuilder.Port = 5443;
            uriBuilder.Path = "Account/Create";
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["returnUrl"] = System.Web.HttpUtility.UrlEncode("https://localhost:5444/home/weather");
            uriBuilder.Query = query.ToString();

            //var path = Path.Combine(authUrl,
            //    "Account/Create");
            //$"returnurl={System.Web.HttpUtility.UrlEncode("https://localhost:5444/home/weather")}");

            return Redirect(uriBuilder.ToString());
        }
    }
}
