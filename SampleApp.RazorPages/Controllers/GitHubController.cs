using Microsoft.AspNetCore.Mvc;
using SampleApp.RazorPages.Extensions;
using SampleApp.RazorPages.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SampleApp.RazorPages.Controllers
{
    public class GitHubController : Controller
    {
        private static HttpClient _client = new HttpClient();

        public IActionResult Test() => View();

        [HttpPost]
        public async Task<ContentResult> Source()
        {
            var request = await Request.DeserializeAsync<GitHubSourceRequest>();

            var response = await _client.GetAsync(new Uri(request.Url));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                content = HttpUtility.HtmlEncode(content);
                
                if (request.Links != null)
                {
                    foreach (var link in request.Links)
                    {
                        content = content.Replace(link.Text, $"<span data-tooltip=\"{link.Target}\" class=\"{link.CssClass}\">{link.Text}</span>");
                    }
                }

                return Content(content, "text/html");
            }

            throw new Exception(response.ReasonPhrase);
        }
    }
}
