using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Defra.CustomerMaster.Identity.Api.Model
{
    public class WebFaultException:Exception
    {
        
        public string ErrorMsg;

        public int HttpStatusCode;

        public WebFaultException(string errorMsg, int httpStatusCode)
        {
            ErrorMsg = errorMsg;
            HttpStatusCode = httpStatusCode;
        }
    }
}