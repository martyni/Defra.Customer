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

    public class SearchCustomerAddress : WorkFlowActivityBase
    {
        [Input("UPRN")]
        public InArgument<string> UPRN { get; set; }

        [Input("Street")]
        public InArgument<string> Street { get; set; }

        [RequiredArgument]
        [Input("BuildingNumber")]
        public InArgument<string> BuildingNumber { get; set; }

        [Input("PostCode")]
        public InArgument<string> PostCode { get; set; }

        [Input("Country")]
        public InArgument<string> Country { get; set; }

        [Output("ReturnAddressRef")]
        [ReferenceTarget("defra_address")]
        public OutArgument<EntityReference> OutAddressEntityRef { get; set; }

        [Output("ReturnCountryRef")]
        [ReferenceTarget("defra_country")]
        public OutArgument<EntityReference> OutCountryEntityRef { get; set; }

        public override void ExecuteCRMWorkFlowActivity(CodeActivityContext executionContext, LocalWorkflowContext crmWorkflowContext)
        {
            string uprn;
            string buildingnumber;
            string street;
            string postcode;
            string country;
            uprn = UPRN.Get(executionContext);
            buildingnumber = BuildingNumber.Get(executionContext);
            street = Street.Get(executionContext);
            postcode = PostCode.Get(executionContext);
            country = Country.Get(executionContext);
            street = Street.Get(executionContext);
            SCII.Helper objCommon = new SCII.Helper(executionContext);

            AddressData addressData = new AddressData();
            Guid addressId = Guid.Empty;
            Guid contactDetailId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(objCommon.service);
            if (!string.IsNullOrEmpty(country))
            {
                string countryValue = country.ToUpper();
                crmWorkflowContext.Trace("Country search started" + country);

                var countryRecord = from c in orgSvcContext.CreateQuery(SCS.Address.COUNTRY)
                                    where (((string)c["defra_isocodealpha3"]) == countryValue) && (int)c[SCS.ContactDetails.STATECODE] == 0
                                    select new { CountryId = c.Id };
                Guid countryGuid = countryRecord != null && countryRecord.FirstOrDefault() != null ? countryRecord.FirstOrDefault().CountryId : Guid.Empty;
                crmWorkflowContext.Trace("country found" + countryGuid);
                if (countryGuid != Guid.Empty)
                {
                    EntityReference entityRef = new EntityReference(SCS.Address.COUNTRY, countryGuid);
                    OutCountryEntityRef.Set(executionContext, entityRef);
                }

                crmWorkflowContext.Trace("Finished: Defra.CustMaster.Identity.WfActivities.ExecuteCRMWorkFlowActivity.CustomerAddress");
            }

            if (country.Trim().ToUpper() == "GBR")
            {
                if (uprn != null)
                {
                    crmWorkflowContext.Trace("UPRN search:started..");
                    var propertyWithUPRN = from c in orgSvcContext.CreateQuery(SCS.Address.ENTITY)
                                           where ((string)c[SCS.Address.UPRN]).Equals(uprn.Trim()) && (int)c[SCS.ContactDetails.STATECODE] == 0
                                           select new { AddressId = c.Id };
                    addressId = propertyWithUPRN != null && propertyWithUPRN.FirstOrDefault() != null ? propertyWithUPRN.FirstOrDefault().AddressId : Guid.Empty;
                    crmWorkflowContext.Trace("UK UPRN Address:" + addressId);
                }

                if (addressId == Guid.Empty && street != null && postcode != null && buildingnumber != null)
                {
                    crmWorkflowContext.Trace("postcode and street search:started");
                    var propertyWithDuplicate = from c in orgSvcContext.CreateQuery(SCS.Address.ENTITY)
                                                where ((string)c[SCS.Address.STREET]).Equals(street.Trim()) && ((string)c[SCS.Address.POSTCODE]).Equals(postcode.Trim()) && ((string)c[SCS.Address.PREMISES]).Equals(buildingnumber.Trim()) && (int)c[SCS.ContactDetails.STATECODE] == 0
                                                select new { AddressId = c.Id };
                    addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
                    crmWorkflowContext.Trace("UK PostCode address:" + addressId);
                }
            }
            else
            {
                if (addressId == Guid.Empty && street != null && postcode != null && buildingnumber != null)
                {
                    crmWorkflowContext.Trace("postcode and street search:started");
                    var propertyWithDuplicate = from c in orgSvcContext.CreateQuery(SCS.Address.ENTITY)
                                                where ((string)c[SCS.Address.STREET]).Equals(street.Trim()) && ((string)c[SCS.Address.INTERNATIONALPOSTCODE]).Equals(postcode.Trim()) && ((string)c[SCS.Address.PREMISES]).Equals(buildingnumber.Trim()) && (int)c[SCS.ContactDetails.STATECODE] == 0
                                                select new { AddressId = c.Id };
                    addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
                    crmWorkflowContext.Trace("Non UK Internaltional PostCode address:" + addressId);
                }
            }

            if (addressId != Guid.Empty)
            {
                EntityReference entityRef = new EntityReference("defra_address", addressId);
                OutAddressEntityRef.Set(executionContext, entityRef);
            }
        }
    }
}
