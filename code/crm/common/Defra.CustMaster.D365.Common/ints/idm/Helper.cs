using Defra.CustMaster.D365.Common;
using Defra.CustMaster.D365.Common.Schema.ExtEnums;
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
                tracingService.Trace("UPRN search:started..");
                var propertyWithUPRN = from c in orgSvcContext.CreateQuery(schema.Address.ENTITY)
                                       where ((string)c[schema.Address.UPRN]).Equals((addressDetails.uprn.Trim()))
                                       select new { AddressId = c.Id };
                addressId = propertyWithUPRN != null && propertyWithUPRN.FirstOrDefault() != null ? propertyWithUPRN.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty && addressDetails.street != null && addressDetails.postcode != null && addressDetails.buildingnumber != null)
            {
                tracingService.Trace("postcode and street search:started");
                var propertyWithDuplicate = from c in orgSvcContext.CreateQuery(schema.Address.ENTITY)
                                            where ((string)c[schema.Address.STREET]).Equals((addressDetails.street.Trim())) && ((string)c[schema.Address.POSTCODE]).Equals((addressDetails.postcode.Trim())) && ((string)c[schema.Address.PREMISES]).Equals((addressDetails.buildingnumber.Trim()))
                                            select new { AddressId = c.Id };
                addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty)
            {
                Entity address = new Entity(schema.Address.ENTITY);
                if (addressDetails.uprn != null)
                    address[schema.Address.UPRN] = addressDetails.uprn;
                if (addressDetails.buildingname != null)
                    address[schema.Address.NAME] = addressDetails.buildingname;
                if (addressDetails.buildingnumber != null)
                    address[schema.Address.PREMISES] = addressDetails.buildingnumber;// + "," + addressDetails.buildingname;
                if (addressDetails.street != null)
                    address[schema.Address.STREET] = addressDetails.street;
                if (addressDetails.locality != null)
                    address[schema.Address.LOCALITY] = addressDetails.locality;
                if (addressDetails.town != null)
                    address[schema.Address.TOWN] = addressDetails.town;
                if (addressDetails.postcode != null)
                    address[schema.Address.POSTCODE] = addressDetails.postcode;
                bool resultCompanyHouse;
                if (addressDetails.fromcompanieshouse != null)
                    if (Boolean.TryParse(addressDetails.fromcompanieshouse, out resultCompanyHouse))
                        address[schema.Address.FROMCOMPANIESHOUSE] = resultCompanyHouse;
                if (!string.IsNullOrEmpty(addressDetails.country))
                {
                    tracingService.Trace("Country search started" + addressDetails.country);
                    var CountryRecord = from c in orgSvcContext.CreateQuery(schema.Address.COUNTRY)
                                        where (((string)c["defra_isocodealpha3"]).ToLower().Trim()).Equals((addressDetails.country.ToLower().Trim()))
                                        select new { CountryId = c.Id };
                    Guid countryGuid = CountryRecord != null && CountryRecord.FirstOrDefault() != null ? CountryRecord.FirstOrDefault().CountryId : Guid.Empty;
                    tracingService.Trace("country found" + countryGuid);
                    if (countryGuid != Guid.Empty)
                        address[schema.Address.COUNTRY] = new EntityReference(schema.Address.COUNTRY, countryGuid);
                }
                tracingService.Trace("creating address started");
                addressId = this.service.Create(address);
            }
            if (addressId != Guid.Empty)
            {
                Entity contactDetails = new Entity(schema.ContactDetails.ENTITY);
                contactDetails[schema.Address.ENTITY] = new EntityReference(schema.Address.ENTITY, addressId);


                //Check whether addressType is found in Dynamics defra_addresstypeEnum mapping
                string addressType = Enum.GetName(typeof(AddressTypes), addressDetails.type);

                defra_AddressType dynamicsAddressType = (defra_AddressType)Enum.Parse(typeof(defra_AddressType), addressType);
                contactDetails[schema.ContactDetails.ADDRESSTYPE] = new OptionSetValue((int)dynamicsAddressType);

                contactDetails[schema.ContactDetails.CUSTOMER] = Customer;
                Guid contactDetailId = this.service.Create(contactDetails);
            }

        }

    }
}