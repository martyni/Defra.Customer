using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
namespace Defra.CustomerMaster.Identity.Api.Model
{
    [DataContract]
    public class ServiceUserLink
    {
        

        [DataMember]
        [JsonProperty("serviceLinkContact.fullname")]
        public string ContactName;

        [DataMember]
        [JsonProperty("serviceLinkRole.defra_lobserivceroleid")]
        public string RoleId;       

        [DataMember]
        [JsonProperty("serviceLinkRole.defra_name")]
        public string RoleName;

        [DataMember]
        [JsonProperty("serviceLinkOrganisation.accountid")]
        public string OrganisationId;

        [DataMember]
        [JsonProperty("serviceLinkOrganisation.name")]
        public string OrganisationName;

    }

    [DataContract]
    public class ServiceUserLinks
    {
        [DataMember]
        [JsonProperty("value")]
        public List<ServiceUserLink> serviceUserLinks;
    }
}