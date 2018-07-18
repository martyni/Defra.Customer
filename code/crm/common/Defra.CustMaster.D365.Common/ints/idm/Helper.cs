using Defra.CustMaster.D365.Common.Ints.Idm.resp;
using Defra.CustMaster.D365.Common.Schema.ExtEnums;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SCS = Defra.CustMaster.D365.Common.schema;

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

        public AddressData CreateAddress(Address addressDetails, EntityReference customer)
        {
            AddressData addressData = new AddressData();
            Guid addressId = Guid.Empty;
            Guid contactDetailId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(this.service);
            if (addressDetails.country.Trim().ToUpper() == "GBR")
            {
                if (addressDetails.uprn != null)
                {
                    tracingService.Trace("UPRN search:started..");
                    var propertyWithUPRN = from c in orgSvcContext.CreateQuery(SCS.Address.ENTITY)
                                           where ((string)c[SCS.Address.UPRN]).Equals((addressDetails.uprn.Trim()))
                                           select new { AddressId = c.Id };
                    addressId = propertyWithUPRN != null && propertyWithUPRN.FirstOrDefault() != null ? propertyWithUPRN.FirstOrDefault().AddressId : Guid.Empty;
                }

            }
            if (addressId == Guid.Empty && addressDetails.street != null && addressDetails.postcode != null && addressDetails.buildingnumber != null)
            {
                tracingService.Trace("postcode and street search:started");
                var propertyWithDuplicate = from c in orgSvcContext.CreateQuery(SCS.Address.ENTITY)
                                            where ((string)c[SCS.Address.STREET]).Equals((addressDetails.street.Trim())) && ((string)c[SCS.Address.POSTCODE]).Equals((addressDetails.postcode.Trim())) && ((string)c[SCS.Address.PREMISES]).Equals((addressDetails.buildingnumber.Trim()))
                                            select new { AddressId = c.Id };
                addressId = propertyWithDuplicate != null && propertyWithDuplicate.FirstOrDefault() != null ? propertyWithDuplicate.FirstOrDefault().AddressId : Guid.Empty;
            }
            if (addressId == Guid.Empty)
            {
                Entity address = new Entity(SCS.Address.ENTITY);
                if (addressDetails.country.Trim().ToUpper() == "GBR")
                {
                    if (addressDetails.uprn != null)
                        address[SCS.Address.UPRN] = addressDetails.uprn;
                }
                if (addressDetails.buildingname != null)
                    address[SCS.Address.NAME] = addressDetails.buildingname;
                if (addressDetails.buildingnumber != null)
                    address[SCS.Address.PREMISES] = addressDetails.buildingnumber;// + "," + addressDetails.buildingname;
                if (addressDetails.street != null)
                    address[SCS.Address.STREET] = addressDetails.street;
                if (addressDetails.locality != null)
                    address[SCS.Address.LOCALITY] = addressDetails.locality;
                if (addressDetails.town != null)
                    address[SCS.Address.TOWN] = addressDetails.town;
                if (addressDetails.postcode != null)
                {
                    if (addressDetails.country.Trim().ToUpper() == "GBR")
                    {
                        address[SCS.Address.POSTCODE] = addressDetails.postcode;
                    }
                    else
                    {
                        address[SCS.Address.INTERNATIONALPOSTCODE] = addressDetails.postcode;
                    }
                }

                bool resultedCompanyHouse;
                if (addressDetails.fromcompanieshouse != null)
                    if (Boolean.TryParse(addressDetails.fromcompanieshouse.ToString(), out resultedCompanyHouse))
                        address[SCS.Address.FROMCOMPANIESHOUSE] = resultedCompanyHouse;
                if (!string.IsNullOrEmpty(addressDetails.country))
                {
                    string countryValue = addressDetails.country.ToUpper();
                    tracingService.Trace("Country search started" + addressDetails.country);

                    var CountryRecord = from c in orgSvcContext.CreateQuery(SCS.Address.COUNTRY)
                                        where (((string)c["defra_isocodealpha3"]) == countryValue)
                                        select new { CountryId = c.Id };
                    Guid countryGuid = CountryRecord != null && CountryRecord.FirstOrDefault() != null ? CountryRecord.FirstOrDefault().CountryId : Guid.Empty;
                    tracingService.Trace("country found" + countryGuid);
                    if (countryGuid != Guid.Empty)
                        address[SCS.Address.COUNTRY] = new EntityReference(SCS.Address.COUNTRY, countryGuid);
                    else
                    {
                        tracingService.Trace("country not found:" + addressDetails.country);
                        throw new Exception("country not found:" + addressDetails.country);
                    }
                }
                tracingService.Trace("creating address started");
                addressId = this.service.Create(address);
            }
            if (addressId != Guid.Empty)
            {
                var contactDetailsWithType = from c in orgSvcContext.CreateQuery(SCS.ContactDetails.ENTITY)
                                             where ((string)c[SCS.ContactDetails.ADDRESSTYPE]).Equals((addressDetails.type)) && (((EntityReference)c[SCS.ContactDetails.CUSTOMER]).Id.Equals(customer.Id))
                                             select new { contactDetailsId = c.Id };
                contactDetailId = contactDetailsWithType != null && contactDetailsWithType.FirstOrDefault() != null ? contactDetailsWithType.FirstOrDefault().contactDetailsId : Guid.Empty;
                if (contactDetailId == Guid.Empty)
                {
                    Entity contactDetails = new Entity(SCS.ContactDetails.ENTITY);
                    contactDetails[SCS.Address.ENTITY] = new EntityReference(SCS.ContactDetails.ENTITY, addressId);
                    contactDetails[SCS.ContactDetails.ADDRESSTYPE] = new OptionSetValue((int)addressDetails.type);

                    contactDetails[SCS.ContactDetails.CUSTOMER] = customer;
                    contactDetailId = this.service.Create(contactDetails);
                }
                else
                {
                    tracingService.Trace("Contact details of same type already exist for this customer:" + contactDetailId);
                    throw new Exception("Contact details of same type already exist for this customer:" + contactDetailId);
                }
            }
            else
            {
                tracingService.Trace("Can not create address:");
            }
            addressData.addressid = addressId;
            addressData.contactdetailsid = contactDetailId;
            return addressData;

        }

        public void UpsertContactDetails(int type, string typeValue, EntityReference customer, bool isUpdate, bool isClear)
        {
            Guid contactDetailId = Guid.Empty;
            OrganizationServiceContext orgSvcContext = new OrganizationServiceContext(this.service);
            try
            {

                if (isUpdate || isClear)
                {
                    var contactDetailsWithType = from c in orgSvcContext.CreateQuery(SCS.ContactDetails.ENTITY)
                                                 where ((string)c[SCS.ContactDetails.ADDRESSTYPE]).Equals((type)) && (((EntityReference)c[SCS.ContactDetails.CUSTOMER]).Id.Equals(customer.Id)) && (int)c[SCS.ContactDetails.STATECODE] == 0
                                                 select new { contactDetailsId = c.Id };
                    contactDetailId = contactDetailsWithType != null && contactDetailsWithType.FirstOrDefault() != null ? contactDetailsWithType.FirstOrDefault().contactDetailsId : Guid.Empty;
                }
                Entity contactDetails = new Entity(SCS.ContactDetails.ENTITY);
                // contactDetails[SCS.Address.ENTITY] = new EntityReference(SCS.ContactDetails.ENTITY, addressId);
                contactDetails[SCS.ContactDetails.ADDRESSTYPE] = new OptionSetValue((int)type);
                if (Enum.IsDefined(typeof(EmailTypes), type))
                {
                    contactDetails[SCS.ContactDetails.EMAILADDRESS] = typeValue;
                }
                else if (Enum.IsDefined(typeof(PhoneTypes), type))
                {
                    contactDetails[SCS.ContactDetails.PHONE] = typeValue;
                }

                contactDetails[SCS.ContactDetails.CUSTOMER] = customer;
                if (contactDetailId == Guid.Empty)
                    contactDetailId = this.service.Create(contactDetails);
                else
                {
                    contactDetails.Id = contactDetailId;
                    if (isClear)
                        contactDetails[SCS.ContactDetails.STATECODE] = new OptionSetValue((int)1);
                    this.service.Update(contactDetails);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Validate<T>(T obj, out ICollection<ValidationResult> results)
        {
            results = new List<ValidationResult>();

            return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
        }

    }
}