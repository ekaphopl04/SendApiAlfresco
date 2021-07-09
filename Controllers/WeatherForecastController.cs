using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ApiClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiClient.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ApiClientController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ApiClientController> _logger;

        public ApiClientController(ILogger<ApiClientController> logger)
        {
            _logger = logger;
        }

        

       /* public static async Task RunAsync()
        {
          //  string result = await MakePost(Data data);
           //  Console.WriteLine("main end"+result);
              Console.WriteLine("main end");
        }*/

        [HttpPost()]

         public  async Task<string> MakePost(Data data)
        {    
            HttpClient client = new HttpClient();
            Console.WriteLine("RunAsync");

            data.userId = "admin" ;
            data.password = "abcABC123" ;
            string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true" ;
            Console.WriteLine("RunAsync"+url);
            var response = await client.PostAsJsonAsync(url,data);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            
            return responseString;
        }

        [HttpPost()]

        public  Task<string> SendApi(Data data)
        {
        
          string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true";
            using (var wb = new WebClient())
            {
                var json = "{\"userId\": \"admin\", ";
                json += "    \"password\": \"abcAbc123\"}";
               wb.Headers[HttpRequestHeader.ContentType] = "application/json";
                var response = wb.UploadString(url, "POST", json);
                Console.WriteLine(response.ToString());
                return Task.FromResult(response.ToString()); ;
                
            }
        }
        [HttpPost()]

        public string Send(Data data)
        {
            string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true";
            using (var wb = new WebClient())
            {
                var data1 = new NameValueCollection();
                data1["userId"] = "admin";
                data1["password"] = "abcABC123";
                var response = wb.UploadValues(url, "POST", data1);
                string responseString = Encoding.UTF8.GetString(response);
                return "";
            }
        }
        /*[HttpPost()]
        static async Task<string[]> SendRequest(Request request) {
            var url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets";
            using (var client = new HttpClient()) {
                var requestJson = ToJsonString(request);
                var response = await client.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
                var returnJson = await response.Content.ReadAsStringAsync();
                var returnObject = ToArray(returnJson);
                return (string[])returnObject;
            }
        }*/

    }
}



