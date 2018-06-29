using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization;

namespace Defra.CustomerMaster.Identity.Api.UnitTests
{

    [DataContract]
    public class InitialMatchResponse
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
    public class AuthzResponse
    {
        [DataMember]
        [DefaultValue("200")]
        [JsonProperty("status")]
        public int ErrorCode;

        [DataMember]
        [JsonProperty("message")]
        public string ErrorMsg;

        [DataMember]
        [JsonProperty("roles")]
        public List<string> Roles;


        [DataMember]
        [JsonProperty("mappings")]
        public List<string> Mappings;

       
    }
}

