using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.IO;
using Microsoft.Extensions.Configuration;
using AlfrescoAuthApi;
using km.settings;
using AlfrescoApi.Custom;

namespace ApiClient.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ApiClientController : ControllerBase
    {

        private readonly ILogger<ApiClientController> _logger;

        public ApiClientController(ILogger<ApiClientController> logger )
        {
            _logger = logger;
        }
        public class AlfrescoService 
     {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<AlfrescoService> _logger;
        private readonly Appsettings _settings ;
        
        
        public AlfrescoService(Appsettings settings,IHttpClientFactory factory,ILogger<AlfrescoService> logger , IConfiguration configuration){
            
            _settings = settings ;
            _factory = factory ;
            _logger = logger ;
            this.configuration = configuration ;
            
        }
        [HttpPost()]
        public async Task<string> Upload(FileInfo info,UploadProperty property){
        var ticket = await this.GetTicketId(_settings.Alfresco.User,_settings.Alfresco.Password);
        var client = new AlfrescoApi.Custom.CustomApiClient(_settings.Alfresco.Url,ticket);
        try {
            var http = _factory.CreateClient("alfresco");
            var result = await client.StartUploadFile(http,info , property);
            Console.WriteLine(result.Error);
            Console.WriteLine(result.Result.Entry.Id);
            return result.Result.Entry.Id;
        }catch (Exception ex){
                _logger.LogError(ex.ToString());
                return null  ;
            }

        }
        
         private async Task<string> GetTicketId(string userId,string password){
             var client = _factory.CreateClient("alfresco");
             var auth = new AlfrescoAuthApi.AlfrescoAuth(client);
             var response = (await auth.CreateTicketAsync(new TicketBody { UserId = userId , Password= password})).Entry.Id;
             return response;
         }
     }
           private string ToJsonString(Request request){
            request.userId =  "" ;
            request.password = "" ;
           var serializer = new DataContractJsonSerializer(typeof(Request));
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, request);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

      
        [HttpPost()]

         public  async Task<string> MakePost(Data data)
        {    
            HttpClient client = new HttpClient();
            var request = new Request();
            request.userId = "admin" ;  request.password = "abcABC123" ;
            string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets" ;
            var response = await client.PostAsJsonAsync(url,request);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            string urlpload = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true" ;
            //var responseUploadFile = await client.PostAsync(responseString,data,json);
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

        public  Task<string> testSendApitoken(Data data)
        {
        
          string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets";
            using (var wb = new WebClient())
            {
                var json = "{\"userId\": \"admin\", ";
                json += "    \"password\": \"abcABC123\"}";
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
          /*
        
        [HttpPost()]
        static async Task<string> SendRequest(Request request) {
            var url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets";
            
            using (var client = new HttpClient()) {
                string json = JsonSerializer.Seriallize(request);
                var requestJson = ToJsonString(Request);
                var response = await client.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
                var returnJson = await response.Content.ReadAsStringAsync();
                
                return "";
            }
        }
        [HttpPost()]
        public async ActionResult test(Request flie){
        using(var client = new HttpClient()){
        var responseToken = await api รับโทเคน ;
         var responseUploadFile = await client.PostAsync(
             "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets",json,"application/json") ;
             Uploadfile(resonseToken, File, Json);



            }
        }
        [HttpPost()]

         static async Task<string> SendRequest(Request request){
        var url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets" ;
        using(var httpclient = new HttpClient()){
             var data = new Data(){userId = "admin", password = "adbABC123"};
        var newdata = JsonSerializer.Serialize(data);
        var stringContent = new StringContent(newdata,Encoding.UTF8,"Application/json");
        var responseToken = await httpclient.PostAsync(url,stringContent);
       
       var url = ""
         var responseUploadFile = await client.PostAsync(,json,"application/json") ;
             Uploadfile(resonseToken, File, Json);

        }
        */
            
        
        }
    }




