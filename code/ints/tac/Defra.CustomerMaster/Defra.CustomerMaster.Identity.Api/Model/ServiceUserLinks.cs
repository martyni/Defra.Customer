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
        //[DataMember]
        //public string createdon;

        //[DataMember]
        //[JsonProperty("defra_lobserviceuserlinkid")]
        //public string ServiceUserLinkId;

        //[DataMember]
        //[JsonProperty("defra_name")]
        //public string defra_name;

        //[DataMember]
        //[JsonProperty("_defra_serviceuser_value")]
        //public string _defra_serviceuser_value;

        //[DataMember]
        //[JsonProperty("defra_ServiceUser")]
        //public string defra_ServiceUser;

        //[DataMember]
        //[JsonProperty("_defra_servicerole_value")]
        //public string _defra_servicerole_value;

        //[DataMember]
        //[JsonProperty("defra_ServiceRole")]
        //public string defra_ServiceRole;

        [DataMember]
        [JsonProperty("serviceLinkContact_x002e_fullname")]
        public string ContactName;

        [DataMember]
        [JsonProperty("serviceLinkRole_x002e_defra_lobserivceroleid")]
        public string RoleId;       

        [DataMember]
        [JsonProperty("serviceLinkRole_x002e_defra_name")]
        public string RoleName;

        [DataMember]
        [JsonProperty("serviceLinkOrganisation_x002e_accountid")]
        public string OrganisationId;

        [DataMember]
        [JsonProperty("serviceLinkOrganisation_x002e_name")]
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