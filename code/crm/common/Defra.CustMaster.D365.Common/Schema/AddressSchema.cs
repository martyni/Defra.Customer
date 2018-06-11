﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.schema
{
    public class AddressSchema
    {
        public const string ENTITY = "defra_address";
        public const string NAME = "defra_name";
        public const string UPRN = "defra_uprn";
        public const string PREMISES = "defra_premises";
        public const string STREET = "defra_street";
        public const string LOCALITY = "defra_locality";
        public const string TOWN = "defra_towntext";
        public const string COUNTRY = "defra_country";
        public const string POSTCODE = "defra_postcode";
        public const string FROMCOMPANIESHOUSE = "defra_fromcompanieshouse";
        public const string TACSACCEPTEDVERSION = "defra_tacsacceptedversion";
    }
    public class ContactDetails
    {
        public const string ENTITY = "defra_addressdetails";
        public const string NAME = "defra_name";
        public const string ADDRESSTYPE = "defra_addresstype";
        public const string CUSTOMER = "defra_customer";

    }
}
