using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class AccountData: ResponseDataBase
    {
        public Guid accountid { get; set; }
        public string uniquereference { get; set; }
    }
}
