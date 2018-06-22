using System.Runtime.Serialization;

namespace Defra.CustomerMaster.Identity.Api.Model
{
    [DataContract]
    public class AuthzParam
    {
        [DataMember]
        public string ServiceID;

        [DataMember]
        public string UPN;
    }
}