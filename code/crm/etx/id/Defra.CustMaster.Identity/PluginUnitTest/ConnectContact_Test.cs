using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Defra.CustMaster.Identity.WfActivities.Connection;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk;

namespace PluginUnitTest
{
    [TestClass]
    public class ConnectContact_Test
    {
        [TestMethod]
        public void CoonectContactTestJason_Success()
        {
            /*
            Mock<IContactsDateOfBirthQuery> query = new Mock<IContactsDateOfBirthQuery>();

            query.Setup(q => q.GetContactsDateOfBirth(It.IsAny<Guid>()))
                .Returns((Guid id) => new Contact()
                {
                    sms_IsOver18 = false,
                    BirthDate = DateTime.Today.AddYears(-18).AddDays(-1),
                    ContactId = id
                });

            Mock<ISetContactOverEighteenCommand> command = new Mock<ISetContactOverEighteenCommand>();

            Mock<IAgeCalculator> calculator = new Mock<IAgeCalculator>();
            calculator.Setup(c => c.Is18OrOver(It.IsAny<DateTime>())).Returns(true);

            var service = new IsOverEighteenService(command.Object, query.Object, calculator.Object);

            service.ConfrimContactIsOverEighteen(new Guid());

            command.Verify(c => c.SetIsOverEgihteen(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Once);


            var classObj = new ConnectContact();

            WorkflowContext = MockRepository<IWorkflowContext>();

            WorkflowContext.Stub(x => x.Depth).Return(depth);

            // Workflow Invoker

            WorkFlowInvoker = new WorkflowInvoker(wf);

            WorkFlowInvoker.Extensions.Add<ITracingService>(() => TracingService);

            WorkFlowInvoker.Extensions.Add<IOrganizationServiceFactory>(() => Factory);

            WorkFlowInvoker.Extensions.Add<IWorkflowContext>(() => WorkflowContext);

            string json = @"{""deviceid"":""G7BF20DB2060"",""readingtype"":""Status"",""reading"":""Status"",""eventtoken"":null,""description"":""Engine speed"",""parameters"":{""VehicleName"":""Jeep Wrangler"",""VehicleSerialNumber"":""G7BF20DB2060"",""VIN"":""1J4FA69S74P704699"",""Date"":""10 / 2 / 2017 3:35:48 AM"",""DiagnosticName"":""Engine speed"",""DiagnosticCode"":""107"",""SourceName"":"" * *Go"",""Value"":""1363"",""Unit"":""Engine.UnitOfMeasureRevolutionsPerMinute""},""time"":""2017 - 10 - 02T03: 37:18.863Z""}";

            classObj.PayLoad  = json;
            classObj.ExecuteCRMWorkFlowActivity()
            */
        }
    }
}
