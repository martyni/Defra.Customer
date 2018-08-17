using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Defra.CustMaster.Identity.WfActivities
{
    public class GetHirarchyLevelTypeOptionSetValue : WorkFlowActivityBase
    {


        [RequiredArgument]
        [Input("Type")]
        [AttributeTarget("account", "defra_hierarchylevel")]
        public InArgument<OptionSetValue> HyrarchyLevelValue { get; set; }


        [Output("RequestTypeValue")]
        public OutArgument<int> RequestTypeValue { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            crmWorkflowContext.Trace("Started: Defra.CustMaster.Identity.WfActivities.HirarchyLevel");

            RequestTypeValue.Set(executionContext, HyrarchyLevelValue.Get(executionContext).Value);

            crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.HirarchyLevel");
        }
    }
}
