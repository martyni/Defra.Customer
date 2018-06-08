using Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model
{
    [DataContract]
    public  class Account
    {
        [DataMember]
        public string name;
        [DataMember]
        public int type;
        [DataMember]
        public string crn;
        [DataMember]
        public string email;
        [DataMember]
        public Address address;
        [DataMember]
        public string telephone;
        [DataMember]
        public string hierarchylevel;
        [DataMember]
        public string validatedwithcompanieshouse;
        [DataMember]
        public ParentOrganisation parentorganisation;
        
    }
}
