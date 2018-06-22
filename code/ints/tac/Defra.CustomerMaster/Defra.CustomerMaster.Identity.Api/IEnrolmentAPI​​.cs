using Defra.CustomerMaster.Identity.Api.Model;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Defra.CustomerMaster.Identity.Api
{
    [ServiceContract]
    public interface IEnrolmentAPI
    {
        /// <summary>
        /// InitialMatch
        /// </summary>
        /// <param name="UPN"></param>
        /// <returns>{    "ErrorCode": 200,    "ErrorMsg": null,   
        /// "ServiceUserID": "ec11676a-d85a-e811-a832-000d3a27889d"}
        /// </returns>

        [OperationContract]
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/InitialMatch/{UPN}")]
        string InitialMatch(string UPN);


        /// <summary>
        /// UserInfo
        /// </summary>
        /// <param name="contact"></param>
        /// <returns>{    "ErrorCode": 200,    "ErrorMsg": "",
        /// "ServiceUserID": "b56748e5-dd5a-e811-a838-000d3a2b2b9f"
        /// }</returns>
        
        [OperationContract]
        [WebInvoke(Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/UserInfo/")]
        string UserInfo(Contact contact);


        /// <summary>
        /// Authz call 
        /// </summary>
        /// <param name="ServiceID"></param>
        /// <param name="UPN"></param>
        /// <returns> return new AuthzResponse { status = 200, version = "1.0.0.0",
        /// roles = new List<string>() { "ORG1GUID:Role1GUID", "ORG1GUID:Role2GUID", "ORG2GUID:Role21GUID" },
        ///       mappings = new List<string>() { "ORG1GUID:ORG1NAME", "ORG2GUID:ORG2NAME", "Role1GUID:Role1Name", "Role2GUID:Role2Name", "Role21GUID:Role21Name" } };</returns>
        
        [OperationContract]
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/Authz?ServiceID={ServiceID}&UPN={UPN}")]
        AuthzResponse Authz(string ServiceID, string UPN);
        //List<ServiceUserLink> Authz(string ServiceID, string UPN);
    }


    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
