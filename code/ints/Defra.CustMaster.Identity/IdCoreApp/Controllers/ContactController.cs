﻿using System;
using System.Collections.Generic;
using Defra.CustomerMaster.Identity.Api.Dynamics;
using Defra.CustomerMaster.Identity.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Defra.CustomerMaster.Identity.Api.Controllers
{


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
        //[Route("InitialMatch")]
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
        //[Route("Authz/ServiceID={serviceid}/{b2cobjectid}")]
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
                        if(serviceUserLinks==null|| serviceUserLinks.serviceUserLinks==null|| serviceUserLinks.serviceUserLinks.Count==0)
                        {
                            return new AuthzResponse
                            {
                                status = 204,
                                message="No Content",
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
        [Route("CreateContact")]
        [HttpPost]
        public Object CreateContact([FromBody]Defra.CustomerMaster.Identity.Api.Model.ContactModel contact)
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