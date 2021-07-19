using System;
using System.IO;
using AlfrescoApi.Custom;
using Microsoft.AspNetCore.Http;

namespace ApiClient.Models
{
    public class Data
    {
        public string username {get ; set ;} 
        public string password {get ;set ;}
    }
    public class Request{
    public string userId {get ; set ;} 
    public string password {get ;set ;}
    }

    public class  FileRequest
    {
        public FileInfo info {get ; set ;} 
        public UploadProperty property {get ;set ;}
        public IFormFile file {get ;set ;}

       
        
    }
    public class UploadModel{

    public string Var1 { get; set; }

    public IFormFile File { get; set; }

}
public class UploadRequest {
        public string TargetName { set; get; }
        public string TargetPath { set; get; }
        public IFormFile File { set; get; }
    }
    
  
}
