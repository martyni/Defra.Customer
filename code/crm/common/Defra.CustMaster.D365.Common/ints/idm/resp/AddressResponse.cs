using Defra.CustMaster.D365.Common.Ints.Idm.Resp;
using System;


namespace Defra.CustMaster.D365.Common.Ints.Idm.resp
{
    public class AddressResponse:ResponseCustomerMasterBase
    {
        public AddressData data;
    }
    public class AddressData : ResponseDataBase
    {
        public Guid contactdetailsid { get; set; }
        public Guid addressid { get; set; }
    }
}
