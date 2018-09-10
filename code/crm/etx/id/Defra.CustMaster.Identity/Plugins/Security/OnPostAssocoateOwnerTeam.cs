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
            
            String RelatedRecordLogicalName;
            tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            tracingService.Trace("OnPreValidateAssocoateOwnerTeam before assingning teams.");
            EntityReferenceCollection RelatedRecords = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];

            Relationship relationship = (Relationship)context.InputParameters["Relationship"];

            if (relationship.SchemaName != "teammembership_association")
                return;
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

                    if (entityReference.LogicalName != "team" && entityReference.LogicalName != "systemuser")
                        return;

                    OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(service);
                    //get team name
                    tracingService.Trace("after getting related records");

                    String OwnerTeamName;
                    Boolean IsUserPartOfAnyOtherTeam = false;
                    int index;
                    String BusinessUnitName;
                    //process records only if its not a default team
                   
                        if (context.MessageName == "Associate" && entityReference.LogicalName == "team")
                        {

                        Entity TeamDetails = service.Retrieve("team", entityReference.Id, new ColumnSet("name", "isdefault", "businessunitid"));
                        String MemberShipTeamName = (String)TeamDetails["name"];
                        Boolean IsDefaultTeam = (Boolean)TeamDetails["isdefault"];
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
                        else if(context.MessageName == "Disassociate" && entityReference.LogicalName == "systemuser" )
                        {
                        Guid UserId = entityReference.Id;

                        tracingService.Trace("getting system user details");
                        Entity UserDetails = service.Retrieve("systemuser", entityReference.Id, new ColumnSet( "businessunitid"));
                        tracingService.Trace("related record type" + RelatedRecords.FirstOrDefault().LogicalName);
                        tracingService.Trace("getting team details");

                        Entity TeamDetails = service.Retrieve("team", RelatedRecords.FirstOrDefault().Id, new ColumnSet("name", "isdefault", "businessunitid"));
                        tracingService.Trace("after team details");

                        String MemberShipTeamName = (String)TeamDetails["name"];
                        if (MemberShipTeamName.EndsWith("CM-MEMBER"))
                            {
                            tracingService.Trace("its a member team");

                            //check if user is associated with any other team other than owner team
                            Guid BusinessUnitId = ((EntityReference)TeamDetails["businessunitid"]).Id;
                                String TeamBusinessUnitName = (String)service.Retrieve("businessunit", BusinessUnitId, new ColumnSet("name")).Attributes["name"];
                                Guid? OwnerTeam = GetTeamId(TeamBusinessUnitName + "-CM-OWNER", orgSvcContext);

                            //IsUserPartOfAnyOtherTeam = false;
                            IsUserPartOfAnyOtherTeam =
                                CheckIfUserIsMemberofAnyOtherTeam(TeamDetails.Id, OwnerTeam,UserId,
                                ((EntityReference)TeamDetails["businessunitid"]).Id,
                                orgSvcContext);
                            if (!IsUserPartOfAnyOtherTeam)
                                {
                                    tracingService.Trace("before handling remove team member");
                                //if user is not part of any other team then remove that user

                                Guid[] Users = new Guid[1];
                                Users[0] = UserId;
                                    RemoveMembersFromTeam(OwnerTeam, Users, service);
                                    tracingService.Trace("after handling remove team member");
                                }
                            }

                            else
                            {
                                tracingService.Trace("not a meber team");
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


        private Boolean CheckIfUserIsMemberofAnyOtherTeam(Guid CurrentTeamId, Guid? OwnerTeam, Guid UserID, Guid BusinessUnitID, OrganizationServiceContext orgSvcContext)
        {
            tracingService.Trace("Searching if user is member of any other team current team id:" + CurrentTeamId);
            var GetMemberTeam = from c in orgSvcContext.CreateQuery("team")
                                join t in orgSvcContext.CreateQuery("teammembership")
                                on c["teamid"] equals t["teamid"]
                                where  (Guid)c["businessunitid"] == BusinessUnitID
                                && (Boolean)c["isdefault"] == false
                                && (Guid)c["teamid"] != CurrentTeamId
                                && (Guid)c["teamid"] != OwnerTeam.Value
                                && (Guid) t["systemuserid"] == UserID
                                select new
                                {
                                    teamid = c.Id
                                };


            tracingService.Trace("Count of existing teams:" + GetMemberTeam.ToList().Count);
            

            return GetMemberTeam.ToList().Count > 0 ? true : false; 
        }

        public  void RemoveMembersFromTeam(Guid? TeamId, Guid[] Users, IOrganizationService service)
        {
            Guid[] UserIds;

            if (TeamId.HasValue)
            {
                tracingService.Trace("before removal");

                RemoveMembersTeamRequest addRequest = new RemoveMembersTeamRequest();
                addRequest.TeamId = TeamId.Value;
                addRequest.MemberIds = Users;
                service.Execute(addRequest);
            }
        }

    }


}
