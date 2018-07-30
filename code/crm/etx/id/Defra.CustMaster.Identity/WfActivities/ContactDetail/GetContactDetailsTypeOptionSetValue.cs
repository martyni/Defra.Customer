using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Defra.CustMaster.Identity.WfActivities
{
   public class GetContactDetailsTypeOptionSetValue : WorkFlowActivityBase
    {


        [RequiredArgument]
        [Input("Type")]
        [AttributeTarget("defra_addressdetails", "defra_addresstype")]
        public InArgument<OptionSetValue> TypeValue { get; set; }

      
        [Output("RequestTypeValue")]
        public OutArgument<int> RequestTypeValue { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.ContactDetailType");

            RequestTypeValue.Set(executionContext, TypeValue.Get(executionContext).Value);

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ContactDetailType");
        }
    }
}
