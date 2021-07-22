
using System;
using System.Collections.Generic;

namespace Getdatajson.Models{

    
         class User {
        public string Id { set; get; }
        public string DisplayName { set; get; }
    }

    class Content {
        public string MimeType { set; get; }
        public string MimeTypeName { set; get; }
        public int SizeInBytes { set; get; }
        public string Encoding { set; get; }
    }

    class Entry {
        public bool IsFile { set; get; }
        public User CreatedByUser { set; get; }
        public DateTime ModifiedAt { set; get; }
        public string NodeType { set; get; }
        public Content content { set; get; }
        public Dictionary<string, string> Contents { set; get; }
        public string ParentId { set; get; }
        public List<string> AspectNames { set; get; }
        public DateTime CreatedAt { set; get; }
        public bool IsFolder { set; get; }
        public User ModifedByUser { set; get; }
        public string Name { set; get; }
        public string Id { set; get; }
        public Dictionary<string, string> Properties { set; get; }
    }
        class Info {
        public Entry Entry { set; get; }

  
    }
}