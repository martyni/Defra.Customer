using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class AccountResponse: ResponseCustomerMasterBase
    {
        public AccountResponse()
        {
            this.AccountData = new AccountData();
        }
        public AccountData AccountData;
    }
}
