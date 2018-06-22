using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.resp
{
    public class ConnectContactResponse : ResponseCustomerMasterBase
    {
        public ConnectContact data;
    }

    public class ConnectContact : ResponseDataBase
    {
        public Guid contactdetailsid { get; set; }
        public Guid addressid { get; set; }
    }
}
