// <copyright file="ContactController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Defra.CustMaster.Identity.CoreApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using Defra.CustMaster.Identity.CoreApp.Dynamics;
    using Defra.CustMaster.Identity.CoreApp.Model;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    [Produces("application/json")]
    [Route("api/")]
    public class ContactController : Controller
    {
        private IConfiguration iconfig;
        private IClientFactory iHttpClient;
        private ITokenFactory iToken;
        private ICrmApiWrapper _crmApiWrapper;

        public ContactController(IConfiguration iConfig, ICrmApiWrapper crmApiWrapper, IClientFactory iHttpClientFactory, ITokenFactory iTokenFactory)
        {
            iconfig = iConfig;
            iToken = iTokenFactory;
            iHttpClient = iHttpClientFactory;
            _crmApiWrapper = crmApiWrapper;
        }

        [HttpGet]
        [Route("InitialMatch")]
        public object InitialMatch([FromQuery] string b2cobjectid)
        {
            try
            {
                if (!string.IsNullOrEmpty(b2cobjectid))
                {
                    return Ok(_crmApiWrapper.InitialMatch(b2cobjectid));
                }
                else
                {
                    return BadRequest(new ServiceObject { ErrorCode = 400, ErrorMsg = "B2CObjectid is invalid" });
                }
            }
            catch (WebFaultException ex)
            {
                return BadRequest(new ServiceObject { ErrorCode = ex.HttpStatusCode, ErrorMsg = ex.ErrorMsg });
            }

            catch (Exception ex)
            {
                return BadRequest(new ServiceObject { ErrorCode = 500, ErrorMsg = ex.Message });
            }
        }

        [HttpGet]
        [Route("Authz")]
        public AuthzResponse Authz([FromQuery] string serviceid, [FromQuery]string b2cobjectid)
        {
            List<string> rolesList = new List<string>();
            List<string> mappingsList = new List<string>();
            try
            {
                if (!string.IsNullOrEmpty(b2cobjectid))
                {
                    if (!string.IsNullOrEmpty(serviceid))
                    {
                        ServiceUserLinks serviceUserLinks = _crmApiWrapper.Authz(serviceid, b2cobjectid);

                        foreach (ServiceUserLink serviceUserLink in serviceUserLinks.serviceUserLinks)
                        {
                            string roleListItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.RoleId + ":" + serviceUserLink.EnrolmentStatus;
                            if (!rolesList.Contains(roleListItem))
                            {
                                rolesList.Add(roleListItem);
                            }

                            string mappingListOrgItem = serviceUserLink.OrganisationId + ":" + serviceUserLink.OrganisationName;
                            if (!mappingsList.Contains(mappingListOrgItem))
                            {
                                mappingsList.Add(mappingListOrgItem);
                            }

                            string mappingListRoleItem = serviceUserLink.RoleId + ":" + serviceUserLink.RoleName;
                            if (!mappingsList.Contains(mappingListRoleItem))
                            {
                                mappingsList.Add(mappingListRoleItem);
                            }

                            string mappingListStatus = serviceUserLink.EnrolmentStatus + ":" + serviceUserLink.EnrolmentStatusText;

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
                    message = ex.ErrorMsg,
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
                message = "Success!",
                version = "1.0.0.0",
                roles = rolesList,
                mappings = mappingsList
            };
        }
    }
}