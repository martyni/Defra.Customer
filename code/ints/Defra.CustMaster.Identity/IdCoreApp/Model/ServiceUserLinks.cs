namespace Defra.CustMaster.Identity.CoreApp.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    
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


        [DataMember]
        [JsonProperty("defra_enrolmentstatus")]
        public string EnrolmentStatus;

        [DataMember]
        [JsonProperty("defra_enrolmentstatus@OData.Community.Display.V1.FormattedValue")]
        public string EnrolmentStatusText;
        

    }

    [DataContract]
    public class ServiceUserLinks
    {
        [DataMember]
        [JsonProperty("value")]
        public List<ServiceUserLink> serviceUserLinks;
    }
}