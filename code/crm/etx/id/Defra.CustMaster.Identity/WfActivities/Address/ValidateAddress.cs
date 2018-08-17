namespace Defra.CustMaster.Identity.WfActivities
{
    using System;
    using System.Activities;
    using System.Linq;
    using D365.Common.Ints.Idm.resp;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Workflow;
    using SCII = D365.Common.Ints.Idm;
    using SCS = D365.Common.schema;

    public class ValidateAddress : WorkFlowActivityBase
    {
        [Input("UPRN")]
        public InArgument<string> UPRN { get; set; }

        [Input("Street")]
        public InArgument<string> Street { get; set; }

        [RequiredArgument]
        [Input("BuildingNumber")]
        public InArgument<string> BuildingNumber { get; set; }

        [RequiredArgument]
        [Input("BuildingName")]
        public InArgument<string> BuildingName { get; set; }

        [Input("PostCode")]
        public InArgument<string> PostCode { get; set; }

        [Input("Country")]
        public InArgument<string> Country { get; set; }

        [RequiredArgument]
        [Input("AddressTypeValue")]
        public InArgument<int> AddressType { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            string uprn;
            string buildingnumber;
            string buildingname;
            string street;
            string postcode;
            string country;
            int type;
            uprn = UPRN.Get(executionContext);
            buildingnumber = BuildingNumber.Get(executionContext);
            buildingname = BuildingName.Get(executionContext);
            street = Street.Get(executionContext);
            postcode = PostCode.Get(executionContext);
            country = Country.Get(executionContext);
            street = Street.Get(executionContext);
            SCII.Helper objCommon = new SCII.Helper(executionContext);
            type = AddressType.Get(executionContext);
            AddressData addressData = new AddressData();
            Guid addressId = Guid.Empty;
            Guid contactDetailId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);

            try
            {
                if (string.IsNullOrEmpty(buildingname))
                {
                    if (string.IsNullOrEmpty(buildingnumber))
                    {
                        throw new Exception("Provide either building name or building number, Building name is mandatory if the building number is empty;");
                    }
                }

                if (!Enum.IsDefined(typeof(SCII.AddressTypes), type))
                {
                    throw new Exception("Option set value for address of type not found;" + type);
                }

                if (string.IsNullOrEmpty(postcode))
                {
                    throw new Exception("Postcode is required");
                }
                else if (postcode.Length > 8)
                {
                    throw new Exception("postcode length can not be greater than 8 for UK countries;");
                }

                if (string.IsNullOrEmpty(country))
                {
                    throw new Exception("Country is required");
                }
                else if (country.Length > 3)
                {
                    throw new Exception("Country ISO ALPHA - 3 Code cannot be greater than 3;");
                }

                if (country.Trim().ToUpper() == "GBR")
                {
                    if (postcode.Length > 8)
                    {
                        throw new Exception("postcode length can not be greater than 8 for UK countries;");
                    }
                }
                else
                {
                    if (postcode.Length > 25)
                    {
                        throw new Exception("postcode length can not be greater than 25 for NON-UK countries;");
                    }
                }
            }
            catch (Exception ex)
            {
                crmWorkflowContext.Trace(ex.Message);
                throw ex;
            }
        }
    }
}
