using System;
using System.Net;
using System.Runtime.Serialization;


namespace Defra.CustomerMaster.Identity.Api
{

    [DataContract]
    public class ServiceObject
    {
        [DataMember]
        public string ServiceUserID;

        [DataMember]
        public int ErrorCode;

        [DataMember]
        public string ErrorMsg;
    }
}