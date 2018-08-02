namespace Defra.CustMaster.Identity.CoreApp.Controllers
{
    using System;
    using System.Collections.Generic; 
    using Defra.CustMaster.Identity.CoreApp.Dynamics;
    using Defra.CustMaster.Identity.CoreApp.Model;   
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Produces("application/json")]
    // [Route("api/[controller]")]
    [Route("api/")]
    public class ContactController : Controller
    {
        IConfiguration iconfig;
        IClientFactory iHttpClient;
        ITokenFactory iToken;
        ICrmApiWrapper _crmApiWrapper;

        public ContactController(IConfiguration iConfig, ICrmApiWrapper crmApiWrapper, IClientFactory iHttpClientFactory, ITokenFactory iTokenFactory)
        {
            iconfig = iConfig;
            iToken = iTokenFactory;
            iHttpClient = iHttpClientFactory;
            _crmApiWrapper = crmApiWrapper;
        }

        // GET api/values
        [Route("InitialMatch/{b2cobjectid}")]
        // http://defra-cm-dev-id-app.azurewebsites.net/api/InitialMatch/7b1ad2d0-7946-11e8-8d36-851e870eee8a
        // http://defra-cm-dev-id-app.azurewebsites.net/api/InitialMatch/14faa6c0-7bdd-11e8-bc93-67f83a37356f
        // [Route("InitialMatch")]
        [HttpGet]
       // public object InitialMatch([FromBody] InitialMatchRequest requestPayload)
        public object InitialMatch(string b2cobjectid)
        {
            try
            {
                if (!string.IsNullOrEmpty(b2cobjectid) && Guid.TryParse(b2cobjectid, out Guid result))
                    return Ok(_crmApiWrapper.InitialMatch(b2cobjectid));
                else
                {
                    return BadRequest(new ServiceObject { ErrorCode = 400, ErrorMsg = "B2CObjectid is invalid" });
                }
            }

            catch (Exception ex)
            {
                return BadRequest(new ServiceObject { ErrorCode = 500, ErrorMsg = ex.InnerException.Message });
            }
        }

        //[Route("Authz")]
        [HttpGet]
        // http://defra-cm-dev-id-app.azurewebsites.net/api/Authz/ServiceID=534BA555-037A-E811-A95B-000D3A2BC547&B2CObjectId=7b1ad2d0-7946-11e8-8d36-851e870eee8a
        // http://defra-cm-dev-id-app.azurewebsites.net/api/Authz/ServiceID=46048F69-037A-E811-A95B-000D3A2BC547&B2CObjectId=14faa6c0-7bdd-11e8-bc93-67f83a37356f
        // [Route("Authz/ServiceID={serviceid}/{b2cobjectid}")]
        [Route("Authz/ServiceID={serviceid}&B2CObjectId={b2cobjectid}")]
       // public AuthzResponse Authz([FromBody] AuthzRequest requestPayload)
        public AuthzResponse Authz(string serviceid, string b2cobjectid)
        {
            List<string> rolesList = new List<string>();
            List<string> mappingsList = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(b2cobjectid) && Guid.TryParse(b2cobjectid, out Guid resultB2Cobject))
                {
                    if (!string.IsNullOrEmpty(serviceid) && Guid.TryParse(serviceid, out Guid resultServiceId))
                    {
                        ServiceUserLinks serviceUserLinks = _crmApiWrapper.Authz(serviceid, b2cobjectid);
                        //return serviceUserLinks.value;                
                        foreach (ServiceUserLink serviceUserLink in serviceUserLinks.serviceUserLinks)
                        {
                            string roleListItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.RoleId+ ":" +serviceUserLink.EnrolmentStatus;
                            if (!rolesList.Contains(roleListItem))
                                rolesList.Add(roleListItem);
                            string mappingListOrgItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.OrganisationName;
                            if (!mappingsList.Contains(mappingListOrgItem))
                                mappingsList.Add(mappingListOrgItem);
                            string mappingListRoleItem = serviceUserLink.RoleId + ":" + serviceUserLink.RoleName;
                            if (!mappingsList.Contains(mappingListRoleItem))
                                mappingsList.Add(mappingListRoleItem);
                            string mappingListStatus = serviceUserLink.EnrolmentStatus + ":" + serviceUserLink.EnrolmentStatusText;
                            //if (!mappingsList.Contains(mappingListStatus))
                                mappingsList.Add(mappingListStatus);
                           
                        }

                        if (serviceUserLinks == null || serviceUserLinks.serviceUserLinks == null || serviceUserLinks.serviceUserLinks.Count == 0)
                        {
                            return new AuthzResponse
                            {
                                status = 204,
                                message = "No Content",
                                version = "1.0.0.0",
                                roles = rolesList,
                                mappings = mappingsList
                            };
                        }
                    }
                    else
                    {
                        return new AuthzResponse
                        {
                            status = 400,
                            message = "ServiceId is invalid",
                            version = "1.0.0.0",
                            roles = rolesList,
                            mappings = mappingsList
                        };
                    }
                }
                else
                {
                    return new AuthzResponse
                    {
                        status = 400,
                        message = "B2CObjectId is invalid",
                        version = "1.0.0.0",
                        roles = rolesList,
                        mappings = mappingsList
                    };
                }
            }
            catch (WebFaultException ex)
            {
                System.Diagnostics.Trace.TraceError(ex.Message);
                return new AuthzResponse
                {
                    status = 400,
                    message = ex.Message,
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
                    status = 500,
                    message = ex.Message,
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

        // POST api/values
       // [Route("CreateContact")]
       // [HttpPost]
        public Object CreateContact([FromBody]ContactModel contact)
        {
            try
            {
                ContactModel createContactResult = _crmApiWrapper.CreateContact(contact);
                //if(createContactResult.Code==200)
                return Ok(_crmApiWrapper.CreateContact(contact));
                //else if(createContactResult.Code==412)
                // return DuplicateWaitObjectException
                // else
                //  return in
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactModel { Code = 400, Message = ex.Message, MessageDetail = ex.InnerException.Message });
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}