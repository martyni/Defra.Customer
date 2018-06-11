using System;

namespace Defra.CustMaster.D365.Common.Ints.Idm.Resp
{
    public class Rootobject
    {
        public string program { get; set; }
        public string version { get; set; }
        public string release { get; set; }
        public DateTime datetime { get; set; }
        public long timestamp { get; set; }
        public string status { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }
}
