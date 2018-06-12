using System;
using System.Collections.Generic;
using System.Text;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class ContactData:ResponseDataBase
    {
        public Guid contactid { get; set; }
        public string uniquereference { get; set; }
    }
}
