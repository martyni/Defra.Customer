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
                Entity TeamDetails = service.Retrieve("team", entityReference.Id, new ColumnSet("name", "isdefault"));
                String MemberShipTeamName = (String)TeamDetails["name"];
                Boolean IsDefaultTeam = (Boolean)TeamDetails["isdefault"];
                String OwnerTeamName;

                Guid[] UserIds;
                int index;
                if (!IsDefaultTeam)
                {
                    index = MemberShipTeamName.LastIndexOf("-");
                    if (index > 0)
                    {
                        OwnerTeamName = MemberShipTeamName.Substring(0, index) + "-OWNER";
                        tracingService.Trace("team found");
                        Guid? TeamId = GetTeamId(OwnerTeamName, orgSvcContext);
                        //Associate Team with User
                        tracingService.Trace("before handling membership team");
                        AssociateTeamWithUsers(TeamId, RelatedRecords, service);
                        tracingService.Trace("after handling membership team");

                    }
                }
                else //handle default team
                {
                    //CHEMS - UK - REACH - IT
                    MemberShipTeamName = (string)TeamDetails["name"] + "-CM-MEMBER";
                    tracingService.Trace("team found");
                    Guid? TeamId = GetTeamId(MemberShipTeamName, orgSvcContext);//GetMemberTeam.FirstOrDefault().teamid;
                    //Associate Team with User
                    tracingService.Trace("before handling default team");
                    AssociateTeamWithUsers(TeamId, RelatedRecords, service);
                    tracingService.Trace("after handling default team");

                }
                tracingService.Trace("PreCreateAccountSetMultiSelectOptionSet after assinging values.");
            }

        }
        private void AssociateTeamWithUsers(Guid? TeamId, EntityReferenceCollection Users, IOrganizationService service)
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

            if(GetMemberTeam.FirstOrDefault() != null)
            {
                ReturnVal = GetMemberTeam.FirstOrDefault().teamid;
            }
            else
            {
                tracingService.Trace("Team " + TeamName + " not found.");
            }
            return ReturnVal;
        }

    }


}
