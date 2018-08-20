using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
namespace Defra.CustMaster.Identity.Plugins
{
    public class PreCreateAccountSetMultiSelectOptionSet : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            tracingService.Trace("PreCreateAccountSetMultiSelectOptionSet before assinging values.");


            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "account")
                    return;

                OptionSetValueCollection OrganisationTypeValue;
                OptionSetValueCollection Role;

                if (entity["defra_cminterimorganisationtypevalue"] != null)
                {
                    //set organistaion type
                    OrganisationTypeValue = new OptionSetValueCollection();
                    OrganisationTypeValue.Add(new OptionSetValue((int)entity["defra_cminterimorganisationtypevalue"]));
                    entity["defra_type"] = OrganisationTypeValue;

                }

                if(entity["defra_cminterimrole"] != null)
                {
                    Role = new OptionSetValueCollection();
                    Role.Add(new OptionSetValue((int)entity["defra_cminterimrole"]));
                    entity["defra_role"] = Role;

                }

              
                tracingService.Trace("PreCreateAccountSetMultiSelectOptionSet after assinging values.");
            }
        }

    }
}
