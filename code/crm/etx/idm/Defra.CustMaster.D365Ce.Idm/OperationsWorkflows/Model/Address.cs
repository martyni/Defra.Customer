using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model
{
    [DataContract]
    public class Address
    {
        [DataMember]
        public string type;
        [DataMember]
        public string uprn;
        [DataMember]
        public string buildingnumber;
        [DataMember]
        public string buildingname;
        [DataMember]
        public string street;
        [DataMember]
        public string locality;
        [DataMember]
        public string town;
        [DataMember]
        public string postcode;
        [DataMember]
        public string county;
        [DataMember]
        public string fromcompanieshouse;       
    }
    public enum AddressTypes
    {
        RegisteredAddress = 1, BusinessActivityAddress = 2, CorrespondenceAddress = 3, BillingorPaymentAddress = 4 
    };
}
