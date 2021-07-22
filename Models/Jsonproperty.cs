using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonProperty
{
    


class RootObject
	{
		[JsonProperty("entry")]
		public Results Results { get; set; }

    }
	
	class Results
	{
		
        [JsonProperty("createdByUser")]
		public Dictionary<string, JobCode> JobCodes { get; set; }
        //[JsonProperty("jobcodes")]
		//public Dictionary<string, JobCode> JobCodes { get; set; }
	}
	
	class JobCode
	{
		[JsonProperty("id")]
		public string id { get; set; }
        [JsonProperty("displayName")]
        public string displayName { get; set; }
        
		
		
	}

}