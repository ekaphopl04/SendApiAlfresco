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
using DataBase.Models;
using System.Linq;
using JsonProperty;
using System.Collections.Generic;
using Getdatajson.Models;
namespace ApiClient.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ApiClientController : ControllerBase
    {

        private readonly ILogger<ApiClientController> _logger;

        private readonly ReportContext _db;

        public ApiClientController(Appsettings settings, IHttpClientFactory factory, ILogger<ApiClientController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ReportContext db)
        {
            _settings = settings;
            _db = db;
            _factory = factory;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            this.configuration = configuration;

        }
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory _factory;
        private readonly Appsettings _settings;

        private async void Connecting()
        {
            var ticket = await this.GetTicketId("admin", "abcABC123");
            var client = new AlfrescoApi.Custom.CustomApiClient("https://beflextest.bcecm.com", ticket);


        }
        private async Task<string> GetIdToken(string userId, string password)
        {
            var client = _factory.CreateClient("alfresco");
            var auth = new AlfrescoAuthApi.AlfrescoAuth(client);
            var response = (await auth.CreateTicketAsync(new TicketBody { UserId = userId, Password = password })).Entry.Id;
            return response;
        }

        private ActionResult PostForDatabase(string UserId, string Name, string Path, string CustomerId, string IdUpload, string Type, string TypeName, bool Active)
        {
            var UserCheck = _db.ReportTable.Where(x => x.UserId == UserId).Select(x => x.UserId).FirstOrDefault();
            if (UserCheck != UserId && UserId != null)
            {
                _db.ReportTable.Add(new Report
                {
                    UserId = UserId,
                    NamePath = Name,
                    Path = Path,
                    CustomerId = CustomerId,
                    IdUpload = IdUpload,
                    Type = Type,
                    TypeName = TypeName,
                    Active = Active
                });

                _db.SaveChanges();
                return Ok();

            }
            else
            {
                return BadRequest();
            }
        }
        private async Task<bool> StartUpload(byte[] bytes, string path, string name, List<PropertyInput> propertyInput)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://beflexdemo.bcecm.com/")
            };
            //var oj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(textJson);
            //Console.WriteLine("PropertyName"+PropertyInput.PropertyName[0]);
            //Console.WriteLine("PropertyValue"+PropertyInput.PropertyValue[0]);
            //Console.WriteLine("PropertyName"+PropertyInput.PropertyName[1]);
            //Console.WriteLine("PropertyValue"+PropertyInput.PropertyValue[1]);
            // Console.WriteLine("PropertyName"+PropertyInput.ToString());
            // Console.WriteLine("PropertyName"+PropertyInput.ToString());
            /*Console.WriteLine("PropertyName"+PropertyInput[0].PropertyName);
            Console.WriteLine("PropertyValue"+PropertyInput[0].PropertyValue);
            Console.WriteLine("PropertyName"+PropertyInput[1].PropertyName);
            Console.WriteLine("PropertyValue"+PropertyInput[1].PropertyValue);*/

            var user = "admin";
            var password = "admin";
            var userPassword = user + ":" + password;
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(userPassword));
            Console.WriteLine("Username is " + user);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);
            var content = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(bytes);
            content.Add(byteContent, "filedata", name);
            content.Add(new StringContent(name), "cm:title");
            //content.Add(new StringContent(CustomerId), "test:test1");
            content.Add(new StringContent(path), "relativePath");
            content.Add(new StringContent("true"), "autoRename");
            foreach (var str in propertyInput)
            {
                content.Add(new StringContent(str.PropertyValue), str.PropertyName);
            }


            Console.WriteLine(name);
            Console.WriteLine(path);
            Console.WriteLine(bytes);


            Console.WriteLine("Contentttttt" + content);
            var response = await client.PostAsync("/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children", content);
            Console.WriteLine("Resprosee" + response);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("222" + body + "2222");
            //แปลงจากstringเป็นObject
            var obj = JsonConvert.DeserializeObject(body);
            Console.WriteLine("Id:5555555555 " + obj);


            //Getdatajson objjson = JsonConvert.DeserializeObject<Getdatajson>(body);
            //  Console.WriteLine(objjson.id+"5");
            //   Console.WriteLine(objjson.name);
            //	Console.WriteLine(ob.entry.id);

            /*     RootObject objectjson = JsonConvert.DeserializeObject<RootObject>(body);
               foreach (KeyValuePair<string, JobCode> kvp in objectjson.Results.JobCodes)
              {
                  Console.WriteLine("Id: " + kvp.Value.id);
                  Console.WriteLine("Name: " + kvp.Value.displayName);
                  Console.WriteLine();
              }
                  Console.WriteLine("333333"+objectjson.Results.JobCodes+"333");
                  Console.WriteLine("333"+obj+"333");*/

            //แปลงจากobjectเป็นjsonstring

            var pretty = JsonConvert.SerializeObject(obj, Formatting.Indented);
            //var json = File.ReadAllText(pretty);
            var objectjson = JsonConvert.DeserializeObject<Info>(pretty);
            //ดึงค่าjson จากคลาส
            //Console.WriteLine(objectjson.Entry.CreatedByUser.Id + "ffffffffff");
            //ดึงค่าjson จากDictionary String string
            //Console.WriteLine(objectjson.Entry.Properties["cm:title"]+"ffffffffff");
            //Console.WriteLine(objectjson.Entry.content.MimeType + "111111111");
            //รับค่าจากJsonมาเก็บในตัวแปร 
            var IdUpload = objectjson.Entry.Id;
            var Type = objectjson.Entry.content.MimeType;
            var TypeName = objectjson.Entry.content.MimeTypeName;
            var CustomerId = "";
            bool active = false;

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine("> Success");
                Console.WriteLine(pretty);
                active = true;
                PostForDatabase(user, name, path, CustomerId, IdUpload, Type, TypeName, active);
                return true;
            }
            else
            {
                Console.WriteLine("! Error {0}", response.StatusCode);
                Console.WriteLine(pretty);
                PostForDatabase(user, name, path, CustomerId, IdUpload, Type, TypeName, active);
                return false;
            }

        }


        [HttpPost]
        public async Task<ActionResult<dynamic>> UploadFile([FromForm] UploadRequest request)
        {
            var memory = new MemoryStream();
            request.File.CopyTo(memory);

            var bytes = memory.ToArray();
            var path = request.TargetPath;
            var name = request.TargetName;
            // var textJson = request.textJson; 

            var requestJson = JsonConvert.DeserializeObject<dynamic>(request.PropertyInput);
            //    Console.WriteLine(rq);
            List<PropertyInput> propertylist = new List<PropertyInput>();
            foreach (var str in requestJson)
            {
                Console.WriteLine("11111111+"+JsonConvert.SerializeObject(str.PropertyName));
                Console.WriteLine("22222222+"+JsonConvert.SerializeObject(str.PropertyValue));
                propertylist.Add(new PropertyInput { PropertyName = str.PropertyName, PropertyValue = str.PropertyValue });

            }
            // var inputPP = JsonConvert.DeserializeObject<PropertyInput>(JsonConvert.SerializeObject(request.PropertyInput));
            // Console.WriteLine("inputPP");

            // Console.WriteLine(JsonConvert.SerializeObject(inputPP));
            // Console.WriteLine("------");



            // var sss = JsonConvert.SerializeObject(PropertyInput, Formatting.Indented);
            //  var ss2 = JsonConvert.SerializeObject(request.PropertyInput, Formatting.Indented);
            // Console.WriteLine("แปลงเป็นjsonแล้วแสดง"+sss);
            // Console.WriteLine("แปลงเป็นjsonแล้วแสดงแบบที่2"+ss2);
            // Console.WriteLine("PropertyNameแบบแรก"+request.PropertyInput.ToString());
            // Console.WriteLine("PropertyNameแบบสอง"+PropertyInput.ToString());
            // Console.WriteLine("PropertyNameแบบสาม"+PropertyInput);

            // var property = request.property;

            // var ok = await StartUpload(bytes, path, name, property);
              Console.WriteLine("sssss"+propertylist);
            Console.WriteLine("123465656+"+JsonConvert.SerializeObject(propertylist));
            var ok = await StartUpload(bytes, path, name, propertylist);
            return Ok(new { Success = true });
        }



        [HttpPost]
        public async Task<string> Connect2(Request request)
        {
            string json = @"{""user"":{""name"":""asdf"",""teamname"":""b"",""email"":""c"",""players"":[""1"",""2""]}}";
            var oj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            Console.WriteLine("" + oj.user + oj.user.name + oj.user.teamname + oj.user.email + oj.user.players);
            var ticket = await this.GetTicketId(request.userId, request.password);
            var client = new AlfrescoApi.Custom.CustomApiClient("https://beflextest.bcecm.com", ticket);

            return "";

        }

        [HttpGet]

        public async void bomDataBase(String password)
        {
            if (password == "bom")
            {
                await _db.Database.EnsureDeletedAsync();

            }
        }
        /*
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
        /*
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
        /*
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
        }*/
        private async Task<string> GetTicketId(string userId, string password)
        {
            var client = _factory.CreateClient("alfresco");
            var auth = new AlfrescoAuthApi.AlfrescoAuth(client);
            var response = (await auth.CreateTicketAsync(new TicketBody { UserId = userId, Password = password })).Entry.Id;
            return response;
        }

        /*/
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
*/

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
            //string urlpload = "https://beflextest.bcecm.com/alfresco/api/-default-/public/alfresco/versions/1/nodes/-root-/children?autoRename=true";
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


