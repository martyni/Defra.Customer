using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SCS = Defra.CustMaster.D365.Common.schema;
using SCSE = Defra.CustMaster.D365.Common.Schema.ExtEnums;
using SCII = Defra.CustMaster.D365.Common.Ints.Idm;
using SCIIR = Defra.CustMaster.D365.Common.Ints.Idm.Resp;

namespace Defra.CustMaster.Identity.WfActivities.Address
{
    public class AddAddress : WorkFlowActivityBase
    {
        #region Local Properties
        SCII.Helper objCommon;
        //EntityReference _Contact;
        int _errorCode = 400; //Bad Request
        string _errorMessage = string.Empty;
        string _errorMessageDetail = string.Empty;
        Guid _contactId = Guid.Empty;
        string _uniqueReference = string.Empty;

        #endregion
        protected override void Execute(CodeActivityContext context)
        {
            // Construct the Local plug-in context.
            LocalWorkflowContext localcontext = new LocalWorkflowContext(context);
            localcontext.Trace("started execution");

            #region "Load CRM Service from context"
            objCommon = new SCII.Helper(context);

            localcontext.Trace("CreateContact activity:Load CRM Service from context --- OK");
        }

    }
}
    