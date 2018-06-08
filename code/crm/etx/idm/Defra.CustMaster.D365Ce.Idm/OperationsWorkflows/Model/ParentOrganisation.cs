using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Defra.CustMaster.D365Ce.Idm.OperationsWorkflows.Model
{
    [DataContract]
    public class ParentOrganisation
    {
        [DataMember]
        public String parentorganisationcrmid;
    }
}
