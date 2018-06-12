using Defra.CustMaster.D365.Common.schema;
using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Defra.CustMaster.D365.Common.Ints.Idm
{
    public class Helper
    {
        public ITracingService tracingService;
        public IWorkflowContext context;
        public IOrganizationServiceFactory serviceFactory;
        public IOrganizationService service;

        public Helper(CodeActivityContext executionContext)
        {
            tracingService = executionContext.GetExtension<ITracingService>();
            context = executionContext.GetExtension<IWorkflowContext>();
            serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            service = serviceFactory.CreateOrganizationService(context.UserId);
        }



        public void CreateAddress(Address addressDetails, EntityReference Customer)
        {
            Guid addressId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(this.service);
            if (addressDetails.uprn != null)
            {
                var propertyWithUPRN = from c in orgSvcContext.CreateQuery(AddressSchema.ENTITY)
                                       where ((string)c[AddressSchema.UPRN]).Equals((addressDetails.uprn.Trim()))
                                       select new { AddressId = c.Id };
                addressId = propertyWithUPRN != null && propertyWithUPRN.FirstOrDefault() != null ? propertyWithUPRN.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty && addressDetails.street != null && addressDetails.postcode != null && addressDetails.buildingnumber != null)
            {
                var propertyWithDuplicate = from c in orgSvcContext.CreateQuery(AddressSchema.ENTITY)
                                            where ((string)c[AddressSchema.STREET]).Equals((addressDetails.street.Trim())) && ((string)c[AddressSchema.POSTCODE]).Equals((addressDetails.postcode.Trim())) && ((string)c[AddressSchema.PREMISES]).Equals((addressDetails.buildingnumber.Trim()))
                                            select new { AddressId = c.Id };
                addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty)
            {
                Entity address = new Entity(AddressSchema.ENTITY);
                if (addressDetails.uprn != null)
                    address[AddressSchema.UPRN] = addressDetails.uprn;
                if (addressDetails.buildingname != null)
                    address[AddressSchema.NAME] = addressDetails.buildingname;
                if (addressDetails.buildingnumber != null)
                    address[AddressSchema.PREMISES] = addressDetails.buildingnumber;// + "," + addressDetails.buildingname;
                if (addressDetails.street != null)
                    address[AddressSchema.STREET] = addressDetails.street;
                if (addressDetails.locality != null)
                    address[AddressSchema.LOCALITY] = addressDetails.locality;
                if (addressDetails.town != null)
                    address[AddressSchema.TOWN] = addressDetails.town;
                if (addressDetails.postcode != null)
                    address[AddressSchema.POSTCODE] = addressDetails.postcode;
                bool resultCompanyHouse;
                if (addressDetails.fromcompanieshouse != null)
                    if (Boolean.TryParse(addressDetails.fromcompanieshouse, out resultCompanyHouse))
                        address[AddressSchema.FROMCOMPANIESHOUSE] = resultCompanyHouse;
                if (addressDetails.country != null)
                {
                    var CountryRecord = from c in orgSvcContext.CreateQuery(AddressSchema.ENTITY)
                                        where ((string)c[AddressSchema.NAME]).ToLower().Contains((addressDetails.country.Trim().ToLower()))
                                        select new { CountryId = c.Id };
                    Guid countryGuid = CountryRecord != null && CountryRecord.FirstOrDefault() != null ? CountryRecord.FirstOrDefault().CountryId : Guid.Empty;
                    if (countryGuid != Guid.Empty)
                        address[AddressSchema.COUNTRY] = new EntityReference(AddressSchema.COUNTRY, countryGuid);
                }
                addressId = this.service.Create(address);
            }
            if (addressId != Guid.Empty)
            {
                Entity contactDetails = new Entity(ContactDetails.ENTITY);
                contactDetails[AddressSchema.ENTITY] = new EntityReference(AddressSchema.ENTITY, addressId);

                if (addressDetails.type != null)
                {
                    if (Enum.GetValues(typeof(AddressTypes)).Equals(addressDetails.type))
                    {
                        contactDetails[ContactDetails.ADDRESSTYPE] = new OptionSetValue((int)addressDetails.type);
                    }
                }

                contactDetails[ContactDetails.CUSTOMER] = Customer;
                Guid contactDetailId = this.service.Create(contactDetails);
            }

        }

    }
}