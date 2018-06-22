using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class ConnectContactResponse : ResponseCustomerMasterBase
    {
        public ConnectContactData data;
    }

    public class ConnectContactData : ResponseDataBase
    {
        public Guid toconnectionid { get; set; }
        public Guid fromconnectionid { get; set; }
    }
}
