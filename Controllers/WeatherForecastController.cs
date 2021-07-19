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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace ApiClient.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ApiClientController : ControllerBase
    {

        private readonly ILogger<ApiClientController> _logger;

        public ApiClientController(Appsettings settings, IHttpClientFactory factory, ILogger<ApiClientController> logger, IConfiguration configuration,IWebHostEnvironment webHostEnvironment)
        {
            _settings = settings;
            _factory = factory;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment ;
            this.configuration = configuration;

        }
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory _factory;
        private readonly Appsettings _settings;
        
          private async void Connecting()
        {
            var ticket = await this.GetTicketId("admin","abcABC123");
            var client = new AlfrescoApi.Custom.CustomApiClient("https://beflextest.bcecm.com", ticket);
           
            
        }
        private async Task<string> GetIdToken(string userId, string password)
        {
            var client = _factory.CreateClient("alfresco");
            var auth = new AlfrescoAuthApi.AlfrescoAuth(client);
            var response = (await auth.CreateTicketAsync(new TicketBody { UserId = userId, Password = password })).Entry.Id;
            return response;
        }
        private async Task<bool> StartUpload(byte[] bytes, string path, string name) {
            using var client = new HttpClient {
                BaseAddress = new Uri("https://beflextest.bcecm.com")
            };
            var user = "admin";
            var password = "abcABC123";
            var userPassword = user+":"+password;
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(userPassword));
            Console.WriteLine("Username is " + user);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);
            var content = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(bytes);

            content.Add(byteContent, "filedata", "default");
            content.Add(new StringContent(name), "cm:title");
            content.Add(new StringContent(path), "relativePath");
            content.Add(new StringContent("true"), "autoRename");

            var response = await client.PostAsync("/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children", content);
            var body = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject(body);
            var pretty = JsonConvert.SerializeObject(obj, Formatting.Indented);

            if (response.StatusCode == System.Net.HttpStatusCode.Created) {
                Console.WriteLine("> Success");
                Console.WriteLine(pretty);
                return true;
            } else {
                Console.WriteLine("! Error {0}", response.StatusCode);
                Console.WriteLine(pretty);
                return false;
            }
        }


        [HttpPost]
        public async Task<ActionResult<dynamic>> UploadFile([FromForm] UploadRequest request) {
            var memory = new MemoryStream();
            request.File.CopyTo(memory);

            var bytes = memory.ToArray();
            var path = request.TargetPath;
            var name = request.TargetName;

            var ok = await StartUpload(bytes, path, name);
            return Ok(new { Success = ok });
        }
      


        [HttpPost]
        public async Task<string> Connect2(Request request)
        {
            var ticket = await this.GetTicketId(request.userId,request.password);
            var client = new AlfrescoApi.Custom.CustomApiClient("https://beflextest.bcecm.com", ticket);
            return "";
            
        }



        [HttpPost()]
        public async Task<string> Upload(FileRequest value,IFormFile file)
        {
            var ticket = await this.GetTicketId("admin","abcABC123");
            var client = new AlfrescoApi.Custom.CustomApiClient("https://beflextest.bcecm.com", ticket);
            try
            {  
                var http = _factory.CreateClient("alfresco");
                var result = await client.StartUploadFile(http, value.info, value.property);
                Console.WriteLine(result.Error);
               Console.WriteLine(result.Result.Entry.Id);
               using (var client1 = new HttpClient()){
                client1.BaseAddress = new Uri("https://beflextest.bcecm.com");
                    
                    byte[] data;
                    using (var br = new BinaryReader(file.OpenReadStream()))
                        data = br.ReadBytes((int)file.OpenReadStream().Length);

                    ByteArrayContent bytes = new ByteArrayContent(data);

                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    
                    multiContent.Add(bytes, "file", file.FileName);

                    var result1 = client1.PostAsync(
                        "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true", multiContent).Result;
                    
                }
                
                
              //  return result.Result.Entry.Id;
              return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
            
            

        }
  /*      [HttpPost]
public async Task Upload123([FromForm]UploadModel model)
{
    if (model.File == null) throw new Exception("File is null");
    if (model.File.Length == 0) throw new Exception("File is empty");
    model.Var1 += "hello world";

    using (Stream stream = model.File.OpenReadStream())
    {
        using (var binaryReader = new BinaryReader(stream))
        {
           // Save the file here.
        }
    }
}*/

         [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file.Length > 0)
            {
                string filePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "images", file.FileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
            }
            return Ok(new { fileUrl = $"{this.Request.Scheme}://{this.Request.Host}/images/{file.FileName}" });
        }
        [HttpPost()]
        public async Task<string> Uploadold(FileRequest value,IFormFile file)
        {
            var ticket = await this.GetTicketId(_settings.Alfresco.User, _settings.Alfresco.Password);
            var client = new AlfrescoApi.Custom.CustomApiClient(_settings.Alfresco.Url, ticket);
            try
            {  
                var http = _factory.CreateClient("alfresco");
                var result = await client.StartUploadFile(http, value.info, value.property);
                Console.WriteLine(result.Error);
                Console.WriteLine(result.Result.Entry.Id);
                using (var client1 = new HttpClient()){
                client1.BaseAddress = new Uri("https://beflextest.bcecm.com");
                    
                    byte[] data;
                    using (var br = new BinaryReader(file.OpenReadStream()))
                        data = br.ReadBytes((int)file.OpenReadStream().Length);

                    ByteArrayContent bytes = new ByteArrayContent(data);

                    
                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    
                    multiContent.Add(bytes, "file", file.FileName);

                    var result1 = client1.PostAsync("https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true", multiContent).Result;
                    
                }
                
                
                return //result.Result.Entry.Id;
            "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
            
            

        }

        /*[HttpPost]
public void Post()
{
    Stream bodyStream = HttpContext.Request.Body;

    if (Request.HasFormContentType)
    {
        var form = Request.Form;
        foreach (var formFile in form.Files)
        {
            var targetDirectory = Path.Combine(_appEnvironment.WebRootPath, "uploads");

            var fileName = GetFileName(formFile);

            var savePath = Path.Combine(targetDirectory, fileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                formFile.CopyTo(fileStream);
            }                   
        }
    }          
}*/

    private StatusCodeResult Post1(IFormFile file)
{
    try
    {
        if (file != null && file.Length > 0)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri("https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true");
                    
                    byte[] data;
                    using (var br = new BinaryReader(file.OpenReadStream()))
                        data = br.ReadBytes((int)file.OpenReadStream().Length);

                    ByteArrayContent bytes = new ByteArrayContent(data);

                    
                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    
                    multiContent.Add(bytes, "file", file.FileName);

                    var result = client.PostAsync("api/Values", multiContent).Result;
                    

                    return StatusCode((int)result.StatusCode); //201 Created the request has been fulfilled, resulting in the creation of a new resource.
                                                
                }
                catch (Exception)
                {
                    return StatusCode(500); // 500 is generic server error
                }
            }
        }

        return StatusCode(400); // 400 is bad request

    }
    catch (Exception)
    {
        return StatusCode(500); // 500 is generic server error
    }
}
        private async Task<string> GetTicketId(string userId, string password)
        {
            var client = _factory.CreateClient("alfresco");
            var auth = new AlfrescoAuthApi.AlfrescoAuth(client);
            var response = (await auth.CreateTicketAsync(new TicketBody { UserId = userId, Password = password })).Entry.Id;
            return response;
        }

        private string ToJsonString(Request request)
        {
            request.userId = "";
            request.password = "";
            var serializer = new DataContractJsonSerializer(typeof(Request));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, request);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }


        [HttpPost()]

        public async Task<string> MakePost(Data data)
        {
            HttpClient client = new HttpClient();
            var request = new Request();
            request.userId = "admin"; request.password = "abcABC123";
            string url = "https://beflextest.bcecm.com/alfresco/api/-default-/public/authentication/versions/1/tickets";
            var response = await client.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            string urlpload = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true";
            //var responseUploadFile = await client.PostAsync(responseString,data,json);
            return responseString;
        }

        [HttpPost()]

        public Task<string> SendApi(Data data)
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

        public Task<string> testSendApitoken(Data data)
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


