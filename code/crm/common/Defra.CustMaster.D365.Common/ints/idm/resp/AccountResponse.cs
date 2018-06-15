using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class AccountResponse: ResponseCustomerMasterBase
    {
        public AccountResponse()
        {
            this.data = new AccountData();
        }
        public AccountData data;
    }
}
