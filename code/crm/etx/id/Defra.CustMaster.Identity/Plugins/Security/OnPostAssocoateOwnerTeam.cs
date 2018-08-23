using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Defra.CustMaster.Identity.Plugins.Security
{
    public class OnPostAssocoateOwnerTeam : IPlugin
    {
        ITracingService tracingService;
        public void Execute(IServiceProvider serviceProvider)
        {
            tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            tracingService.Trace("OnPreValidateAssocoateOwnerTeam before assingning teams.");

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (context.MessageName == "Associate" || context.MessageName == "Disassociate")
            {

                if (context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is EntityReference)
                {
                    EntityReference entityReference = (EntityReference)context.InputParameters["Target"];

                    tracingService.Trace("contains target entity");
                    tracingService.Trace("logical name:" + entityReference.LogicalName);

                    if (entityReference.LogicalName != "team")
                        return;

                    EntityReferenceCollection RelatedRecords = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];
                    OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(service);
                    //get team name
                    Entity TeamDetails = service.Retrieve("team", entityReference.Id, new ColumnSet("name", "isdefault", "businessunitid"));
                    String MemberShipTeamName = (String)TeamDetails["name"];
                    Boolean IsDefaultTeam = (Boolean)TeamDetails["isdefault"];
                    String OwnerTeamName;
                    Boolean IsUserPartOfAnyOtherTeam = false;
                    int index;
                    String BusinessUnitName;
                    //process records only if its not a default team
                    if (!IsDefaultTeam)
                    {
                        if (context.MessageName == "Associate")
                        {
                            if (MemberShipTeamName.EndsWith("CM-MEMBER"))
                            {
                                index = MemberShipTeamName.LastIndexOf("-");
                                if (index > 0)
                                {
                                    OwnerTeamName = MemberShipTeamName.Substring(0, index) + "-OWNER";
                                    tracingService.Trace("team found");
                                    Guid? TeamId = GetTeamId(OwnerTeamName, orgSvcContext);
                                    //Associate Team with User
                                    tracingService.Trace("before handling membership team");
                                    AddMembersToTeam(TeamId, RelatedRecords, service);
                                    tracingService.Trace("after handling membership team");

                                }
                            }
                            {
                                tracingService.Trace("not a member team");
                            }
                        }
                        else if(context.MessageName == "Disassociate")
                        {
                            if (MemberShipTeamName.EndsWith("CM-MEMBER"))
                            {
                                //check if user is associated with any other team other than owner team
                                Guid BusinessUnitId = ((EntityReference)TeamDetails["businessunitid"]).Id;
                                String TeamBusinessUnitName = (String)service.Retrieve("businessunit", BusinessUnitId, new ColumnSet("name")).Attributes["name"];
                                Guid? OwnerTeam = GetTeamId(TeamBusinessUnitName + "-CM-OWNER", orgSvcContext);
                                IsUserPartOfAnyOtherTeam =
                                    CheckIfUserIsMemberofAnyOtherTeam((Guid)TeamDetails["teamid"], ((EntityReference)TeamDetails["businessunitid"]).Id,
                                    orgSvcContext);
                                if (!IsUserPartOfAnyOtherTeam)
                                {
                                    tracingService.Trace("before handling remove team member");
                                    //if user is not part of any other team then remove that user
                                    RemoveMembersFromTeam(OwnerTeam, RelatedRecords, service);
                                    tracingService.Trace("after handling remove team member");
                                }
                            }

                            else
                            {
                                tracingService.Trace("not a meber team");
                            }
                        }
                    }
                    //}
                    //else //handle default team
                    //{
                    //    //CHEMS - UK - REACH - IT
                    //    MemberShipTeamName = (string)TeamDetails["name"] + "-CM-MEMBER";
                    //    tracingService.Trace("team found");
                    //    Guid? TeamId = GetTeamId(MemberShipTeamName, orgSvcContext);//GetMemberTeam.FirstOrDefault().teamid;
                    //                                                                //Associate Team with User
                    //    tracingService.Trace("before handling default team");
                    //    AddMembersToTeam(TeamId, RelatedRecords, service);
                    //    tracingService.Trace("after handling default team");

                    //}
                    tracingService.Trace("PreCreateAccountSetMultiSelectOptionSet after assinging values.");
                }
            }
        }
        private void AddMembersToTeam(Guid? TeamId, EntityReferenceCollection Users, IOrganizationService service)
        {

            Guid[] UserIds;
            //get user 
            AddMembersTeamRequest addRequest = new AddMembersTeamRequest();
            if (TeamId.HasValue)
            {
                addRequest.TeamId = TeamId.Value;
                UserIds = Users.Select(x => x.Id).ToArray();
                addRequest.MemberIds = UserIds;
                tracingService.Trace("before updating ");
                service.Execute(addRequest);
                tracingService.Trace("after updating ");
            }

        }

        private Guid? GetTeamId(String TeamName, OrganizationServiceContext orgSvcContext)
        {
            Guid? ReturnVal = null;
            tracingService.Trace("Searching for team" + TeamName);
            var GetMemberTeam = from c in orgSvcContext.CreateQuery("team")
                                where (string)c["name"] == TeamName
                                select new
                                {
                                    teamid = c.Id
                                };

            if (GetMemberTeam.FirstOrDefault() != null)
            {
                ReturnVal = GetMemberTeam.FirstOrDefault().teamid;
            }
            else
            {
                tracingService.Trace("Team " + TeamName + " not found.");
            }
            return ReturnVal;
        }


        private Boolean CheckIfUserIsMemberofAnyOtherTeam(Guid CurrentTeamId,Guid BusinessUnitID, OrganizationServiceContext orgSvcContext)
        {
            tracingService.Trace("Searching if user is member of any other team current team id:" + CurrentTeamId);
            var GetMemberTeam = from c in orgSvcContext.CreateQuery("team")
                                where  (Guid)c["businessunitid"] == BusinessUnitID
                                && (Boolean)c["isdefault"] == false
                                && (Guid)c["teamid"] != CurrentTeamId
                                select new
                                {
                                    teamid = c.Id
                                };


            tracingService.Trace("Count of existing teams:" + GetMemberTeam.ToList().Count);
            

            return GetMemberTeam.ToList().Count > 0 ? true : false; 
        }

        public static void RemoveMembersFromTeam(Guid? TeamId, EntityReferenceCollection Users, IOrganizationService service)
        {
            Guid[] UserIds;

            if (TeamId.HasValue)
            {
                RemoveMembersTeamRequest addRequest = new RemoveMembersTeamRequest();
                addRequest.TeamId = TeamId.Value;
                UserIds = Users.Select(x => x.Id).ToArray();
                addRequest.MemberIds = UserIds;
                service.Execute(addRequest);
            }
        }

    }


}
