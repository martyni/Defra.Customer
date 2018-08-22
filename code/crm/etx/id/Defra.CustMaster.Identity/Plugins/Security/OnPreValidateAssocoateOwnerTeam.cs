using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.Crm.Sdk.Messages;

namespace Defra.CustMaster.Identity.Plugins.Security
{
    public class OnPreValidateAssocoateOwnerTeam : IPlugin 
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            tracingService.Trace("OnPreValidateAssocoateOwnerTeam before assingning teams.");

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "team")
                    return;

                EntityReferenceCollection RelatedRecords = (EntityReferenceCollection)context.InputParameters["RelatedEntitities"];
                OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(service);

                String MemberShipTeamName = (String)entity.Attributes["name"];
                String OwnerTeamName;
                Guid[] UserIds;
                int index = MemberShipTeamName.LastIndexOf("-");
                if (index > 0)
                {
                    OwnerTeamName = MemberShipTeamName.Substring(0, index) + "-OWNER";
                    if (OwnerTeamName != String.Empty)
                    {
                        var GetOwnerTeam = from c in orgSvcContext.CreateQuery("team")
                                           where (string)c["teamid"] == OwnerTeamName 
                                           && (int)c["statecode"] == 0
                                           select new
                                           {
                                               teamid = c.Id
                                           };

                        if(GetOwnerTeam.FirstOrDefault() != null)
                        {
                            tracingService.Trace("team found");

                            Guid TeamId = GetOwnerTeam.FirstOrDefault().teamid;
                            //get user 
                            AddMembersTeamRequest addRequest = new AddMembersTeamRequest();
                            addRequest.TeamId = TeamId;
                            UserIds = RelatedRecords.Select(x => x.Id).ToArray();
                            addRequest.MemberIds = UserIds;
                            tracingService.Trace("before updating ");

                            service.Execute(addRequest);
                            tracingService.Trace("after updating ");

                        }
                    }
                }
                tracingService.Trace("PreCreateAccountSetMultiSelectOptionSet after assinging values.");
            }
        }


    }
}
