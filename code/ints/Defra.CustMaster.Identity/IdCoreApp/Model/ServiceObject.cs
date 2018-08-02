namespace Defra.CustMaster.Identity.CoreApp.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
  

    [DataContract]
    public class InitialMatchResponse
    {    
        [DataMember]
        [JsonProperty("value")]
        public List<ServiceObject> value;
    }

    [DataContract]
    public class ServiceObject
    {
        [DataMember]
        [JsonProperty("contactid")]
        public string ServiceUserID;


        [DataMember]
        [JsonProperty("defra_uniquereference")]
        public string UniqueReferenceId;

        [DataMember]
        [DefaultValue("200")]
        public int ErrorCode;

        [DataMember]
        public string ErrorMsg;
    }
}