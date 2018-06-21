using Defra.CustomerMaster.Identity.Api.Dynamics;
using Defra.CustomerMaster.Identity.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Defra.CustomerMaster.Identity.Api
{

    public class EnrolmentAPI : IEnrolmentAPI
    {
        /// <summary>
        /// Authz call 
        /// </summary>
        /// <param name="ServiceID"></param>
        /// <param name="UPN"></param>
        /// <returns> return new AuthzResponse { status = 200, version = "1.0.0.0",
        /// roles = new List<string>() { "ORG1GUID:Role1GUID", "ORG1GUID:Role2GUID", "ORG2GUID:Role21GUID" },
        ///       mappings = new List<string>() { "ORG1GUID:ORG1NAME", "ORG2GUID:ORG2NAME", "Role1GUID:Role1Name", "Role2GUID:Role2Name", "Role21GUID:Role21Name" } };</returns>

        //public List<ServiceUserLink> Authz(string ServiceID, string UPN)
        public AuthzResponse Authz(string ServiceID, string UPN)
        {
          
            List<string> rolesList = new List<string>();
            List<string> mappingsList = new List<string>();
            try
            {
                ServiceUserLinks serviceUserLinks = new CrmApiWrapper().Authz(ServiceID, UPN);
                //return serviceUserLinks.value;                
                foreach (ServiceUserLink serviceUserLink in serviceUserLinks.serviceUserLinks)
                {
                    string roleListItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.RoleId;
                    if (!rolesList.Contains(roleListItem))
                        rolesList.Add(roleListItem);
                    string mappingListOrgItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.OrganisationName;
                    if (!mappingsList.Contains(mappingListOrgItem))
                        mappingsList.Add(mappingListOrgItem);
                    string mappingListRoleItem = serviceUserLink.RoleId + ":" + serviceUserLink.RoleName;
                    if (!mappingsList.Contains(mappingListRoleItem))
                        mappingsList.Add(mappingListRoleItem);
                }
            }
            catch (WebFaultException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return new AuthzResponse
                {
                    status = 401,
                    version = "1.0.0.0",
                    roles = rolesList,
                    mappings = mappingsList
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return new AuthzResponse
                {
                    status = 401,
                    version = "1.0.0.0",
                    roles = rolesList,
                    mappings = mappingsList
                };

            }
            return new AuthzResponse
                {
                    status = 200,
                    version = "1.0.0.0",
                    roles = rolesList,
                    mappings = mappingsList
                };        
           

        }

        /// <summary>
        /// InitialMatch
        /// </summary>
        /// <param name="UPN"></param>
        /// <returns>{    "ErrorCode": 200,    "ErrorMsg": null,   
        /// "ServiceUserID": "ec11676a-d85a-e811-a832-000d3a27889d"}
        /// </returns>
        public string InitialMatch(string UPN)
        {
            try
            {
                System.Diagnostics.Trace.TraceError("IntialMatch call:" + UPN);
                //return string.Format("ServicieUserID is: {0}", new CrmApiWrapper().InitialMatch(UPN));
                if (string.IsNullOrEmpty(UPN) || string.IsNullOrWhiteSpace(UPN))
                    throw new WebFaultException("UPN can not be empty or null", 400);
                Contact crmContact = new CrmApiWrapper().InitialMatch(UPN);
                ServiceObject returnObj = new ServiceObject() { ServiceUserID = crmContact.contactid, ErrorCode = (int)crmContact.HttpStatusCode, ErrorMsg = crmContact.Message };

                return JsonConvert.SerializeObject(returnObj);
            }
            catch (WebFaultException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                //return (new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.ErrorMsg, ErrorCode = ex.HttpStatusCode });
                return JsonConvert.SerializeObject(new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.ErrorMsg, ErrorCode = ex.HttpStatusCode });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                // return (new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.Message });
                return JsonConvert.SerializeObject(new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.Message });
            }
        }

        /// <summary>
        /// UserInfo
        /// </summary>
        /// <param name="contact"></param>
        /// <returns>{    "ErrorCode": 200,    "ErrorMsg": "",
        /// "ServiceUserID": "b56748e5-dd5a-e811-a838-000d3a2b2b9f"
        /// }</returns>
        public string UserInfo(Contact contact)
        {
            try
            {
                System.Diagnostics.Trace.TraceError("UserInfo call params:{0},{1}", contact.UPN, contact.emailid);

                if ((contact == null) || string.IsNullOrEmpty(contact.UPN))
                {
                    throw new WebFaultException("UPN can not be empty or null", 401);
                }
                if ((contact == null) || string.IsNullOrEmpty(contact.lastname))
                {
                    throw new WebFaultException("lastname can not be empty or null", 401);
                }
                Contact crmContact = new CrmApiWrapper().UserInfo(contact);
                ServiceObject returnObj = new ServiceObject() { ServiceUserID = crmContact.contactid, ErrorCode = (int)crmContact.HttpStatusCode, ErrorMsg = crmContact.Message };

                return JsonConvert.SerializeObject(returnObj);

            }
            catch (WebFaultException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return JsonConvert.SerializeObject(new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.ErrorMsg, ErrorCode = ex.HttpStatusCode });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                // throw new WebFaultException(customError, HttpStatusCode.NotFound);
                return JsonConvert.SerializeObject(new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.Message });
                //return (new ServiceObject() { ServiceUserID = null, ErrorMsg = ex.Message });
            }
        }
    }
}
